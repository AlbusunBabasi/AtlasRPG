// AtlasRPG.Web/Models/Run/TurnHubViewModel.cs
using AtlasRPG.Core.Entities.Runs;
using AtlasRPG.Core.Entities.Items;
using AtlasRPG.Core.Entities.GameData;
using AtlasRPG.Core.ValueObjects;

namespace AtlasRPG.Web.Models.RunViews
{
    public class TurnHubViewModel
    {
        public Core.Entities.Runs.Run Run { get; set; } = null!;
        public RunTurn CurrentTurn { get; set; } = null!;
        public RunEquipment? Equipment { get; set; }
        public List<SkillDefinition> AvailableSkills { get; set; } = new();
        public List<RunItem> Inventory { get; set; } = new();

        // Gerçek hesaplanmış stat'lar (StatCalculatorService'den gelir)
        public CharacterStats Stats { get; set; } = new();

        // Kısa erişim alias'lar (view'da @Model.MaxHp çalışmaya devam etsin)
        public decimal MaxHp => Stats.MaxHp;
        public decimal MaxMana => Stats.MaxMana;
        public decimal Damage => Stats.MeleeDamage; // Weapon tipine göre ViewModel'de override edilebilir
        public decimal Armor => Stats.Armor;
        public decimal CritChance => Stats.CritChance;

        // Ek stat preview
        public decimal Evasion => Stats.Evasion;
        public decimal Ward => Stats.Ward;
        public decimal BlockChance => Stats.BlockChance;
        public decimal Initiative => Stats.Initiative;
        public decimal Accuracy => Stats.Accuracy;
        public decimal FireResist => Stats.FireResist;
        public decimal ColdResist => Stats.ColdResist;
        public decimal LightResist => Stats.LightningResist;
        public decimal ChaosResist => Stats.ChaosResist;
        public decimal CritMult => Stats.CritMultiplier;
        public decimal ArmorPen => Stats.ArmorPenetration;

        public Guid? LastSelectedSkillId { get; set; }
        public List<AtlasRPG.Core.Entities.Items.Item> ShopItems { get; set; } = new();
        public Dictionary<Guid, int> ShopPrices { get; set; } = new();

    }
}
