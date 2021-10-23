using AdotaFacil.Business.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdotaFacil.Repository.Mappings
{
    public class PetBreedMapping : IEntityTypeConfiguration<PetBreed>
    {
        public void Configure(EntityTypeBuilder<PetBreed> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(c => c.Name)
                .IsRequired()
                .HasColumnType("varchar(100)");

            builder.ToTable("PetBreeds");
        }
    }
}
