// AtlasRPG.Infrastructure/Data/Configurations/RunEquipmentConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AtlasRPG.Core.Entities.Runs;

namespace AtlasRPG.Infrastructure.Data.Configurations
{
    public class RunEquipmentConfiguration : IEntityTypeConfiguration<RunEquipment>
    {
        public void Configure(EntityTypeBuilder<RunEquipment> builder)
        {
            builder.ToTable("RunEquipments");

            builder.HasKey(re => re.Id);

            builder.HasIndex(re => re.RunId)
                .IsUnique();

            // Relationships - Run owned by this equipment
            builder.HasOne(re => re.Run)
                .WithOne(r => r.Equipment)
                .HasForeignKey<RunEquipment>(re => re.RunId)
                .OnDelete(DeleteBehavior.Cascade);

            // Weapon slot
            builder.HasOne(re => re.Weapon)
                .WithMany()
                .HasForeignKey(re => re.WeaponId)
                .OnDelete(DeleteBehavior.Restrict);

            // Offhand slot
            builder.HasOne(re => re.Offhand)
                .WithMany()
                .HasForeignKey(re => re.OffhandId)
                .OnDelete(DeleteBehavior.Restrict);

            // Armor slot
            builder.HasOne(re => re.Armor)
                .WithMany()
                .HasForeignKey(re => re.ArmorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Belt slot
            builder.HasOne(re => re.Belt)
                .WithMany()
                .HasForeignKey(re => re.BeltId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}