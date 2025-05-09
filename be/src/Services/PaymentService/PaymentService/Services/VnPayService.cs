using Microsoft.AspNetCore.Http;
using PaymentService.Configurations;
using PaymentService.Models.DTOs;
using PaymentService.Helper;
using PaymentService.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Services
{
    public class VnPayService : IVnPayService
    {
        private readonly VnPayConfig _vnPayConfig;

        public VnPayService(VnPayConfig vnPayConfig)
        {
            _vnPayConfig = vnPayConfig;
        }

        public string CreatePaymentUrl(VnPayReq request, string ipAddress)
        {
            var tick = DateTime.Now.Ticks.ToString();
            var vnpay = new VnPayLibrary();

            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", _vnPayConfig.TmnCode);
            vnpay.AddRequestData("vnp_Amount", ((long)request.Amount * 100).ToString());
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", ipAddress);
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", request.OrderInfo);
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", _vnPayConfig.ReturnUrl);
            vnpay.AddRequestData("vnp_TxnRef", tick);

            var paymentUrl = vnpay.CreateRequestUrl(_vnPayConfig.PaymentUrl, _vnPayConfig.HashSecret);
            return paymentUrl;
        }

        public bool ValidateCallback(IQueryCollection collections)
        {
            var vnpay = new VnPayLibrary();
            foreach (var (key, value) in collections)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(key, value.ToString());
                }
            }

            var orderId = Convert.ToInt64(collections["vnp_TxnRef"]);
            var vnPayTranId = Convert.ToInt64(collections["vnp_TransactionNo"]);
            var vnpResponseCode = collections["vnp_ResponseCode"].ToString();
            var vnpSecureHash = collections["vnp_SecureHash"].ToString();

            var checkSignature = vnpay.ValidateSignature(vnpSecureHash, _vnPayConfig.HashSecret);

            return checkSignature;
        }
    }
}
