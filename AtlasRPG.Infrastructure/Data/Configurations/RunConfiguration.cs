// AtlasRPG.Infrastructure/Data/Configurations/RunConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AtlasRPG.Core.Entities.Runs;

namespace AtlasRPG.Infrastructure.Data.Configurations
{
    public class RunConfiguration : IEntityTypeConfiguration<Run>
    {
        public void Configure(EntityTypeBuilder<Run> builder)
        {
            builder.ToTable("Runs");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(r => r.RunHash)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(r => r.UserId);
            builder.HasIndex(r => r.RunHash).IsUnique();
            builder.HasIndex(r => new { r.UserId, r.IsActive });

            // Relationships
            builder.HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(r => r.Turns)
                .WithOne(t => t.Run)
                .HasForeignKey(t => t.RunId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(r => r.Inventory)
                .WithOne(i => i.Run)
                .HasForeignKey(i => i.RunId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.Equipment)
                .WithOne(e => e.Run)
                .HasForeignKey<RunEquipment>(e => e.RunId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}