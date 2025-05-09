using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Auth.Security
{
    public static class Endpoints
    {
        public static readonly string[] PublicEndpoints =
        {
            "/api/auth/login",
            "/api/auth/register",
            "/api/auth/confirm-email",
            "/api/auth/refresh-token",

            "/api/oauth/login-google",
            "/api/oauth/google-response",
            "/api/oauth/login-facebook",
            "/api/oauth/facebook-response",
            "/api/oauth/refresh-token",

            "/api/product/all",
            "/api/product/detail",

            "/api/category/all",
            "/api/category/detail",

            "/api/payment/vnpay-return",
            "/notificationhub/negotiate",

            "/payment.paymentgrpc/processpayment",
            "/order.ordergrpc/updateorderstatus",

            "/api/stock/detail",
            "/api/stock/check",
        };

        public static readonly string[] AdminEndpoints =
        {
            "/api/account/all",
            "/api/account/detail",
            "/api/account/delete",
            "/api/account/profile",
            "/api/account/forgot-password",
            "/api/account/verify-code",
            "/api/account/reset-password",

            "/api/product/create",
            "/api/product/update",
            "/api/product/delete",

            "/api/category/create",
            "/api/category/update",
            "/api/category/delete",

            "/api/warehouse/all",
            "/api/warehouse/detail",
            "/api/warehouse/create",
            "/api/warehouse/update",
            "/api/warehouse/delete",

            "/api/stock/create",
            "/api/stock/update",
            "/api/stock/delete",

            "/api/order/all",
            "/api/order/detail",
            "/api/order/update",
            "/api/order/delete",
            "/api/order/complete",
            "/api/order/cancel",

            "/api/review/create",
            "/api/review/delete",
            "/notificationhub",
        };

        public static readonly string[] UserEndpoints =
        {
            "/api/account/forgot-password",
            "/api/account/verify-code",
            "/api/account/reset-password",
            "/api/account/profile",

            "/api/order/create",
            "/api/cart/detail",
            "/api/cart/add",
            "/api/cart/clear",
            "/api/cart/update",
            "/api/cart/delete/productItem",
            "/api/payment/vnpay",
            "/api/review/detail",
            "/api/review/detail/id",
            "/api/notification/send",
            "/api/notification/unread",
            "/api/notification/read",
            "/notificationhub",
        };
    }
}
