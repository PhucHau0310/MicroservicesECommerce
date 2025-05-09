using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Models.DTOs.Res
{
    public record TokenRes(string AccessToken, string RefreshToken);
}
