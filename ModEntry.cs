using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Tools;

namespace WeaponShipping
{
    public class ModEntry : Mod
    {
        /*********
        ** Fields
        *********/
        private ModConfig Config = null!;

        /*********
        ** Weapon price lookup table
        ** Sell prices based on ~50% of Adventurer's Guild / shop buy prices.
        *********/
        private static readonly Dictionary<string, int> WeaponPrices = new()
        {
            { "Wood Sword",            100  },
            { "Wood Club",             125  },
            { "Elf Blade",             150  },
            { "Wood Mallet",           200  },
            { "Pirate's Sword",        250  },
            { "Cutlass",               300  },
            { "Silver Saber",          750  },
            { "Steel Smallsword",      600  },
            { "Iron Edge",             500  },
            { "Burglars Shank",        750  },
            { "Claymore",             1000  },
            { "Shadow Dagger",        1250  },
            { "Assassin Blade",       1500  },
            { "Crystal Dagger",       1750  },
            { "Bone Sword",           1500  },
            { "Templar's Blade",      2000  },
            { "Obsidian Edge",        3000  },
            { "Dark Sword",           2500  },
            { "Holy Blade",           3500  },
            { "Lava Katana",          5000  },
            { "Galaxy Sword",        15000  },
            { "Galaxy Dagger",       10000  },
            { "Galaxy Hammer",       12000  },
            { "Infinity Blade",      30000  },
            { "Infinity Dagger",     25000  },
            { "Infinity Gavel",      27500  },
            { "Dwarf Sword",          5000  },
            { "Dwarf Hammer",         5000  },
            { "Dwarf Dagger",         5000  },
            { "Dragontooth Cutlass",  6000  },
            { "Dragontooth Club",     6000  },
            { "Dragontooth Shiv",     6000  },
            { "Iridium Needle",       8000  },
            { "Slingshot",             200  },
            { "Master Slingshot",      750  },
        };

        /*********
        ** Public methods
        *********/
        public override void Entry(IModHelper helper)
        {
            this.Config = helper.ReadConfig<ModConfig>();

            helper.Events.GameLoop.DayEnding += this.OnDayEnding;
            helper.Events.Display.MenuChanged += this.OnMenuChanged;
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;

            this.Monitor.Log("WeaponShipping mod loaded. You can now sell weapons in the shipping bin!", LogLevel.Info);
        }

        /*********
        ** Private methods
        *********/

        /// <summary>
        /// Register with Generic Mod Config Menu if it's installed.
        /// </summary>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var configMenu = this.Helper.ModRegistry
                .GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");

            if (configMenu is null)
                return;

            configMenu.Register(
                mod: this.ModManifest,
                reset: () => this.Config = new ModConfig(),
                save: () => this.Helper.WriteConfig(this.Config)
            );

            configMenu.AddSectionTitle(
                mod: this.ModManifest,
                text: () => "Weapon Shipping"
            );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Price Multiplier",
                tooltip: () => "Multiplies all weapon sell prices. 1.0 = normal, 2.0 = double, 0.5 = half.",
                getValue: () => this.Config.PriceMultiplier,
                setValue: value => this.Config.PriceMultiplier = value,
                min: 0.1f,
                max: 10f,
                interval: 0.1f
            );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Minimum Sell Price",
                tooltip: () => "No weapon will sell for less than this amount (before the multiplier).",
                getValue: () => (float)this.Config.MinimumSellPrice,
                setValue: value => this.Config.MinimumSellPrice = (int)value,
                min: 0f,
                max: 10000f,
                interval: 50f
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Damage Formula for Unknown Weapons",
                tooltip: () => "If on, weapons not in the price table (e.g. from mods) use a damage-based formula. If off, they sell for the minimum price.",
                getValue: () => this.Config.UseDamageFormulaForUnknownWeapons,
                setValue: value => this.Config.UseDamageFormulaForUnknownWeapons = value
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Verbose Logging",
                tooltip: () => "Print each weapon sold and its price to the SMAPI console.",
                getValue: () => this.Config.VerboseLogging,
                setValue: value => this.Config.VerboseLogging = value
            );
        }

        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is ItemGrabMenu menu)
            {
                if (menu.context is Farm || IsShippingBinMenu(menu))
                    PatchShippingMenu(menu);
            }
        }

        private bool IsShippingBinMenu(ItemGrabMenu menu)
        {
            return menu.source == ItemGrabMenu.source_none
                && menu.reverseGrab
                && menu.showReceivingMenu == false;
        }

        private void PatchShippingMenu(ItemGrabMenu menu)
        {
            var original = menu.inventory.highlightMethod;
            menu.inventory.highlightMethod = item =>
            {
                if (item is MeleeWeapon || item is Slingshot)
                    return true;
                return original(item);
            };

            var originalAccept = menu.ItemsToGrabMenu?.highlightMethod;
            if (originalAccept != null && menu.ItemsToGrabMenu != null)
            {
                menu.ItemsToGrabMenu.highlightMethod = item =>
                {
                    if (item is MeleeWeapon || item is Slingshot)
                        return true;
                    return originalAccept(item);
                };
            }
        }

        private void OnDayEnding(object sender, DayEndingEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            Farm farm = Game1.getFarm();
            if (farm == null)
                return;

            var weaponsToSell = farm.getShippingBin(Game1.player)
                .Where(item => item is MeleeWeapon || item is Slingshot)
                .ToList();

            if (weaponsToSell.Count == 0)
                return;

            int totalEarned = 0;

            foreach (var weapon in weaponsToSell)
            {
                int sellPrice = GetWeaponSellPrice(weapon);
                totalEarned += sellPrice;

                farm.getShippingBin(Game1.player).Remove(weapon);

                if (this.Config.VerboseLogging)
                    this.Monitor.Log($"Sold '{weapon.DisplayName}' for {sellPrice}g.", LogLevel.Info);
            }

            if (totalEarned > 0)
            {
                Game1.player.Money += totalEarned;
                this.Monitor.Log($"Weapon sales: +{totalEarned}g ({weaponsToSell.Count} weapon{(weaponsToSell.Count == 1 ? "" : "s")}).", LogLevel.Info);
            }
        }

        private int GetWeaponSellPrice(Item item)
        {
            int basePrice;

            if (WeaponPrices.TryGetValue(item.Name, out int knownPrice))
            {
                basePrice = knownPrice;
            }
            else if (this.Config.UseDamageFormulaForUnknownWeapons && item is MeleeWeapon weapon)
            {
                int avgDamage = (weapon.minDamage.Value + weapon.maxDamage.Value) / 2;
                basePrice = (int)(avgDamage * avgDamage * 0.8f) + avgDamage * 15;
            }
            else if (item is Slingshot sling)
            {
                basePrice = sling.upgradeLevel.Value switch
                {
                    0 => 200,
                    1 => 750,
                    _ => 1500
                };
            }
            else
            {
                basePrice = 0;
            }

            basePrice = System.Math.Max(basePrice, this.Config.MinimumSellPrice);
            return (int)(basePrice * this.Config.PriceMultiplier);
        }
    }
}
