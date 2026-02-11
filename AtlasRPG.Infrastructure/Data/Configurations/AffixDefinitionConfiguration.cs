// AtlasRPG.Infrastructure/Data/Configurations/AffixDefinitionConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AtlasRPG.Core.Entities.Items;

namespace AtlasRPG.Infrastructure.Data.Configurations
{
    public class AffixDefinitionConfiguration : IEntityTypeConfiguration<AffixDefinition>
    {
        public void Configure(EntityTypeBuilder<AffixDefinition> builder)
        {
            builder.ToTable("AffixDefinitions");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.AffixKey)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(a => a.DisplayName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(a => a.AllowedSlots)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(a => a.Tier1Min).HasPrecision(18, 4);
            builder.Property(a => a.Tier1Max).HasPrecision(18, 4);
            builder.Property(a => a.Tier2Min).HasPrecision(18, 4);
            builder.Property(a => a.Tier2Max).HasPrecision(18, 4);
            builder.Property(a => a.Tier3Min).HasPrecision(18, 4);
            builder.Property(a => a.Tier3Max).HasPrecision(18, 4);

            builder.HasIndex(a => a.AffixKey).IsUnique();
        }
    }
}