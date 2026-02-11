// AtlasRPG.Infrastructure/Data/Configurations/ItemConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AtlasRPG.Core.Entities.Items;

namespace AtlasRPG.Infrastructure.Data.Configurations
{
    public class ItemConfiguration : IEntityTypeConfiguration<Item>
    {
        public void Configure(EntityTypeBuilder<Item> builder)
        {
            builder.ToTable("Items");

            builder.HasKey(i => i.Id);

            builder.Property(i => i.BaseAttackSpeed).HasPrecision(18, 2);
            builder.Property(i => i.BaseDamage).HasPrecision(18, 2);
            builder.Property(i => i.BaseCritChance).HasPrecision(18, 4);
            builder.Property(i => i.BaseArmor).HasPrecision(18, 2);
            builder.Property(i => i.BaseEvasion).HasPrecision(18, 2);
            builder.Property(i => i.BaseWard).HasPrecision(18, 2);
            builder.Property(i => i.BaseBlockChance).HasPrecision(18, 4);

            builder.HasIndex(i => new { i.Slot, i.ItemLevel });

            builder.HasMany(i => i.Affixes)
                .WithOne(a => a.Item)
                .HasForeignKey(a => a.ItemId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}