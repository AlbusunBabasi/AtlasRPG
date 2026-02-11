// AtlasRPG.Infrastructure/Data/Configurations/PassiveNodeDefinitionConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AtlasRPG.Core.Entities.GameData;

namespace AtlasRPG.Infrastructure.Data.Configurations
{
    public class PassiveNodeDefinitionConfiguration : IEntityTypeConfiguration<PassiveNodeDefinition>
    {
        public void Configure(EntityTypeBuilder<PassiveNodeDefinition> builder)
        {
            builder.ToTable("PassiveNodeDefinitions");

            builder.HasKey(pnd => pnd.Id);

            builder.Property(pnd => pnd.NodeId)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(pnd => pnd.DisplayName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(pnd => pnd.NodeType)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(pnd => pnd.RequiredLevel)
                .IsRequired();

            builder.Property(pnd => pnd.PrerequisiteNodeIds)
                .HasMaxLength(100);

            builder.Property(pnd => pnd.EffectJson)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(pnd => pnd.Description)
                .HasMaxLength(500);

            builder.HasIndex(pnd => pnd.NodeId)
                .IsUnique();

            builder.HasIndex(pnd => pnd.RequiredLevel);
        }
    }
}