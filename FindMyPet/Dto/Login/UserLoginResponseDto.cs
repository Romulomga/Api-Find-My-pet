using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FindMyPet.Dto
{
    public class UserLoginResponseDto
    {
        public UserDto User { get; set; }
        public string Token { get; set; }
    }
    public class UserDto
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string AvatarUrl { get; set; }
    }
}
