using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Tools;

namespace WeaponShipping
{
    public class ModEntry : Mod
    {
        private ModConfig Config = null!;

        // ~50% of Adventurer's Guild / shop buy prices
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

        // Named vanilla boots; unknown boots fall back to the stats formula
        private static readonly Dictionary<string, int> BootsPrices = new()
        {
            { "Sneakers",              100  },
            { "Work Boots",            250  },
            { "Leather Boots",         350  },
            { "Rubber Boots",          200  },
            { "Thermal Boots",         500  },
            { "Combat Boots",          750  },
            { "Emily's Magic Boots",  1000  },
            { "Space Boots",          1500  },
            { "Crystal Shoes",         750  },
            { "Genie Shoes",          1500  },
            { "Dark Boots",           2000  },
            { "Mermaid Boots",        1750  },
            { "Cinderclown Shoes",     750  },
            { "Tundra Boots",          600  },
            { "Firewalker Boots",      600  },
        };

        public override void Entry(IModHelper helper)
        {
            this.Config = helper.ReadConfig<ModConfig>();

            helper.Events.GameLoop.DayEnding  += this.OnDayEnding;
            helper.Events.Display.MenuChanged += this.OnMenuChanged;
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;

            this.Monitor.Log("WeaponShipping mod loaded. Sell weapons, clothing, hats, and boots via the shipping bin!", LogLevel.Info);
        }

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
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

            // ── What to Sell ──────────────────────────────────────
            configMenu.AddSectionTitle(mod: this.ModManifest, text: () => "What to Sell");

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Enable Weapons",
                tooltip: () => "Allow melee weapons and slingshots to be sold via the shipping bin.",
                getValue: () => this.Config.EnableWeapons,
                setValue: value => this.Config.EnableWeapons = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Enable Clothing",
                tooltip: () => "Allow clothing (shirts, pants) to be sold via the shipping bin.",
                getValue: () => this.Config.EnableClothing,
                setValue: value => this.Config.EnableClothing = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Enable Hats",
                tooltip: () => "Allow hats to be sold via the shipping bin.",
                getValue: () => this.Config.EnableHats,
                setValue: value => this.Config.EnableHats = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Enable Boots",
                tooltip: () => "Allow boots to be sold via the shipping bin.",
                getValue: () => this.Config.EnableBoots,
                setValue: value => this.Config.EnableBoots = value
            );

