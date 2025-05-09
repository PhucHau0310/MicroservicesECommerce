using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Models.DTOs
{
    public class VnPayReq
    {
        public decimal Amount { get; set; }
        public required string OrderInfo { get; set; }  
    }
}
