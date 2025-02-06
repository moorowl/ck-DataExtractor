using Newtonsoft.Json.Linq;
using System.Linq;

namespace DataExtractor.Extractors {
	public class LootTables : Extractor {
		public override string Name => "lootTables";

		public override JToken Extract() {
			var lootTables = Manager.mod.LootTable.OrderBy(lootTable => (int) lootTable.id);

			var data = lootTables.Select(lootTable => {
				var id = lootTable.id;

				return new {
					Id = (int) id,
					InternalName = id,
					UniqueDrops = new {
						Min = lootTable.minUniqueDrops,
						Max = lootTable.maxUniqueDrops
					},
					DontAllowDuplicates = lootTable.dontAllowDuplicates,
					LootInfos = lootTable.lootInfos.Select(info => new {
						Id = info.objectID,
						Weight = info.weight,
						Amount = info.amount,
						OnlyDropsInBiome = (Biome?) (info.onlyDropsInBiome != Biome.None ? info.onlyDropsInBiome : null)
					}),
					GuaranteedLootInfos = lootTable.guaranteedLootInfos.Select(info => new {
						Id = info.objectID,
						Weight = info.weight,
						Amount = info.amount,
						OnlyDropsInBiome = (Biome?) (info.onlyDropsInBiome != Biome.None ? info.onlyDropsInBiome : null)
					})
				};
			});

			return Serialize(data);
		}
	}
}