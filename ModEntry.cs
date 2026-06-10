using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Tools;

namespace WeaponShipping
{
    public class ModEntry : Mod
    {
        private ModConfig Config = null!;

        // Skillful Clothes Revamp integration
        private const string SkillfulClothesModId = "w8AwA8w.SkillfulClothesRevamp";
        private bool skillfulClothesLoaded;
        private const int SkillfulClothesValuePerPoint = 100;

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

        // Named vanilla rings; unknown/modded rings fall back to their own vanilla price
        private static readonly Dictionary<string, int> RingPrices = new()
        {
            { "Small Glow Ring",       100  },
            { "Glow Ring",             300  },
            { "Small Magnet Ring",     100  },
            { "Magnet Ring",           300  },
            { "Slime Charmer Ring",   1000  },
            { "Warrior Ring",         1000  },
            { "Vampire Ring",         1000  },
            { "Savage Ring",          1000  },
            { "Ring of Yoba",         1000  },
            { "Sturdy Ring",          1000  },
            { "Burglar's Ring",       1500  },
            { "Iridium Band",         2000  },
            { "Jukebox Ring",         1500  },
            { "Amethyst Ring",         200  },
            { "Topaz Ring",            200  },
            { "Aquamarine Ring",       200  },
            { "Jade Ring",             200  },
            { "Emerald Ring",          300  },
            { "Ruby Ring",             300  },
            { "Glowstone Ring",        400  },
            { "Crabshell Ring",        750  },
            { "Napalm Ring",          1500  },
            { "Thorns Ring",          1500  },
            { "Phoenix Ring",         2500  },
            { "Hot Java Ring",        1000  },
            { "Lucky Ring",           1000  },
            { "Protection Ring",      1000  },
            { "Soul Sapper Ring",     1000  },
        };

        public override void Entry(IModHelper helper)
        {
            this.Config = helper.ReadConfig<ModConfig>();

            this.skillfulClothesLoaded = helper.ModRegistry.IsLoaded(SkillfulClothesModId);

            helper.Events.Display.MenuChanged += this.OnMenuChanged;
            helper.Events.Display.RenderedHud += this.OnRenderedHud;
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

            // ── Categories ────────────────────────────────────────
            configMenu.AddSectionTitle(mod: this.ModManifest, text: () => "Categories");

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
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Enable Rings",
                tooltip: () => "Allow rings to be sold via the shipping bin.",
                getValue: () => this.Config.EnableRings,
                setValue: value => this.Config.EnableRings = value
            );
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

            // ── Skillful Clothes Integration ──────────────────────
            // Only surfaced when Skillful Clothes Revamp is actually installed.
            if (this.skillfulClothesLoaded)
            {
                configMenu.AddSectionTitle(mod: this.ModManifest, text: () => "Skillful Clothes Integration");

                configMenu.AddBoolOption(
                    mod: this.ModManifest,
                    name: () => "Enable Skillful Clothes Integration",
                    tooltip: () => "Boost the sell price of clothing based on the strength and rarity of its Skillful Clothes Revamp effects.",
                    getValue: () => this.Config.EnableSkillfulClothesIntegration,
                    setValue: value => this.Config.EnableSkillfulClothesIntegration = value
                );
            }

