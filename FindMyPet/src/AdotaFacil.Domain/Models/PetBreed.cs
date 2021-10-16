using AdotaFacil.Business.Models.Base;
using System;

namespace AdotaFacil.Business.Models
{
    public class PetBreed : BaseEntity
    {
        public string Name { get; set; }
        public Guid PetTypeId { get; set; }
        public PetType PetType { get; set; }
    }
}
