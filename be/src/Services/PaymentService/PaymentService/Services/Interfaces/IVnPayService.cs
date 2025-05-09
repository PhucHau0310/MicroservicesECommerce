using Microsoft.AspNetCore.Http;
using PaymentService.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Services.Interfaces
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(VnPayReq request, string ipAddress);
        bool ValidateCallback(IQueryCollection collections);
    }
}
