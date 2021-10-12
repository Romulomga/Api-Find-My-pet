using FindMyPet.Business.Models.Base;
using System.Collections.Generic;

namespace FindMyPet.Business.Models
{
    public class PetType: BaseEntity
    {
        public string Name { get; set; }
        public IEnumerable<Pet> Pets { get; set; }
        public IEnumerable<PetBreed> PetBreeds { get; set; }
    }
}
