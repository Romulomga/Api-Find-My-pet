using FindMyPet.Business.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace FindMyPet
{
    public class TokenUser : ITokenUser
    {
        private readonly IHttpContextAccessor _accessor;

        public TokenUser(IHttpContextAccessor Accessor = null)
        {
            _accessor = Accessor;
        }

        public string Name => _accessor.HttpContext.User.Identity.Name;

        public long? GetUserId()
        {
            return IsAuthenticated() ? Convert.ToInt64(_accessor.HttpContext.User.GetUserId()) : null;
        }

        public string? GetUserEmail()
        {
            return IsAuthenticated() ? _accessor.HttpContext.User.GetUserEmail() : null;
        }

        public bool IsAuthenticated()
        {
            return _accessor.HttpContext.User.Identity.IsAuthenticated;
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

        public static string GetUserEmail(this ClaimsPrincipal Principal)
        {
            if (Principal == null)
            {
                throw new ArgumentException(nameof(Principal));
            }

            var claim = Principal.FindFirst(ClaimTypes.Email);
            return claim?.Value;
        }
    }
}
