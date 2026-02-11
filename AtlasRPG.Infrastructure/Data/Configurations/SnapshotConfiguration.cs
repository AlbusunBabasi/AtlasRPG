// AtlasRPG.Infrastructure/Data/Configurations/SnapshotConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AtlasRPG.Core.Entities.Matchmaking;

namespace AtlasRPG.Infrastructure.Data.Configurations
{
    public class SnapshotConfiguration : IEntityTypeConfiguration<Snapshot>
    {
        public void Configure(EntityTypeBuilder<Snapshot> builder)
        {
            builder.ToTable("Snapshots");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.SnapshotHash)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(s => s.StatsJson)
                .IsRequired();

            builder.Property(s => s.PowerScore).HasPrecision(18, 4);
            builder.Property(s => s.StructuralScore).HasPrecision(18, 4);

            builder.HasIndex(s => s.SnapshotHash).IsUnique();
            builder.HasIndex(s => new { s.TurnIndex, s.PowerBand, s.IsValid });
            builder.HasIndex(s => s.CreatedAt);
        }
    }
}