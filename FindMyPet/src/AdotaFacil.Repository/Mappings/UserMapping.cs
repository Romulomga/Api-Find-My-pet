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
    public class UserMapping : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(c => c.AvatarUrl)
                .HasColumnType("varchar(1000)");

            builder.HasMany(f => f.Posts)
                .WithOne(p => p.User)
                .HasForeignKey(p => p.UserId);
        }
    }
}
