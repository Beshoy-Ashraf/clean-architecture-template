using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
      public void Configure(EntityTypeBuilder<RefreshToken> builder)
      {

            builder.HasKey(i => i.Id);

            builder.HasOne(x => x.User)
                  .WithMany(x => x.RefreshTokens)
                  .HasForeignKey(x => x.UserId);

      }
}