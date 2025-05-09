using AuthService.Models.DTOs.Res;
using AuthService.Repositories.Interfaces;
using AuthService.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Abstractions;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace AuthService.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<TokenService> _logger;  
        public TokenService(IConfiguration configuration, IUserRepository userRepository, ILogger<TokenService> logger)
        {
            _configuration = configuration;
            _userRepository = userRepository;
            _logger = logger;
        }
        public string GenerateAccessToken(IEnumerable<Claim> claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        public async Task<TokenRes> RefreshTokenAsync(string token)
        {
            var refreshToken = await _userRepository.GetRefreshTokenByToken(token);
            if (refreshToken == null || refreshToken.Expiration < DateTime.UtcNow || refreshToken.IsRevoked)
            {
                return null;
            }

            await _userRepository.RevokeRefreshToken(refreshToken);

            var user = await _userRepository.GetUserById(refreshToken.UserId);

            if (user == null)
            {
                return null;
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                //new Claim(ClaimTypes.Role, user.Roles.RoleType.ToString())
            };


            if (user.Roles != null)
            {
                claims.Add(new Claim(ClaimTypes.Role, user.Roles.RoleType.ToString()));
            }

            var newAccessToken = GenerateAccessToken(claims);
            var newRefreshToken = GenerateRefreshToken();

            // save new token
            await _userRepository.SaveRefreshToken(user.Id, newRefreshToken);

            return new TokenRes(newAccessToken, newRefreshToken);
        }

        public ClaimsPrincipal ValidateToken(string token)
        {
            _logger.LogInformation($"Validating token: {token}");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);
                _logger.LogInformation($"Token validated successfully: {principal.Identity.Name}");
                _logger.LogInformation($"Token validated successfully: {principal.Identity.IsAuthenticated}");

                return principal;
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogError($"Token validation failed: {ex.Message}");
                return null;
            }
        }

        public async Task<TokenRes> ExchangeCodeForTokensAsync(string code)
        {
            var httpClient = new HttpClient();
            var tokenRequest = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("client_id", "113856962829-mchf7l61opti8866cr611v2v00tcqcdj.apps.googleusercontent.com"),
                new KeyValuePair<string, string>("client_secret", "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"),
                new KeyValuePair<string, string>("redirect_uri", "https://localhost:7036/api/Auth/google-callback"),
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
            });

            var response = await httpClient.PostAsync("https://oauth2.googleapis.com/token", tokenRequest);
            var responseString = await response.Content.ReadAsStringAsync();

            var tokenResponse = JsonConvert.DeserializeObject<TokenRes>(responseString);
            return tokenResponse;
        }
    }
}
