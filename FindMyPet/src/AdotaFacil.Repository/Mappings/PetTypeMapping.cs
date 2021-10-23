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
    public class PetTypeMapping : IEntityTypeConfiguration<PetType>
    {
        public void Configure(EntityTypeBuilder<PetType> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(c => c.Name)
                .IsRequired()
                .HasColumnType("varchar(100)");

            builder.HasMany(f => f.PetBreeds)
                .WithOne(p => p.PetType)
                .HasForeignKey(p => p.PetTypeId);

            builder.HasMany(f => f.Pets)
                .WithOne(p => p.PetType)
                .HasForeignKey(p => p.PetTypeId);

            builder.ToTable("PetTypes");
        }
    }
}
