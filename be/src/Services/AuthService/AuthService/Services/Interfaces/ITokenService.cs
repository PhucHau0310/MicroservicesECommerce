using AuthService.Models.DTOs.Res;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Services.Interfaces
{
    public interface ITokenService
    {
        Task<TokenRes> RefreshTokenAsync(string token);
        string GenerateAccessToken(IEnumerable<Claim> claims);
        string GenerateRefreshToken();
        ClaimsPrincipal ValidateToken(string token);
        Task<TokenRes> ExchangeCodeForTokensAsync(string code);
    }
}
