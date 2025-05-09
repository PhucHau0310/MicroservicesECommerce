using AuthService.Models.DTOs.Req;
using AuthService.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AuthService.Models.DTOs.Res;
using AuthService.Models.DTOs;
using AuthService.Services.Interfaces;

namespace AuthService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OAuthController : ControllerBase
    {
        private readonly ILogger<OAuthController> _logger;
        private readonly IOAuthService _oAuthService;
        private readonly ITokenService _tokenService;
        public OAuthController(ILogger<OAuthController> logger, IOAuthService oAuthService, ITokenService tokenService)
        {
            _logger = logger;
            _oAuthService = oAuthService;
            _tokenService = tokenService;   
        }

        [HttpGet("login-google")]
        public IActionResult LoginWithGoogle()
        {
            var properties = new AuthenticationProperties { RedirectUri = "/api/oauth/google-response" };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google-response")]
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if (!result.Succeeded)
            {
                return BadRequest("Google authentication failed.");
            }

            var additionalInfo = new LoginOAuthReq
            {
                Username = result.Principal.FindFirst(ClaimTypes.Name)?.Value,
                Email = result.Principal.FindFirst(ClaimTypes.Email)?.Value,
                FirstName = result.Principal.FindFirst(ClaimTypes.GivenName)?.Value,
                LastName = result.Principal.FindFirst(ClaimTypes.Surname)?.Value,
                AvatarUrl = result.Principal.FindFirst("profile_picture")?.Value
            };

            var tokenDto = await _oAuthService.LoginWithGoogleAsync(additionalInfo);
            if (tokenDto == null)
            {
                return Unauthorized("Authentication failed, unable to generate tokens.");
            }

            //return Ok(tokenDto);
            return Ok(new Response<object>
            {
                Success = true,
                Message = "Login with google successfully.",
                Data = new
                {
                    AccessToken = tokenDto.AccessToken,
                    RefreshToken = tokenDto.RefreshToken
                }
            });
        }

        [HttpGet("login-facebook")]
        public IActionResult LoginWithFacebook()
        {
            var properties = new AuthenticationProperties { RedirectUri = "/api/oauth/facebook-response" };
            return Challenge(properties, FacebookDefaults.AuthenticationScheme);
        }

        [HttpGet("facebook-response")]
        public async Task<IActionResult> FacebookResponse()
        {
            var result = await HttpContext.AuthenticateAsync(FacebookDefaults.AuthenticationScheme);
            if (!result.Succeeded)
            {
                return BadRequest("Facebook authentication failed.");
            }

            var additionalInfo = new LoginOAuthReq
            {
                Username = result.Principal.FindFirst(ClaimTypes.Name)?.Value,
                Email = result.Principal.FindFirst(ClaimTypes.Email)?.Value,
                FirstName = result.Principal.FindFirst(ClaimTypes.GivenName)?.Value,
                LastName = result.Principal.FindFirst(ClaimTypes.Surname)?.Value,
                AvatarUrl = result.Principal.FindFirst("profile_picture")?.Value
            };

            var tokenDto = await _oAuthService.LoginWithFacebookAsync(additionalInfo);
            if (tokenDto == null)
            {
                return Unauthorized("Authentication failed, unable to generate tokens.");
            }

            //return Ok(tokenDto);
            return Ok(new Response<object>
            {
                Success = true,
                Message = "Login with facebook successfully.",
                Data = new {
                    AccessToken = tokenDto.AccessToken,
                    RefreshToken = tokenDto.RefreshToken
                }
            });
        }
    }
}
