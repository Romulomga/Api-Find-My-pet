using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FindMyPet.Dto.Login
{
    public class SocialLoginDto
    {
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        public string AccessToken { get; set; }
        public string UserName { get; set; }

    }
}
