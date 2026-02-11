// AtlasRPG.Infrastructure/Data/Configurations/SkillDefinitionConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AtlasRPG.Core.Entities.GameData;

namespace AtlasRPG.Infrastructure.Data.Configurations
{
    public class SkillDefinitionConfiguration : IEntityTypeConfiguration<SkillDefinition>
    {
        public void Configure(EntityTypeBuilder<SkillDefinition> builder)
        {
            builder.ToTable("SkillDefinitions");

            builder.HasKey(sd => sd.Id);

            builder.Property(sd => sd.SkillId)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(sd => sd.DisplayName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(sd => sd.Multiplier)
                .IsRequired()
                .HasPrecision(18, 4);

            builder.Property(sd => sd.ManaCost)
                .IsRequired();

            builder.Property(sd => sd.Cooldown)
                .IsRequired();

            builder.Property(sd => sd.RequiredLevel)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(sd => sd.EffectJson)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(sd => sd.Description)
                .HasMaxLength(500);

            builder.HasIndex(sd => sd.SkillId)
                .IsUnique();

            builder.HasIndex(sd => sd.WeaponType);
        }
    }
}