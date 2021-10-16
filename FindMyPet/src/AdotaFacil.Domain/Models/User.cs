using AdotaFacil.Business.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace AdotaFacil.Business.Models
{
    public class User : IdentityUser<Guid>
    {
        public string AvatarUrl { get; set; }
        public IEnumerable<Post> Posts { get; set; }
    }
}
