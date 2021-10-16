using AdotaFacil.Business.Models.Base;
using System.Collections.Generic;

namespace AdotaFacil.Business.Models
{
    public class PetGender : BaseEntity
    {
        public string Name { get; set; }
        public IEnumerable<Pet> Pets { get; set; }
    }
}
