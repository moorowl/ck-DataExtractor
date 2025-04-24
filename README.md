# DataExtractor

Core Keeper mod to extract a lot of data from the game.

## Usage

First, build the mod using the official [SDK](https://mod.io/g/corekeeper/r/core-keeper-mod-sdk-introduction).

Once installed, run the game with the `-dataex` command-line arg, followed by a path to the folder you want the data placed in. `{version}` will be replaced with the game's version. Example:  `-dataex "C:/Core Keeper Data/{version}"`

## Data provided

| File name | Description/Notes
| --- | --- |
| `achievements` |
| `conditionEffects` |
| `conditions` | Status effects.
| `contentBundles` |
| `cooking` | All base cooking combinations.
| `credits` | Credits entries.
| `customScenePrefabs` | Unique object variations used in scenes.
| `customScenes` | Scene structures.
| `dungeons` | Dungeon structures. (both unique and random)
| `enums` |
| `environmentEvents` | Environment events (e.g. cave-ins) and their spawn conditions.
| `environmentSpawnObjects` | Natural object and creature spawns.
| `factions` |
| `fishing` | Fishing loot and struggle data.
| `graphicalPrefabs` | Object graphical prefab info. Just sounds at the moment.
| `instruments` |
| `lootTables` |
| `miscellaneous` |
| `music` | Music rosters and their audio files.
| `objects` | Items, objects, creatures, projectiles, etc.
| `objectCategories` | Object creative mode categories.
| `pets` | Pet talents and skins/palettes.
| `platforms` |
| `playerCustomization` |
| `roles` | Character backgrounds.
| `seasons` | Seasonal events and their start/end dates.
| `setBonuses` |
| `sfx` |
| `skills` | Skills and their talents.
| `souls` |
| `spriteAssets` |
| `text` |
| `tiles` | `TileType`s, `Tileset`s and their properties/map colors.
| `upgradeCosts` | Costs to upgrade items in the Upgrade Station.
| `version` | Game version, branch, Unity version and build date.
| `worldGen` | Default world parameters and tile mappings for 1.0 generation.
