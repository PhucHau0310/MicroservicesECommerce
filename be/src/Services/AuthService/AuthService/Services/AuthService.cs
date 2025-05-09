using AuthService.Models.DTOs.Req;
using AuthService.Models.DTOs;
using AuthService.Repositories.Interfaces;
using AuthService.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AuthService.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IUserRepository userRepository, ITokenService tokenService, ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<TokenDto> LoginAsync(LoginAuthReq loginReq)
        {
            if (loginReq.Email == null)
            {
                _logger.LogError("Email is required");
                return null;
            }

            if (loginReq.Password == null)
            {
                _logger.LogError("Password is required");
                return null;
            }

            var user = _userRepository.Authenticate(loginReq.Email, loginReq.Password);
            if (user == null) return null;

            // Check if user already has a valid RefreshToken
            var existingToken = await _userRepository.GetRefreshToken(user.Id);

            string refreshToken;
            if (existingToken != null)
            {
                // Use the existing RefreshToken if it's still valid
                refreshToken = existingToken.RefreshToken;
            }
            else
            {
                // if user doesn't have a valid RefreshToken, generate a new one
                refreshToken = _tokenService.GenerateRefreshToken();
                await _userRepository.SaveRefreshToken(user.Id, refreshToken);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                //new Claim(ClaimTypes.Role, user.Roles.RoleType.ToString()),
            };

            _logger.LogInformation($"User Role: {user.Roles?.RoleType}");

            if (user.Roles != null)
            {
                claims.Add(new Claim(ClaimTypes.Role, user.Roles.RoleType.ToString()));
            }

            var accessToken = _tokenService.GenerateAccessToken(claims);

            return new TokenDto(accessToken, refreshToken);
        }

        public async Task<bool> RegisterAsync(RegisterReq registerReq)
        {
            return await _userRepository.RegisterAsync(registerReq.Email, 
                registerReq.Username, registerReq.FirstName, 
                registerReq.LastName, registerReq.Password);
        }
    }
}
