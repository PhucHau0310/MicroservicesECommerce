using AuthService.Models.DTOs.Res;
using AuthService.Repositories.Interfaces;
using AuthService.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IUserRepository _userRepository;

        public AccountController(ILogger<AccountController> logger, IUserRepository userRepository)
        {
            _logger = logger;
            _userRepository = userRepository;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userRepository.GetUsers();
            return Ok(new Response<List<User>>
            {
                Success = true,
                Message = "Get users successfully.",
                Data = users
            });
        }

        [HttpGet("detail")]
        public async Task<IActionResult> GetUserById(Guid userId)
        {
            var user = await _userRepository.GetUserById(userId);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(new Response<User>
            {
                Success = true,
                Message = $"Get user {userId} successfully.",
                Data = user
            });
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile(Guid userId)
        {
            var user = await _userRepository.GetUserById(userId);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(new Response<User>
            {
                Success = true,
                Message = $"Get user {userId} successfully.",
                Data = user
            });
        }

        [HttpDelete("delete")]
        public IActionResult DeleteUserById(Guid userId)
        {
            var result = _userRepository.DeleteUserById(userId);
            if (!result)
            {
                return NotFound();
            }
            return Ok(new Response<string>
            {
                Success = true,
                Message = $"Delete user {userId} successfully.",
                Data = null
            });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new Response<string>
                {
                    Success = false,
                    Message = "Email is required.",
                    Data = null
                });
            }

            try
            {
                // Call the service to handle forgot password logic
                var result = await _userRepository.ForgotPassword(email);

                if (!result)
                {
                    return NotFound(new Response<string>
                    {
                        Success = false,
                        Message = "Email not found in the system.",
                        Data = null
                    });
                }

                return Ok(new Response<string>
                {
                    Success = true,
                    Message = "Password reset email sent successfully.",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new Response<string>
                {
                    Success = false,
                    Message = "An error occurred while processing the request.",
                    Data = ex.Message
                });
            }
        }

        [HttpPost("verify-code")]
        public async Task<IActionResult> VerifyCode(string email, string code)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code))
            {
                return BadRequest(new Response<string>
                {
                    Success = false,
                    Message = "Email and Code is required.",
                    Data = null
                });
            }

            try
            {
                var result = await _userRepository.VerifyCode(email, code);

                if (!result)
                {
                    return BadRequest(new Response<string>
                    {
                        Success = false,
                        Message = "Code is invalid or expired",
                        Data = null
                    });
                }

                return Ok(new Response<string>
                {
                    Success = true,
                    Message = "Verify code is successfully.",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new Response<string>
                {
                    Success = false,
                    Message = "An error occurred while processing the verfiy code.",
                    Data = ex.Message
                });
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(string email, string newPass)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(newPass))
            {
                return BadRequest(new Response<string>
                {
                    Success = false,
                    Message = "Email and Newpass is required.",
                    Data = null
                });
            }

            try
            {
                var result = await _userRepository.ChangePass(email, newPass);

                if (!result)
                {
                    return NotFound(new Response<string>
                    {
                        Success = false,
                        Message = "Not found email.",
                        Data = null
                    });
                }

                return Ok(new Response<string>
                {
                    Success = true,
                    Message = "Change pass is successfully.",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new Response<string>
                {
                    Success = false,
                    Message = "An error occurred while processing the change pass.",
                    Data = ex.Message
                });
            }
        }
    }
}
