using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FindMyPet.Models
{
    public class User: IdentityUser<long>
    {
        public string AvatarUrl { get; set; }
    }
}
