// AtlasRPG.Infrastructure/Data/Configurations/RunItemConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AtlasRPG.Core.Entities.Runs;

namespace AtlasRPG.Infrastructure.Data.Configurations
{
    public class RunItemConfiguration : IEntityTypeConfiguration<RunItem>
    {
        public void Configure(EntityTypeBuilder<RunItem> builder)
        {
            builder.ToTable("RunItems");

            builder.HasKey(ri => ri.Id);

            builder.Property(ri => ri.AcquiredAtTurn)
                .IsRequired();

            builder.Property(ri => ri.IsEquipped)
                .IsRequired()
                .HasDefaultValue(false);

            builder.HasIndex(ri => new { ri.RunId, ri.IsEquipped });

            // Relationships
            builder.HasOne(ri => ri.Run)
                .WithMany(r => r.Inventory)
                .HasForeignKey(ri => ri.RunId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ri => ri.Item)
                .WithMany()
                .HasForeignKey(ri => ri.ItemId)
                .OnDelete(DeleteBehavior.Restrict); // Items can be shared
        }
    }
}