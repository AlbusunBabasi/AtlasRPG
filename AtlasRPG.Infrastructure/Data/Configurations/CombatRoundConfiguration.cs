// AtlasRPG.Infrastructure/Data/Configurations/CombatRoundConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AtlasRPG.Core.Entities.Combat;

namespace AtlasRPG.Infrastructure.Data.Configurations
{
    public class CombatRoundConfiguration : IEntityTypeConfiguration<CombatRound>
    {
        public void Configure(EntityTypeBuilder<CombatRound> builder)
        {
            builder.ToTable("CombatRounds");

            builder.HasKey(cr => cr.Id);

            builder.Property(cr => cr.RoundNumber)
                .IsRequired();

            builder.Property(cr => cr.PlayerAction)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(cr => cr.OpponentAction)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(cr => cr.EventLog)
                .HasMaxLength(2000);

            // Decimal properties
            builder.Property(cr => cr.PlayerDamage)
                .HasPrecision(18, 2);

            builder.Property(cr => cr.OpponentDamage)
                .HasPrecision(18, 2);

            builder.Property(cr => cr.PlayerHpRemaining)
                .HasPrecision(18, 2);

            builder.Property(cr => cr.OpponentHpRemaining)
                .HasPrecision(18, 2);

            builder.HasIndex(cr => new { cr.CombatResultId, cr.RoundNumber });

            // Relationships
            builder.HasOne(cr => cr.CombatResult)
                .WithMany(c => c.Rounds)
                .HasForeignKey(cr => cr.CombatResultId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}