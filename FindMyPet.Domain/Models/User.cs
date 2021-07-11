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
    public class User : IdentityUser<Guid>
    {
        public string AvatarUrl { get; set; }
    }
}