            // ── Display ───────────────────────────────────────────
            configMenu.AddSectionTitle(mod: this.ModManifest, text: () => "Display");

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Show Bin Contents HUD",
                tooltip: () => "Show a small overlay on the farm with the number and estimated value of sellable gear waiting in the shipping bin.",
                getValue: () => this.Config.ShowBinContentsHUD,
                setValue: value => this.Config.ShowBinContentsHUD = value
            );
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
            else if (e.NewMenu is ShippingMenu shippingMenu)
            {
                AdjustShippingSummary(shippingMenu);
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
            || (item is Boots    && this.Config.EnableBoots)
            || (item is Ring     && this.Config.EnableRings);

        private static string GetCategoryName(Item item) => item switch
        {
            MeleeWeapon _ => "Weapons",
            Slingshot _   => "Weapons",
            Clothing _    => "Clothing",
            Hat _         => "Hats",
            Boots _       => "Boots",
            Ring _        => "Rings",
            _             => "Other"
        };

        private int GetSellPrice(Item item)
        {
            int basePrice = item switch
            {
                MeleeWeapon w => ComputeWeaponBasePrice(w),
                Slingshot s   => ComputeSlingshotBasePrice(s),
                Clothing c    => 250 + ComputeSkillfulClothesBonus(c),
                Hat _         => 500,
                Boots b       => ComputeBootsBasePrice(b),
                Ring r        => ComputeRingBasePrice(r),
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

        private static int ComputeRingBasePrice(Ring ring)
        {
            if (RingPrices.TryGetValue(ring.Name, out int knownPrice))
                return knownPrice;

            // Unknown/modded rings: fall back to the ring's own vanilla price, else a flat default
            int vanillaPrice = ring.price.Value;
            return vanillaPrice > 0 ? vanillaPrice : 300;
        }

        // ── Skillful Clothes Revamp integration ───────────────────
        // Effects are stored on the clothing item's modData by Skillful Clothes
        // Revamp. We read every entry that belongs to that mod and value the item
        // by the combined magnitude of its numeric effects plus how many distinct
        // effects it carries, so stronger and rarer pieces fetch more gold.
        private int ComputeSkillfulClothesBonus(Clothing clothing)
        {
            if (!this.skillfulClothesLoaded || !this.Config.EnableSkillfulClothesIntegration)
                return 0;

            double strength = 0;
            int effectCount = 0;

            foreach (string key in clothing.modData.Keys)
            {
                if (key.IndexOf("SkillfulClothesRevamp", StringComparison.OrdinalIgnoreCase) < 0
                    && !key.StartsWith("w8AwA8w", StringComparison.OrdinalIgnoreCase))
                    continue;

                effectCount++;

                string raw = clothing.modData[key] ?? string.Empty;
                foreach (Match match in Regex.Matches(raw, @"-?\d+(\.\d+)?"))
                {
                    if (double.TryParse(match.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
                        strength += System.Math.Abs(value);
                }
            }

            if (effectCount == 0)
                return 0;

            return (int)((strength + effectCount) * SkillfulClothesValuePerPoint);
        }

        // ── Vanilla end-of-day shipping summary integration ───────
        // Weapons and gear are shipped by the vanilla night routine just like any
        // other bin item: they already land in the summary's "Misc" (other) category,
        // but priced at their (usually negligible) vanilla sell value. Here we bump
        // each one up to our custom price and pay the player the difference, so the
        // screen stays completely vanilla while the gold matches our pricing.
        private void AdjustShippingSummary(ShippingMenu menu)
        {
            try
            {
                const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
                Type type = typeof(ShippingMenu);

                var categoryItems    = (List<List<Item>>)type.GetField("categoryItems", flags)!.GetValue(menu)!;
                var categoryTotals   = (List<int>)type.GetField("categoryTotals", flags)!.GetValue(menu)!;
                var itemValues       = (Dictionary<Item, int>)type.GetField("itemValues", flags)!.GetValue(menu)!;
                var singleItemValues = (Dictionary<Item, int>)type.GetField("singleItemValues", flags)!.GetValue(menu)!;
                var categoryDials    = (List<MoneyDial>)type.GetField("categoryDials", flags)!.GetValue(menu)!;

                int total = ShippingMenu.total_category;
                int totalDelta = 0;
                var summary = new Dictionary<string, (int Count, int Gold)>();
                var touchedCategories = new HashSet<int>();

                // Walk the real categories (everything except the combined "total" view)
                // so each item is only counted once.
                for (int cat = 0; cat < total; cat++)
                {
                    foreach (Item item in categoryItems[cat])
                    {
                        if (!IsItemShippable(item))
                            continue;

                        int vanillaValue = singleItemValues.TryGetValue(item, out int v) ? v : 0;
                        int ourPrice = GetSellPrice(item);
                        int newValue = System.Math.Max(ourPrice, vanillaValue);
                        int delta = newValue - vanillaValue;
                        if (delta <= 0)
                            continue;

                        itemValues[item] = newValue;
                        singleItemValues[item] = newValue;
                        categoryTotals[cat] += delta;
                        categoryTotals[total] += delta;
                        totalDelta += delta;
                        touchedCategories.Add(cat);

                        string category = GetCategoryName(item);
                        summary.TryGetValue(category, out var stats);
                        summary[category] = (stats.Count + 1, stats.Gold + newValue);

                        if (this.Config.VerboseLogging)
                            this.Monitor.Log($"Sold '{item.DisplayName}' ({category}) for {newValue}g (vanilla value {vanillaValue}g).", LogLevel.Info);
                    }
                }

                if (totalDelta <= 0)
                    return;

                // Keep the animated money dials in sync with the corrected totals.
                touchedCategories.Add(total);
                foreach (int cat in touchedCategories)
                {
                    if (cat < categoryDials.Count)
                    {
                        categoryDials[cat].currentValue = categoryTotals[cat];
                        categoryDials[cat].previousTargetValue = categoryTotals[cat];
                    }
                }

                Game1.player.Money += totalDelta;

                var parts = summary.Select(kv => $"{kv.Key}: {kv.Value.Count} sold for {kv.Value.Gold}g");
                this.Monitor.Log(
                    $"Shipping sales — {string.Join(", ", parts)}. Bonus over vanilla value: +{totalDelta}g.",
                    LogLevel.Info);
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"Could not adjust the shipping summary: {ex.Message}", LogLevel.Trace);
            }
        }

        // ── Bin contents HUD ──────────────────────────────────────
        private void OnRenderedHud(object? sender, RenderedHudEventArgs e)
        {
            if (!this.Config.ShowBinContentsHUD || !Context.IsWorldReady)
                return;

            if (Game1.currentLocation is not Farm)
                return;

            Farm? farm = Game1.getFarm();
            if (farm == null)
                return;

            int count = 0;
            int value = 0;
            foreach (var item in farm.getShippingBin(Game1.player))
            {
                if (item != null && IsItemShippable(item))
                {
                    count++;
                    value += GetSellPrice(item);
                }
            }

            if (count == 0)
                return;

            var font = Game1.smallFont;
            string line1 = $"Bin: {count} gear";
            string line2 = $"~{value}g";

            Vector2 size1 = font.MeasureString(line1);
            Vector2 size2 = font.MeasureString(line2);

            int boxWidth  = (int)System.Math.Max(size1.X, size2.X) + 32;
            int boxHeight = (int)(size1.Y + size2.Y) + 24;
            int x = Game1.uiViewport.Width  - boxWidth  - 16;
            int y = Game1.uiViewport.Height - boxHeight - 16;

            IClickableMenu.drawTextureBox(
                e.SpriteBatch, Game1.menuTexture, new Rectangle(0, 256, 60, 60),
                x, y, boxWidth, boxHeight, Color.White, 1f, false);

            Utility.drawTextWithShadow(e.SpriteBatch, line1, font, new Vector2(x + 16, y + 12), Game1.textColor);
            Utility.drawTextWithShadow(e.SpriteBatch, line2, font, new Vector2(x + 16, y + 12 + size1.Y), Game1.textColor);
        }
    }
}
