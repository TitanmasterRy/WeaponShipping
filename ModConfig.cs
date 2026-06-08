namespace WeaponShipping
{
    /// <summary>
    /// Mod configuration. Saved to config.json in the mod folder.
    /// </summary>
    public class ModConfig
    {
        /// <summary>Multiplier applied to all weapon sell prices. Default 1.0 = normal prices.</summary>
        public float PriceMultiplier { get; set; } = 1.0f;

        /// <summary>Minimum gold a weapon can sell for, regardless of price or multiplier.</summary>
        public int MinimumSellPrice { get; set; } = 100;

        /// <summary>If true, unknown/modded weapons use the damage-based price formula. If false, they sell for MinimumSellPrice.</summary>
        public bool UseDamageFormulaForUnknownWeapons { get; set; } = true;

        /// <summary>If true, a message is shown in the SMAPI console listing each weapon sold and its price.</summary>
        public bool VerboseLogging { get; set; } = false;
    }
}
