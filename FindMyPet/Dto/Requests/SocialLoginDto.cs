using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FindMyPet.Dto.Requests
{
    public class SocialLoginDto
    {
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        public string AccessToken { get; set; }
        public string UserName { get; set; }

        [EmailAddress(ErrorMessage = "O campo {0} está em formato inválido")]
        public string Email { get; set; }


    }
}
