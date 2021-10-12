using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace FindMyPet.Business.Models
{
    public class User : IdentityUser<Guid>
    {
        public string AvatarUrl { get; set; }
        public IEnumerable<Post> Posts { get; set; }
    }
}
