using FindMyPet.Business.Models.Base;
using System;

namespace FindMyPet.Business.Models
{
    public class PetBreed: BaseEntity
    {
        public string Name { get; set; }
        public Guid PetTypeId { get; set; }
        public PetType PetType { get; set; }
    }
}
