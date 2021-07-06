using FindMyPet.Business.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FindMyPet.Business.Models
{
    public class User: IdentityUser<long>, IUser
    {
        public string AvatarUrl { get; set; }

        private readonly IHttpContextAccessor Accessor;

        public User(IHttpContextAccessor Accessor = null)
        {
            this.Accessor = Accessor;
        }

        public long GetUserId()
        {
            return Convert.ToInt64(Accessor.HttpContext.User.GetUserId());
        }
    }

    public static class ClaimsPrincipalExtensions
    {
        public static string GetUserId(this ClaimsPrincipal Principal)
        {
            if (Principal == null)
            {
                throw new ArgumentException(nameof(Principal));
            }

            var claim = Principal.FindFirst(ClaimTypes.NameIdentifier);
            return claim?.Value;
        }
    }
}