            // ── Pricing ───────────────────────────────────────────
            configMenu.AddSectionTitle(mod: this.ModManifest, text: () => "Pricing");

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Price Multiplier",
                tooltip: () => "Multiplies all sell prices. 1.0 = normal, 2.0 = double, 0.5 = half.",
                getValue: () => this.Config.PriceMultiplier,
                setValue: value => this.Config.PriceMultiplier = value,
                min: 0.1f,
                max: 10f,
                interval: 0.1f
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Minimum Sell Price",
                tooltip: () => "No item will sell for less than this amount (before the multiplier).",
                getValue: () => (float)this.Config.MinimumSellPrice,
                setValue: value => this.Config.MinimumSellPrice = (int)value,
                min: 0f,
                max: 10000f,
                interval: 50f
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Damage Formula for Unknown Weapons",
                tooltip: () => "If on, weapons not in the price table use a damage-based formula. If off, they sell for the minimum price.",
                getValue: () => this.Config.UseDamageFormulaForUnknownWeapons,
                setValue: value => this.Config.UseDamageFormulaForUnknownWeapons = value
            );

            // ── Other ─────────────────────────────────────────────
            configMenu.AddSectionTitle(mod: this.ModManifest, text: () => "Other");

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Verbose Logging",
                tooltip: () => "Print each item sold and its price to the SMAPI console.",
                getValue: () => this.Config.VerboseLogging,
                setValue: value => this.Config.VerboseLogging = value
            );
        }

        private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is ItemGrabMenu menu)
            {
                if (menu.context is Farm || IsShippingBinMenu(menu))
                    PatchShippingMenu(menu);
            }
        }

        private static bool IsShippingBinMenu(ItemGrabMenu menu)
        {
            return menu.source == ItemGrabMenu.source_none
                && menu.reverseGrab
                && menu.showReceivingMenu == false;
        }

        private void PatchShippingMenu(ItemGrabMenu menu)
        {
            var original = menu.inventory.highlightMethod;
            menu.inventory.highlightMethod = item =>
                IsItemShippable(item) || original(item);

            var originalAccept = menu.ItemsToGrabMenu?.highlightMethod;
            if (originalAccept != null && menu.ItemsToGrabMenu != null)
            {
                menu.ItemsToGrabMenu.highlightMethod = item =>
                    IsItemShippable(item) || originalAccept(item);
            }
        }

        private bool IsItemShippable(Item item) =>
            ((item is MeleeWeapon || item is Slingshot) && this.Config.EnableWeapons)
            || (item is Clothing && this.Config.EnableClothing)
            || (item is Hat      && this.Config.EnableHats)
            || (item is Boots    && this.Config.EnableBoots);

        private void OnDayEnding(object? sender, DayEndingEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            Farm? farm = Game1.getFarm();
            if (farm == null)
                return;

            var bin = farm.getShippingBin(Game1.player);
            var itemsToSell = bin.Where(IsItemShippable).ToList();

            if (itemsToSell.Count == 0)
                return;

            int totalEarned = 0;
            var summary = new Dictionary<string, (int Count, int Gold)>();

            foreach (var item in itemsToSell)
            {
                int sellPrice = GetSellPrice(item);
                totalEarned += sellPrice;
                bin.Remove(item);

                string category = GetCategoryName(item);
                summary.TryGetValue(category, out var stats);
                summary[category] = (stats.Count + 1, stats.Gold + sellPrice);

                if (this.Config.VerboseLogging)
                    this.Monitor.Log($"Sold '{item.DisplayName}' ({category}) for {sellPrice}g.", LogLevel.Info);
            }

            if (totalEarned > 0)
            {
                Game1.player.Money += totalEarned;

                var parts = summary.Select(kv => $"{kv.Key}: {kv.Value.Count} sold for {kv.Value.Gold}g");
                this.Monitor.Log(
                    $"Shipping sales — {string.Join(", ", parts)}. Total: +{totalEarned}g.",
                    LogLevel.Info);
            }
        }

        private static string GetCategoryName(Item item) => item switch
        {
            MeleeWeapon _ => "Weapons",
            Slingshot _   => "Weapons",
            Clothing _    => "Clothing",
            Hat _         => "Hats",
            Boots _       => "Boots",
            _             => "Other"
        };

        private int GetSellPrice(Item item)
        {
            int basePrice = item switch
            {
                MeleeWeapon w => ComputeWeaponBasePrice(w),
                Slingshot s   => ComputeSlingshotBasePrice(s),
                Clothing _    => 250,
                Hat _         => 500,
                Boots b       => ComputeBootsBasePrice(b),
                _             => 0
            };

            basePrice = System.Math.Max(basePrice, this.Config.MinimumSellPrice);
            return (int)(basePrice * this.Config.PriceMultiplier);
        }

        private int ComputeWeaponBasePrice(MeleeWeapon weapon)
        {
            if (WeaponPrices.TryGetValue(weapon.Name, out int knownPrice))
                return knownPrice;

            if (this.Config.UseDamageFormulaForUnknownWeapons)
            {
                int avgDamage = (weapon.minDamage.Value + weapon.maxDamage.Value) / 2;
                return (int)(avgDamage * avgDamage * 0.8f) + avgDamage * 15;
            }

            return 0;
        }

        private static int ComputeSlingshotBasePrice(Slingshot sling) =>
            sling.upgradeLevel.Value switch
            {
                0 => 200,
                1 => 750,
                _ => 1500
            };

        private static int ComputeBootsBasePrice(Boots boots)
        {
            if (BootsPrices.TryGetValue(boots.Name, out int knownPrice))
                return knownPrice;

            // Unknown boots: price by defense and immunity stats
            int defense  = boots.defenseBonus.Value;
            int immunity = boots.immunityBonus.Value;
            return defense * 200 + immunity * 150;
        }
    }
}
