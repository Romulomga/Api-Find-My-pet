using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace FindMyPet.Domain
{
    public class User: IdentityUser<long>
    {
        public string AvatarUrl { get; set; }
        public List<UserRole> UserRoles { get; set; }
    }
}
