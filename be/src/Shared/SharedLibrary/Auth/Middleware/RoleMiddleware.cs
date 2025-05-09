using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SharedLibrary.Auth.Security;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SharedLibrary.Auth.Middleware
{
    public class RoleMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RoleMiddleware> _logger;

        public RoleMiddleware(RequestDelegate next, ILogger<RoleMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public class RefreshTokenResponse
        {
            public string AccessToken { get; set; }
            public string RefreshToken { get; set; }
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Token is null or empty");
                return null;
            }

            _logger.LogInformation($"Validating token: {token}");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("90eb037cf2eb7c75e4253be42f4c23b35a63cc7922f5d6b2a1a5582e35e37b91");

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = "https://localhost:6061",
                ValidateAudience = true,
                ValidAudience = "https://localhost:6061",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);
                _logger.LogInformation($"Token validated successfully: {principal.Identity.Name}");
                _logger.LogInformation($"Principal.Identity.IsAuthenticated: {principal.Identity.IsAuthenticated}");

                return principal;
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogError($"Token validation failed: {ex.Message}");
                return null;
            }
        }

        public async Task InvokeAsync(HttpContext context)
        {
            _logger.LogInformation("========== Starting RoleMiddleware ==========");

            // Skip favicon requests
            if (context.Request.Path == "/favicon.ico")
            {
                _logger.LogInformation("Skipping favicon request");
                await _next(context);
                return;
            }

            var path = context.Request.Path.Value.ToLower();
            _logger.LogInformation($"Processing request for path: {path}");

            // Check if it's a public endpoint
            if (Endpoints.PublicEndpoints.Any(e => path.StartsWith(e)))
            {
                _logger.LogInformation($"Allowing access to public endpoint: {path}");
                await _next(context);
                return;
            }

            // Skip auth check for authentication endpoints
            if (!context.User.Identity.IsAuthenticated)
            {
                _logger.LogWarning($"Unauthorized access attempt to: {path}");
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }

            using (var scope = context.RequestServices.CreateScope())
            {
                var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                _logger.LogInformation($"Authorization header: {context.Request.Headers["Authorization"]}");

                if (string.IsNullOrEmpty(token))
                {
                    token = context.Request.Query["access_token"];
                    _logger.LogInformation("No token in Authorization header, trying to get from query string.");
                }

                _logger.LogInformation($"Token: {(string.IsNullOrEmpty(token) ? "NULL" : "FOUND")}");

                if (!string.IsNullOrEmpty(token))
                {
                    _logger.LogInformation("Starting validate token");
                    var principal = ValidateToken(token);

                    // after validate token
                    _logger.LogInformation($"Is authenticated: {context.User?.Identity?.IsAuthenticated}");
                    if (context.User?.Identity?.IsAuthenticated == true)
                    {
                        _logger.LogInformation($"User roles: {string.Join(", ", context.User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value))}");
                    }

                    if (principal != null)
                    {
                        _logger.LogInformation("Valid token with principal");
                        context.User = principal; // asign User into HttpContext
                    }
                    else
                    {
                        _logger.LogWarning("Token validation failed - invalid token");
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsync("Unauthorized - Invalid token");
                        return;
                    }
                }
                else
                {
                    _logger.LogWarning("No token provided.");
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Unauthorized - No token provided");
                    return;
                }

                var expClaim = context.User.FindFirst("exp");
                if (expClaim != null && long.TryParse(expClaim.Value, out var expSeconds))
                {
                    var tokenExpiryDate = DateTimeOffset.FromUnixTimeSeconds(expSeconds).UtcDateTime;
                    if (tokenExpiryDate < DateTime.UtcNow)
                    {
                        var refreshToken = context.Request.Headers["X-Refresh-Token"].ToString();

                        if (string.IsNullOrEmpty(refreshToken))
                        {
                            _logger.LogWarning("Refresh token is missing in request headers.");
                            context.Response.StatusCode = 401;
                            await context.Response.WriteAsync("Unauthorized - Refresh token required");
                            return;
                        }

                        var httpClient = new HttpClient();
                        var requestUri = "https://localhost:4041/gateway/auth/refresh-token";

                        var requestBody = new { RefreshToken = refreshToken, AccessToken = "" };
                        var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                        var response = await httpClient.PostAsync(requestUri, jsonContent);
                        if (!response.IsSuccessStatusCode)
                        {
                            _logger.LogWarning("Failed to refresh access token.");
                            context.Response.StatusCode = 401;
                            await context.Response.WriteAsync("Unauthorized - Invalid refresh token");
                            return;
                        }

                        var responseContent = await response.Content.ReadAsStringAsync();
                        var refreshResponse = JsonSerializer.Deserialize<RefreshTokenResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (refreshResponse == null || string.IsNullOrEmpty(refreshResponse.AccessToken))
                        {
                            _logger.LogWarning("Failed to parse refreshed access token.");
                            context.Response.StatusCode = 401;
                            await context.Response.WriteAsync("Unauthorized - Invalid refresh token");
                            return;
                        }

                        context.Response.Headers["X-New-Access-Token"] = refreshResponse.AccessToken;
                        _logger.LogInformation("Access token refreshed successfully.");
                    }
                }

                bool isAdminEndpoint = Endpoints.AdminEndpoints.Any(e => path.StartsWith(e));
                bool isUserEndpoint = Endpoints.UserEndpoints.Any(e => path.StartsWith(e));

                bool hasAccess = false;

                if (isAdminEndpoint && context.User.IsInRole("Admin"))
                    hasAccess = true;

                if (isUserEndpoint && context.User.IsInRole("User"))
                    hasAccess = true;

                if (!hasAccess)
                {
                    _logger.LogWarning($"Forbidden access attempt to role-specific endpoint: {path}");
                    context.Response.StatusCode = 403; // Forbidden
                    await context.Response.WriteAsync("Forbidden - Insufficient role permissions");
                    return;
                }
            }

            await _next(context);
        }
    }
}
