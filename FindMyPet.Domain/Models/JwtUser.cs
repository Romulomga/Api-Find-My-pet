using FindMyPet.Business.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace FindMyPet.Business.Models
{
    public class JwtUser : IJwtUser
    {
        private readonly IHttpContextAccessor Accessor;

        public JwtUser(IHttpContextAccessor accessor = null)
        {
            this.Accessor = accessor;
        }

        public Guid GetUserId()
        {
            return Guid.Parse(Accessor.HttpContext.User.GetUserId());
        }
    }

    public static class ClaimsPrincipalExtensions
    {
        public static string GetUserId(this ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentException(nameof(principal));
            }

            var claim = principal.FindFirst(ClaimTypes.NameIdentifier);
            return claim?.Value;
        }
    }
}
