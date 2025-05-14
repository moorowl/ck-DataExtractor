using DataExtractor.Utilities;
using Newtonsoft.Json.Linq;
using PugTilemap;
using System.Collections.Generic;
using System.Linq;
using Pug.UnityExtensions;
using Unity.Mathematics;
using UnityEngine;
using static CustomScenesDataTable;

namespace DataExtractor.Extractors {
	public class CustomScenes : Extractor {
		public override string Name => "customScenes";

		public override JToken Extract() {
			var scenesDataTable = Resources.Load<CustomScenesDataTable>("Scenes/CustomScenesDataTable");

			var data = scenesDataTable.scenes.Select((scene, i) => {
				var tiles = new Dictionary<TileCD, List<int2>>();
				var boundsMax = (int2) int.MaxValue;
				var boundsMin = (int2) int.MinValue;

				foreach (var map in scene.maps) {
					using var tileIterator = map.mapData.GetTileIterator();

					while (tileIterator.MoveNext()) {
						var tileInfo = tileIterator.CurrentTileData;
						var key = new TileCD {
							tileset = tileInfo.tilesetType,
							tileType = tileInfo.tileType
						};
						var position = tileIterator.CurrentPosition.ToInt2() + map.localPosition;

						boundsMax = math.min(boundsMax, position);
						boundsMin = math.max(boundsMin, position);

						if (!tiles.ContainsKey(key))
							tiles[key] = new List<int2>();
						tiles[key].Add(position);
					}
				}

				int2? center = scene.hasCenter ? scene.center : null;
				if (scene.maps.Count > 0)
					center = (int2) math.round((float2) (boundsMax + boundsMin) / 2f);

				return new {
					InternalName = scene.sceneName,
					MaxOccurrences = scene.maxOccurrences,
					ReplacedByContentBundle = (ContentBundleID?) (scene.replacedByContentBundle.hasValue ? scene.replacedByContentBundle.value : null),
					BiomesToSpawnIn = scene.biomesToSpawnIn,
					MinDistanceFromCoreInClassicWorlds = scene.minDistanceFromCoreInClassicWorlds,
					CanFlip = new {
						X = scene.canFlipX,
						Y = scene.canFlipY
					},
					Radius = scene.radius,
					Center = center,
					Objects = scene.prefabs.Select((prefab, i) => {
						var objectInfo = prefab.GetComponent<EntityMonoBehaviourData>().objectInfo;

						if (objectInfo.isCustomScenePrefab || prefab.GetComponent<CustomScenePrefabAuthoring>() != null) {
							return new {
								Prefab = Utils.GetUniqueScenePrefabName(prefab),
								Position = scene.prefabPositions[i],
								InventoryOverrides = MapInventoryOverride(scene.prefabInventoryOverrides[i])
							};
						}

						return new {
							Id = objectInfo.objectID,
							Variation = objectInfo.variation,
							Position = scene.prefabPositions[i],
							InventoryOverrides = MapInventoryOverride(scene.prefabInventoryOverrides[i])
						};

						// need this to make the compiler happy for some reason
						return new object { };
					}),
					Tiles = tiles.Select(entry => new {
						TileType = entry.Key.tileType,
						Tileset = (Tileset) entry.Key.tileset,
						Positions = entry.Value
					})
				};
			});

			return Serialize(data);
		}

		private static object MapInventoryOverride(InventoryOverride inventoryOverride) {
			if (!inventoryOverride.hasAnyInventoryOverride)
				return null;

			return new {
				LootTable = (LootTableID?) (inventoryOverride.hasLootTableOverride ? inventoryOverride.lootTableOverride : null),
				Items = inventoryOverride.hasItemsOverride
					? inventoryOverride.itemsOverride.Select(item => new {
						Id = item.objectID,
						Variation = item.variation,
						Amount = item.amount,
					})
					: null,
				ExistingItemsToRemove = (int?) (inventoryOverride.hasItemsOverride ? inventoryOverride.itemsToRemove : null)
			};
		}
	}
}