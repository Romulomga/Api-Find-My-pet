using AdotaFacil.Business.Models.Base;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AdotaFacil.Business.Models
{
    public class PetType : BaseEntity
    {
        [Required(AllowEmptyStrings = false)]
        public string Name { get; set; }
        public IEnumerable<Pet> Pets { get; set; }
        public IEnumerable<PetBreed> PetBreeds { get; set; }
    }
}
