using AdotaFacil.Business.Models.Base;
using NetTopologySuite.Geometries;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdotaFacil.Business.Models
{
    public class Post : BaseEntity
    {
        public string Description { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
        public Pet Pet { get; set; }

        [Required(AllowEmptyStrings = false)]
        public MultiPolygon WktPolygon { get; set; }
    }
}
