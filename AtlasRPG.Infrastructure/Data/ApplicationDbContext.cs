using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AtlasRPG.Core.Entities.Identity;
using AtlasRPG.Core.Entities.Runs;
using AtlasRPG.Core.Entities.Items;
using AtlasRPG.Core.Entities.Combat;
using AtlasRPG.Core.Entities.Matchmaking;
using AtlasRPG.Core.Entities.GameData;

namespace AtlasRPG.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Run System
        public DbSet<Run> Runs { get; set; }
        public DbSet<RunTurn> RunTurns { get; set; }
        public DbSet<RunItem> RunItems { get; set; }
        public DbSet<RunEquipment> RunEquipments { get; set; }
        public DbSet<RunPassiveNode> RunPassiveNodes { get; set; }

        // Item System
        public DbSet<Item> Items { get; set; }
        public DbSet<ItemAffix> ItemAffixes { get; set; }
        public DbSet<AffixDefinition> AffixDefinitions { get; set; }

        // Combat System
        public DbSet<CombatResult> CombatResults { get; set; }
        public DbSet<CombatRound> CombatRounds { get; set; }

        // Matchmaking
        public DbSet<Snapshot> Snapshots { get; set; }

        // Static Game Data
        public DbSet<SkillDefinition> SkillDefinitions { get; set; }
        public DbSet<PassiveNodeDefinition> PassiveNodeDefinitions { get; set; }
        public DbSet<BaseStatDefinition> BaseStatDefinitions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ✅ Bu satır tüm IEntityTypeConfiguration'ları otomatik bulur ve uygular
            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            // Identity table names (optional customization)
            builder.Entity<ApplicationUser>().ToTable("Users");
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityRole>().ToTable("Roles");
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<string>>().ToTable("UserRoles");
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<string>>().ToTable("UserClaims");
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<string>>().ToTable("UserLogins");
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<string>>().ToTable("UserTokens");
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>>().ToTable("RoleClaims");
        }
    }
}