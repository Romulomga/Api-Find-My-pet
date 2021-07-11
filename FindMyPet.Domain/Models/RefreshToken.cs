using System;
using System.Collections.Generic;
using System.Text;

namespace FindMyPet.Business.Models
{
    public class RefreshToken
    {
        public RefreshToken()
        {
            Id = Guid.NewGuid();
            Token = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public string Email { get; set; }
        public Guid Token { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}
