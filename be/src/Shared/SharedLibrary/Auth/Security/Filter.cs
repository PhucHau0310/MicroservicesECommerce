using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Auth.Security
{
    public class Filter : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly string[] _roles;

        public Filter(params string[] roles)
        {
            _roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (!user.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var path = context.HttpContext.Request.Path.Value.ToLower();

            if (_roles.Contains("Admin") && Endpoints.AdminEndpoints.Any(e => path.StartsWith(e.ToLower())))
            {
                if (!user.IsInRole("Admin"))
                {
                    context.Result = new ForbidResult();
                }
            }
            else if (_roles.Contains("User") && Endpoints.UserEndpoints.Any(e => path.StartsWith(e.ToLower())))
            {
                if (!user.IsInRole("User"))
                {
                    context.Result = new ForbidResult();
                }
            }
        }
    }
}
