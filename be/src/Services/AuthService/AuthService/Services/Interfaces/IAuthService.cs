using AuthService.Models.DTOs.Req;
using AuthService.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Services.Interfaces
{
    public interface IAuthService
    {
        Task<TokenDto> LoginAsync(LoginAuthReq loginReq);
        Task<bool> RegisterAsync(RegisterReq registerReq);
    }
}
