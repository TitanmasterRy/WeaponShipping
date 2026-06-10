namespace WeaponShipping
{
    public class ModConfig
    {
        // Category toggles
        public bool EnableWeapons  { get; set; } = true;
        public bool EnableClothing { get; set; } = true;
        public bool EnableHats     { get; set; } = true;
        public bool EnableBoots    { get; set; } = true;
        public bool EnableRings    { get; set; } = true;

        // Pricing
        public float PriceMultiplier  { get; set; } = 1.0f;
        public int   MinimumSellPrice { get; set; } = 100;
        public bool  UseDamageFormulaForUnknownWeapons { get; set; } = true;

        // Skillful Clothes Revamp integration
        public bool EnableSkillfulClothesIntegration { get; set; } = true;

        // Display
        public bool ShowBinContentsHUD { get; set; } = true;

        // Misc
        public bool VerboseLogging { get; set; } = false;
    }
}
