using I2.Loc;
using PugMod;
using PugTilemap;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DataExtractor.Utilities {
	public static class Utils {
		public static string GetText(string term) {
			return LocalizationManager.GetTranslation(term, overrideLanguage: "English");
		}

		public static string GetObjectName(ObjectID id, int variation) {
			var term = GetObjectDisplayTerm(id, variation);
			return GetText($"Items/{term}") ?? GetText($"Names/{term}");
		}

		public static string GetObjectDescription(ObjectID id, int variation) {
			return GetText($"Items/{GetObjectDisplayTerm(id, variation)}Desc");
		}

		public static string GetObjectDisplayTerm(ObjectID id, int variation) {
			id = PlayerController.GetAnyObjectIDReplaceForNameAndDesc(id);

			API.Authoring.ObjectProperties.TryGetPropertyString(id, "name", out var term);

			var termOverride = Manager.ui.itemOverridesTable.GetNameTermOverride(new ObjectData {
				objectID = id,
				variation = variation
			});
			if (termOverride is not null)
				term = termOverride;

			return term;
		}

		public static (Sprite icon, Sprite smallIcon) GetObjectIcons(ObjectInfo info) {
			var objectData = new ObjectDataCD {
				objectID = info.objectID,
				variation = info.variation,
				amount = 1
			};
			var iconOverride = Manager.ui.itemOverridesTable.GetIconOverride(objectData, false);
			var smallIconOverride = Manager.ui.itemOverridesTable.GetIconOverride(objectData, true);

			return (iconOverride ?? info.icon, smallIconOverride ?? info.smallIcon);
		}

		private static readonly Lazy<Dictionary<TileCD, Color32>> TileColors = new(() => {
			var table = Resources.Load<TileTypeColorTable>("TileTypeColorTable");
			var map = new Dictionary<TileCD, Color32>();

			foreach (var tilesetColor in table.tileSetColors) {
				foreach (var tileTypeColor in tilesetColor.tileColors) {
					var key = new TileCD {
						tileset = (int) tilesetColor.pugMapTileset,
						tileType = tileTypeColor.tileType
					};

					map[key] = tileTypeColor.color;
				}
			}

			return map;
		});

		public static bool TryGetTileColor(Tileset tileset, TileType tileType, out Color32 color) {
			var key = new TileCD {
				tileset = (int) tileset,
				tileType = tileType
			};
			return TileColors.Value.TryGetValue(key, out color);
		}

		private static readonly Lazy<Dictionary<int, string>> ScenePrefabNames = new(() => {
			var scenesDataTable = Resources.Load<CustomScenesDataTable>("Scenes/CustomScenesDataTable");
			var allPrefabs = scenesDataTable.scenes.SelectMany(scene => scene.prefabs)
				.GroupBy(prefab => prefab.GetInstanceID())
				.Select(group => group.First())
				.OrderByDescending(prefab => prefab.GetInstanceID());

			var uniqueNameToPrefab = new Dictionary<string, GameObject>();

			foreach (var prefab in allPrefabs) {
				var name = prefab.name;
				var index = 0;

				while (uniqueNameToPrefab.ContainsKey(name)) {
					name = $"{prefab.name}_{index}";
					index++;
				}

				uniqueNameToPrefab[name] = prefab;
			}

			return uniqueNameToPrefab.ToDictionary(x => x.Value.GetInstanceID(), x => x.Key);
		});

		public static string GetUniqueScenePrefabName(GameObject prefab) {
			if (ScenePrefabNames.Value.TryGetValue(prefab.GetInstanceID(), out var name))
				return name;

			return $"{prefab.name}/{prefab.GetInstanceID()}";
		}
	}
}