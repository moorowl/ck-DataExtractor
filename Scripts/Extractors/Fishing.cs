using Newtonsoft.Json.Linq;
using System.Linq;

namespace DataExtractor.Extractors {
	public class Fishing : Extractor {
		public override string Name => "fishing";

		public override JToken Extract() {
			var fishingTable = Manager.mod.FishingTable;

			var data = new {
				BiomeLoot = fishingTable.fishingInfos.Where(info => info.biomes.Count > 0).Select(info => new {
					Biome = info.biomes[0],
					FishLootTable = info.fishLootTableID,
					JunkLootTable = info.lootTableID,
					MinimumFishing = FishingTable.GetSkillRequiredForBiome(info.biomes[0])
				}),
				LiquidLoot = fishingTable.fishingInfos.Where(info => info.waterTilesets.Count > 0).Select(info => new {
					Tileset = info.waterTilesets[0],
					FishLootTable = info.fishLootTableID,
					JunkLootTable = info.lootTableID,
					FishingRequired = FishingTable.GetSkillRequiredForWater(info.waterTilesets[0])
				}),
				Patterns = fishingTable.fishStruggleInfos.Select(info => new {
					Fish = info.fishID,
					Pattern = info.struggleData.Select(struggleData => new {
						IsStruggling = struggleData.isStruggling,
						Duration = struggleData.time
					})
				})
			};

			return Serialize(data);
		}
	}
}