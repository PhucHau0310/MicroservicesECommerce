using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Auth.Configuration
{
    public static class JwtConfiguration
    {
        public static void AddSharedJwtConfig(this IServiceCollection services, IConfiguration config)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
             .AddJwtBearer(options =>
             {
                 options.TokenValidationParameters = new TokenValidationParameters
                 {
                     ValidateIssuer = true,
                     ValidateAudience = true,
                     ValidateLifetime = true,
                     ValidateIssuerSigningKey = true,
                     ValidIssuer = config["Jwt:Issuer"],
                     ValidAudience = config["Jwt:Audience"],
                     IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]))
                 };

                 // Add this section to handle WebSocket authentication
                 options.Events = new JwtBearerEvents
                 {
                     OnMessageReceived = context =>
                     {
                         //var accessToken = context.Request.Query["access_token"];
                         var accessToken = context.Request.Headers["Authorization"]
                                        .ToString()
                                        .Replace("Bearer ", "");

                         if (string.IsNullOrEmpty(accessToken))
                         {
                             accessToken = context.Request.Query["access_token"];
                         }

                         var path = context.HttpContext.Request.Path;
                         if ((!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notificationhub")))
                         {
                             context.Token = accessToken;
                         }
                         return Task.CompletedTask;
                     }
                 };
             })
             .AddCookie()
             .AddGoogle(options =>
             {
                 options.ClientId = config["Authentication:Google:ClientId"];
                 options.ClientSecret = config["Authentication:Google:ClientSecret"];
                 options.CallbackPath = "/signin-google";
                 options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;


                 // Add more scopes to get more information from Google
                 options.Scope.Add("https://www.googleapis.com/auth/userinfo.profile"); // Get user's profile
                 options.Scope.Add("https://www.googleapis.com/auth/userinfo.email"); // Get user's email
                 options.Scope.Add("https://www.googleapis.com/auth/user.birthday.read");// Get user's birthday (if available)

                 options.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "given_name");
                 options.ClaimActions.MapJsonKey(ClaimTypes.Surname, "family_name");
                 options.ClaimActions.MapJsonKey(ClaimTypes.DateOfBirth, "birthday");
                 options.ClaimActions.MapJsonKey("profile_picture", "picture");

                 options.SaveTokens = true; // Save tokens for later use
             })
             .AddFacebook(options =>
             {
                 options.AppId = config["Authentication:Facebook:AppId"];
                 options.AppSecret = config["Authentication:Facebook:AppSecret"];
                 options.CallbackPath = "/signin-facebook";
                 options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;


                 // Add more scopes to get more information from Facebook
                 options.Scope.Add("email"); // Get user's email
                 options.Scope.Add("public_profile"); // Get user's profile

                 // Mapping claims from Facebook data
                 options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
                 options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
                 options.ClaimActions.MapJsonKey("profile_picture", "picture.data.url");

                 options.SaveTokens = true;  // Save tokens for later use
             });
        }
    }
}
