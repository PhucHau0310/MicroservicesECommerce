﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Models.DTOs
{
    public record TokenDto(string AccessToken, string RefreshToken);
}
