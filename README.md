# Weapon Shipping — Stardew Valley SMAPI Mod

Allows you to sell **any weapon** (swords, clubs, daggers, slingshots) by dropping them into the shipping bin. Their sell value is added to your daily earnings just like any other shipped item.

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
2. Drag any weapon from your inventory into the bin.
3. Go to sleep. The weapon's value will appear in your earnings summary the next morning.

### Weapon Sell Prices

Weapons in Stardew Valley don't have official sell prices in the vanilla game data. This mod uses the following logic:

- If the game already defines a sell price → uses that directly.
- Otherwise, it estimates: **(minDamage + maxDamage) / 2 × 10**, with a minimum of **50g**.
- Slingshots sell for **50g / 150g / 300g** depending on upgrade level.

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

### Optional: Auto-deploy on build

Uncomment the `<Target Name="DeployMod">` block in the `.csproj` to automatically copy the mod files to your Mods folder every time you build.

---

## Compatibility

- ✅ Works with multiplayer (each player's shipped weapons are credited to them individually).
- ✅ Compatible with Content Patcher and most other SMAPI mods.
- ⚠️ May conflict with mods that heavily rewrite the shipping bin menu.

---

## License

MIT — do whatever you like with it.
