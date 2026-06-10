# Weapon Shipping — Stardew Valley SMAPI Mod

Allows you to sell **weapons, clothing, hats, boots, and rings** by dropping them into the shipping bin. Their sell value is added to your daily earnings just like any other shipped item.

---

## Requirements

| Requirement | Version |
|-------------|---------|
| Stardew Valley | 1.6+ |
| SMAPI | 3.18.0+ |

---

## Installation

1. **Install SMAPI** if you haven't already → https://smapi.io
2. Download or build this mod (see [Building from Source](#building-from-source) below).
3. Copy the `WeaponShipping` folder (containing `WeaponShipping.dll` and `manifest.json`) into your `Mods` directory:

   | OS | Mods folder path |
   |----|-----------------|
   | Windows | `C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\Mods\` |
   | macOS | `~/Library/Application Support/Steam/steamapps/common/Stardew Valley/Mods/` |
   | Linux | `~/.steam/steam/steamapps/common/Stardew Valley/Mods/` |

4. Launch the game through SMAPI (not directly from Steam).

---

## How to Use

1. Open your shipping bin (the box near the farmhouse).
2. Drag any weapon, clothing item, hat, pair of boots, or ring from your inventory into the bin.
3. Go to sleep. The items appear in the vanilla end-of-day shipping summary under the **Misc** category, priced with this mod's values, and the gold is added to your earnings.

The SMAPI console will also show a per-category breakdown, e.g.:
```
Shipping sales — Weapons: 2 sold for 3000g, Hats: 1 sold for 500g. Bonus over vanilla value: +3500g.
```

---

## Sell Prices

None of these item types have official sell prices in vanilla Stardew Valley. This mod uses the following logic for each category:

### Weapons
- Known weapons use a built-in price table (~50% of Adventurer's Guild buy price). Examples: Wood Sword = 100g, Galaxy Sword = 15,000g, Infinity Blade = 30,000g.
- Unknown/modded melee weapons (if "Damage Formula for Unknown Weapons" is enabled) are estimated: **(avgDamage² × 0.8) + avgDamage × 15**, where avgDamage = `(minDamage + maxDamage) / 2`.
- Slingshots: **200g / 750g / 1500g** by upgrade level.

### Boots
- Known vanilla boots use a built-in price table (e.g. Sneakers = 100g, Dark Boots = 2,000g).
- Unknown boots use a stats formula: **defense × 200 + immunity × 150**.

### Clothing & Hats
- Clothing (shirts, pants) sells for a flat **250g**.
- Hats sell for a flat **500g** (half the Hat Mouse price).

### Rings
- Known vanilla rings use a built-in price table (e.g. Glow Ring = 300g, Iridium Band = 2,000g, Phoenix Ring = 2,500g).
- Unknown/modded rings fall back to the ring's own vanilla price (or a flat **300g** if it has none).
- Combined Rings are supported and sell as a single ring item.

### All categories
- Every price is floored at the configurable **Minimum Sell Price** (default 100g) and then scaled by the **Price Multiplier** (default 1.0).

---

## Configuration

If you have [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098) installed, all options are available in-game under the mod's settings page. Otherwise, edit `config.json` in the mod folder.

| Setting | Default | Description |
|---------|---------|-------------|
| Enable Weapons | `true` | Sell melee weapons and slingshots via the bin |
| Enable Clothing | `true` | Sell clothing (shirts, pants) via the bin |
| Enable Hats | `true` | Sell hats via the bin |
| Enable Boots | `true` | Sell boots via the bin |
| Enable Rings | `true` | Sell rings via the bin |
| Price Multiplier | `1.0` | Scale all sell prices (0.1–10.0) |
| Minimum Sell Price | `100` | Price floor before the multiplier is applied |
| Damage Formula for Unknown Weapons | `true` | Use damage stats to price unlisted weapons |
| Enable Skillful Clothes Integration | `true` | Boost clothing prices by their Skillful Clothes Revamp effects (only shown when that mod is installed) |
| Show Bin Contents HUD | `true` | Small farm overlay showing how much sellable gear is in the bin |
| Verbose Logging | `false` | Log each individual item sold to the SMAPI console |

Options are grouped in GMCM under three headers: **Categories**, **Skillful Clothes Integration** (only when that mod is installed), and **Display**.

---

## Building from Source

### Prerequisites
- [.NET 6 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
- Stardew Valley installed (for the game DLLs)

### Steps

1. Clone or download this repository.
2. Open `WeaponShipping.csproj` and set the `GamePath` property to your Stardew Valley install folder:

   ```xml
   <!-- Example for Windows Steam -->
   <PropertyGroup>
     <GamePath>C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley</GamePath>
   </PropertyGroup>
   ```

   Or pass it on the command line:

   ```bash
   dotnet build -p:GamePath="C:/Program Files (x86)/Steam/steamapps/common/Stardew Valley"
   ```

3. Build the project:

   ```bash
   dotnet build --configuration Release
   ```

4. Find the output in `bin/Release/net6.0/`.
5. Copy `WeaponShipping.dll` and `manifest.json` into a `WeaponShipping` folder inside your `Mods` directory.

The `.csproj` includes a `DeployMod` target that automatically copies the built DLL and manifest to your `Mods` folder on every build.

---

## Compatibility

- ✅ Works with multiplayer (each player's shipped items are credited to them individually).
- ✅ Compatible with Content Patcher and most other SMAPI mods.
- ✅ Modded weapons not in the price table are priced via the damage formula.
- ⚠️ May conflict with mods that heavily rewrite the shipping bin menu.

---

## Changelog

### 1.3.0
- Added support for selling **rings** via the shipping bin, including a vanilla ring price table and a fallback for modded rings. Combined Rings are supported.
- Added the **Enable Rings** config toggle (default on) under the Categories section.

### 1.2.0
- Sold weapons and gear now appear in the vanilla end-of-day shipping summary, folded into the existing **Misc** category alongside other shipped items (only items actually sold that night are shown).
- Added optional **Skillful Clothes Revamp** integration: when that mod is installed, clothing sold through the bin is worth more based on the strength and rarity of its effects. Fully optional — does nothing if the mod isn't present.
- Added a small **Bin Contents HUD** in the bottom-right corner while on the farm, showing the count and estimated value of sellable gear waiting in the shipping bin.
- Reorganised GMCM into three sections — **Categories**, **Skillful Clothes Integration**, and **Display** — with a tooltip on every option.

### 1.1.0
- Added support for selling **clothing**, **hats**, and **boots** via the shipping bin.
- Added per-category config toggles: Enable Weapons, Enable Clothing, Enable Hats, Enable Boots (all on by default).
- Added a boots price table (15 vanilla entries) with a stats-based formula fallback (`defense × 200 + immunity × 150`).
- End-of-day SMAPI log now shows a per-category breakdown (e.g. `Weapons: 2 sold for 3000g, Hats: 1 sold for 500g`).
- GMCM settings page reorganised into three sections: **What to Sell**, **Pricing**, **Other**.
- Added `.gitignore` for `bin/` and `obj/` build artifacts.

### 1.0.0
- Initial release: sell melee weapons and slingshots via the shipping bin.
- Hardcoded price table for all vanilla weapons (~50% of Adventurer's Guild price).
- Damage-based formula for unknown/modded weapons.
- GMCM support for Price Multiplier, Minimum Sell Price, damage formula toggle, and verbose logging.

---

## License

MIT — do whatever you like with it.
