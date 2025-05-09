using AuthService.Models.DTOs.Req;
using AuthService.Models.DTOs;
using AuthService.Repositories.Interfaces;
using AuthService.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace AuthService.Services
{
    public class OAuthService : IOAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;

        public OAuthService(IUserRepository userRepository, ITokenService tokenService)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
        }

        public async Task<TokenDto> LoginWithFacebookAsync(LoginOAuthReq loginReq)
        {
            var userRegistered = await _userRepository.RegisterOrUpdateUser(
                loginReq.Username, 
                loginReq.Email, 
                loginReq.FirstName, 
                loginReq.LastName, 
                loginReq.AvatarUrl, 
            3);

            if (userRegistered == null)
            {
                throw new Exception("User registration or update failed. userRegistered is null.");
            }

            // Check user roles
            if (userRegistered.Roles == null)
            {
                throw new Exception($"User {userRegistered.Username} has no assigned role.");
            }

            // Check if user already has a valid RefreshToken
            var existingToken = await _userRepository.GetRefreshToken(userRegistered.Id);

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
                await _userRepository.SaveRefreshToken(userRegistered.Id, refreshToken);
            }

            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, userRegistered.Username),
                    new Claim(ClaimTypes.NameIdentifier, userRegistered.Id.ToString()),
                    new Claim(ClaimTypes.Email, userRegistered.Email),
                    new Claim(ClaimTypes.Role, userRegistered.Roles.RoleType.ToString())
                };

            var accessToken = _tokenService.GenerateAccessToken(claims);
            return new TokenDto(accessToken, refreshToken);
        }

        public async Task<TokenDto> LoginWithGoogleAsync(LoginOAuthReq loginReq)
        {
            var userRegistered = await _userRepository.RegisterOrUpdateUser(
                loginReq.Username,
                loginReq.Email,
                loginReq.FirstName,
                loginReq.LastName,
                loginReq.AvatarUrl,
            2);

            if (userRegistered == null)
            {
                throw new Exception("User registration or update failed. userRegistered is null.");
            }

            // Check user roles
            if (userRegistered.Roles == null)
            {
                throw new Exception($"User {userRegistered.Username} has no assigned role.");
            }

            // Check if user already has a valid RefreshToken
            var existingToken = await _userRepository.GetRefreshToken(userRegistered.Id);

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
                await _userRepository.SaveRefreshToken(userRegistered.Id, refreshToken);
            }

            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, userRegistered.Username),
                    new Claim(ClaimTypes.NameIdentifier, userRegistered.Id.ToString()),
                    new Claim(ClaimTypes.Email, userRegistered.Email),
                    new Claim(ClaimTypes.Role, userRegistered.Roles.RoleType.ToString())
                };

            var accessToken = _tokenService.GenerateAccessToken(claims);
            return new TokenDto(accessToken, refreshToken);
        }
    }
}
