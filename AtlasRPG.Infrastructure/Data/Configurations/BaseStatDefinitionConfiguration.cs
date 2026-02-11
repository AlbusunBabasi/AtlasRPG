// AtlasRPG.Infrastructure/Data/Configurations/BaseStatDefinitionConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AtlasRPG.Core.Entities.GameData;

namespace AtlasRPG.Infrastructure.Data.Configurations
{
    public class BaseStatDefinitionConfiguration : IEntityTypeConfiguration<BaseStatDefinition>
    {
        public void Configure(EntityTypeBuilder<BaseStatDefinition> builder)
        {
            builder.ToTable("BaseStatDefinitions");

            builder.HasKey(bsd => bsd.Id);

            builder.Property(bsd => bsd.BaseHp)
                .IsRequired()
                .HasDefaultValue(100);

            builder.Property(bsd => bsd.BaseMana)
                .IsRequired()
                .HasDefaultValue(50);

            builder.Property(bsd => bsd.BaseFireResist)
                .HasPrecision(18, 4)
                .HasDefaultValue(0);

            builder.Property(bsd => bsd.BaseColdResist)
                .HasPrecision(18, 4)
                .HasDefaultValue(0);

            builder.Property(bsd => bsd.BaseLightningResist)
                .HasPrecision(18, 4)
                .HasDefaultValue(0);

            builder.Property(bsd => bsd.BaseChaosResist)
                .HasPrecision(18, 4)
                .HasDefaultValue(0);

            builder.Property(bsd => bsd.Notes)
                .HasMaxLength(500);

            builder.HasIndex(bsd => new { bsd.Race, bsd.Class });
        }
    }
}