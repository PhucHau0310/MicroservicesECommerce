using AuthService.Models.DTOs.Req;
using AuthService.Models.DTOs.Res;
using AuthService.Models.DTOs;
using AuthService.Services.Interfaces;
using AuthService.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AuthService.Data;

namespace AuthService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly AuthDbContext _context;
        private readonly IAuthService _authService;
        private readonly ITokenService _tokenService;
        private readonly IUserRepository _userRepository;

        public AuthController(ILogger<AuthController> logger, AuthDbContext context,
                              IAuthService authService, ITokenService tokenService,
                              IUserRepository userRepository)
        {
            _logger = logger;
            _context = context;
            _authService = authService;
            _tokenService = tokenService;
            _userRepository = userRepository;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterReq registerReq)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new Models.DTOs.Res.Response<string>
                {
                    Success = false,
                    Message = "Invalid data.",
                    Data = null
                });
            }

            var existingUsernameCheck = await _context.Users.AnyAsync(u => u.Username == registerReq.Username);
            if (existingUsernameCheck)
            {
                return BadRequest(new Response<string>
                {
                    Success = false,
                    Message = "Username already taken.",
                    Data = null
                });
            }

            var existingEmailCheck = await _context.Users.AnyAsync(u => u.Email == registerReq.Email);
            if (existingEmailCheck)
            {
                return BadRequest(new Response<string>
                {
                    Success = false,
                    Message = "Email already in use.",
                    Data = null
                });
            }

            var result = await _authService.RegisterAsync(registerReq);
            if (!result)
            {
                return BadRequest(new Response<string>
                {
                    Success = false,
                    Message = "Registration failed.",
                    Data = null
                });
            }

            return Ok(new Response<string>
            {
                Success = true,
                Message = "User registered successfully.",
                Data = null
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginAuthReq request)
        {
            TokenDto token = await _authService.LoginAsync(request);
            if (token == null)
            {
                return Unauthorized(new Response<string>
                {
                    Success = false,
                    Message = "Invalid credentials.",
                    Data = null
                });
            }

            return Ok(new Response<object>
            {
                Success = true,
                Message = "Login successfully.",
                Data = new
                {
                    AccessToken = token.AccessToken,
                    RefreshToken = token.RefreshToken,
                }
            });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshTokenAsync([FromBody] TokenDto request)
        {
            var tokenResponse = await _tokenService.RefreshTokenAsync(request.RefreshToken);
            if (tokenResponse == null)
            {
                return Unauthorized(new Response<string>
                {
                    Success = false,
                    Message = "Invalid refresh token.",
                    Data = null
                });
            }

            return Ok(new Response<TokenRes>
            {
                Success = true,
                Message = "Token refreshed",
                Data = tokenResponse
            });
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(Guid userId, string token)
        {
            var result = await _userRepository.ConfirmEmailAsync(userId, token);
            if (!result)
            {
                return BadRequest(new Response<string>
                {
                    Success = false,
                    Message = "Invalid confirmation link or token.",
                    Data = null
                });
            }

            return Ok(new Response<string>
            {
                Success = true,
                Message = "Email confirmed successfully.",
                Data = null
            });
        }
    }
}
