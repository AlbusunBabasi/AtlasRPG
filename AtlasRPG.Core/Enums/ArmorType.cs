// AtlasRPG.Core/Enums/ArmorType.cs
namespace AtlasRPG.Core.Enums
{
    public enum ArmorType
    {
        HeavyArmor = 1,  // Yüksek armor, düşük evasion → Warrior/Berserker
        EvasionArmor = 2,  // Yüksek evasion, düşük armor → Rogue/Archer
        WardArmor = 3   // Ward odaklı, düşük armor/evasion → Mage
    }
}
