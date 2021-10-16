using AdotaFacil.Business.Models.Base;
using NetTopologySuite.Geometries;
using System;

namespace AdotaFacil.Business.Models
{
    public class Post : BaseEntity
    {
        public string Description { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
        public Pet Pet { get; set; }
        public MultiPolygon WktPolygon { get; set; }
    }
}
