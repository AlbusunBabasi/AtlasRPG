// AtlasRPG.Infrastructure/Data/Configurations/RunTurnConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AtlasRPG.Core.Entities.Runs;

namespace AtlasRPG.Infrastructure.Data.Configurations
{
    public class RunTurnConfiguration : IEntityTypeConfiguration<RunTurn>
    {
        public void Configure(EntityTypeBuilder<RunTurn> builder)
        {
            builder.ToTable("RunTurns");

            builder.HasKey(rt => rt.Id);

            builder.Property(rt => rt.TurnNumber)
                .IsRequired();

            builder.HasIndex(rt => new { rt.RunId, rt.TurnNumber })
                .IsUnique();

            // Relationships
            builder.HasOne(rt => rt.Run)
                .WithMany(r => r.Turns)
                .HasForeignKey(rt => rt.RunId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(rt => rt.CombatResult)
                .WithOne(cr => cr.RunTurn)
                .HasForeignKey<RunTurn>(rt => rt.CombatResultId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}