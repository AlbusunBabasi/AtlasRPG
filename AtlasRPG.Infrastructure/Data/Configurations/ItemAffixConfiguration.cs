// AtlasRPG.Infrastructure/Data/Configurations/ItemAffixConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AtlasRPG.Core.Entities.Items;

namespace AtlasRPG.Infrastructure.Data.Configurations
{
    public class ItemAffixConfiguration : IEntityTypeConfiguration<ItemAffix>
    {
        public void Configure(EntityTypeBuilder<ItemAffix> builder)
        {
            builder.ToTable("ItemAffixes");

            builder.HasKey(ia => ia.Id);

            builder.Property(ia => ia.Tier)
                .IsRequired();

            builder.Property(ia => ia.RolledValue)
                .IsRequired()
                .HasPrecision(18, 4);

            builder.HasIndex(ia => ia.ItemId);
            builder.HasIndex(ia => ia.AffixDefinitionId);

            // Relationships
            builder.HasOne(ia => ia.Item)
                .WithMany(i => i.Affixes)
                .HasForeignKey(ia => ia.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ia => ia.AffixDefinition)
                .WithMany()
                .HasForeignKey(ia => ia.AffixDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}