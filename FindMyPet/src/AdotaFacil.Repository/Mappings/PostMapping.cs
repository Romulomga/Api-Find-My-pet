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
    public class PostMapping : IEntityTypeConfiguration<Post>
    {
        public void Configure(EntityTypeBuilder<Post> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(c => c.Description)
                .HasColumnType("varchar(1000)");

            builder.Property(c => c.WktPolygon)
                .IsRequired()
                .HasColumnType("geography");

            builder.HasOne(p => p.Pet)
                .WithOne(p => p.Post);

            builder.ToTable("Posts");
        }
    }
}
