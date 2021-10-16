using AdotaFacil.Business.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;

namespace AdotaFacil.Business.Models
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
