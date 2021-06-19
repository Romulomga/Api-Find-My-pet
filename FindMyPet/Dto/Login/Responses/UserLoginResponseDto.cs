using FindMyPet.Dto.Login.Responses;
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
}
