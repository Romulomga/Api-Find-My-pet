using AdotaFacil.Business.Models.Base;
using System;
using System.ComponentModel.DataAnnotations;

namespace AdotaFacil.Business.Models
{
    public class PetBreed : BaseEntity
    {
        [Required(AllowEmptyStrings = false)]
        public string Name { get; set; }
        public Guid PetTypeId { get; set; }
        public PetType PetType { get; set; }
    }
}
