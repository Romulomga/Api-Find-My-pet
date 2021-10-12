using FindMyPet.Business.Models.Base;
using System;

namespace FindMyPet.Business.Models
{
    public class Post: BaseEntity
    {
        public string Description { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
        public Pet Pet { get; set; }
    }
}
