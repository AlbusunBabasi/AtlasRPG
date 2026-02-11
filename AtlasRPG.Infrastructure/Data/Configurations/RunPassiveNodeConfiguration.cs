// AtlasRPG.Infrastructure/Data/Configurations/RunPassiveNodeConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AtlasRPG.Core.Entities.Runs;

namespace AtlasRPG.Infrastructure.Data.Configurations
{
    public class RunPassiveNodeConfiguration : IEntityTypeConfiguration<RunPassiveNode>
    {
        public void Configure(EntityTypeBuilder<RunPassiveNode> builder)
        {
            builder.ToTable("RunPassiveNodes");

            builder.HasKey(rpn => rpn.Id);

            builder.Property(rpn => rpn.NodeId)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(rpn => rpn.AllocatedAtLevel)
                .IsRequired();

            builder.HasIndex(rpn => new { rpn.RunId, rpn.NodeId })
                .IsUnique();

            // Relationships
            builder.HasOne(rpn => rpn.Run)
                .WithMany(r => r.AllocatedPassives)
                .HasForeignKey(rpn => rpn.RunId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}