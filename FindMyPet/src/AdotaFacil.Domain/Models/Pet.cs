using AdotaFacil.Business.Models.Base;
using System;

namespace AdotaFacil.Business.Models
{
    public class Pet : BaseEntity
    {
        public string Name { get; set; }
        public Guid PetTypeId { get; set; }
        public PetType PetType { get; set; }
        public Guid PetGenderId { get; set; }
        public PetGender PetGender { get; set; }
        public Guid PostId { get; set; }
        public Post Post { get; set; }
    }
}
