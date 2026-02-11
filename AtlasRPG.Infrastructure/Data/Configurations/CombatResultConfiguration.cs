// AtlasRPG.Infrastructure/Data/Configurations/CombatResultConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AtlasRPG.Core.Entities.Combat;

namespace AtlasRPG.Infrastructure.Data.Configurations
{
    public class CombatResultConfiguration : IEntityTypeConfiguration<CombatResult>
    {
        public void Configure(EntityTypeBuilder<CombatResult> builder)
        {
            builder.ToTable("CombatResults");

            builder.HasKey(cr => cr.Id);

            builder.Property(cr => cr.OpponentSnapshotId)
                .HasMaxLength(50);

            builder.Property(cr => cr.OpponentUsername)
                .HasMaxLength(100);

            builder.Property(cr => cr.MatchSeed)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(cr => cr.IsVictory)
                .IsRequired();

            builder.Property(cr => cr.TotalRounds)
                .IsRequired();

            // Decimal properties
            builder.Property(cr => cr.PlayerTotalDamageDealt)
                .HasPrecision(18, 2);

            builder.Property(cr => cr.PlayerTotalDamageTaken)
                .HasPrecision(18, 2);

            builder.Property(cr => cr.OpponentTotalDamageDealt)
                .HasPrecision(18, 2);

            builder.Property(cr => cr.OpponentTotalDamageTaken)
                .HasPrecision(18, 2);

            builder.HasIndex(cr => cr.RunTurnId)
                .IsUnique();

            builder.HasIndex(cr => cr.OpponentSnapshotId);
            builder.HasIndex(cr => cr.CreatedAt);

            // Relationships
            builder.HasOne(cr => cr.RunTurn)
                .WithOne(rt => rt.CombatResult)
                .HasForeignKey<CombatResult>(cr => cr.RunTurnId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(cr => cr.Rounds)
                .WithOne(r => r.CombatResult)
                .HasForeignKey(r => r.CombatResultId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}