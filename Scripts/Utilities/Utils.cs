using I2.Loc;
using PugMod;
using PugTilemap;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DataExtractor.Utilities {
	public static class Utils {
		public static string GetTranslation(string term, string language) {
			return LocalizationManager.GetTranslation(term, overrideLanguage: language)?.Replace("\r", "");
		}
		
		public static Dictionary<string, string> GetTranslations(string term) {
			var translations = LocalizationManager.GetAllLanguages()
				.Select(language => (LocalizationManager.GetLanguageCode(language), GetTranslation(term, language)))
				.Where(tuple => tuple.Item2 != null)
				.ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);
			
			return translations.Any() ? translations : null;
		}

		public static string GetObjectName(ObjectID id, int variation, string language) {
			var term = GetObjectDisplayTerm(id, variation);
			return GetTranslation($"Items/{term}", language) ?? GetTranslation($"Names/{term}", language);
		}
		
		public static Dictionary<string, string> GetObjectNames(ObjectID id, int variation) {
			var objectNames = LocalizationManager.GetAllLanguages()
				.Select(language => (LocalizationManager.GetLanguageCode(language), GetObjectName(id, variation, language)))
				.Where(tuple => tuple.Item2 != null)
				.ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);
			
			return objectNames.Any() ? objectNames : null;
		}

		public static string GetObjectDescription(ObjectID id, int variation, string language) {
			return GetTranslation($"Items/{GetObjectDisplayTerm(id, variation)}Desc", language);
		}
		
		public static Dictionary<string, string> GetObjectDescriptions(ObjectID id, int variation) {
			var objectDescriptions = LocalizationManager.GetAllLanguages()
				.Select(language => (LocalizationManager.GetLanguageCode(language), GetObjectDescription(id, variation, language)))
				.Where(tuple => tuple.Item2 != null)
				.ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);
			
			return objectDescriptions.Any() ? objectDescriptions : null;
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
			var allPrefabs = new List<(GameObject Prefab, string Scene, int IndexInScene)>();

			foreach (var scene in scenesDataTable.scenes) {
				for (var i = 0; i < scene.prefabs.Count; i++) {
					var prefab = scene.prefabs[i];
					allPrefabs.Add((prefab, scene.sceneName, i));
				}
			}
			allPrefabs.Sort((x, y) => x.Prefab.GetInstanceID().CompareTo(y.Prefab.GetInstanceID()));

			var uniqueNameToPrefab = new Dictionary<string, GameObject>();

			foreach (var prefab in allPrefabs) {
				var baseName = prefab.Prefab.name.Replace($"_{prefab.IndexInScene}", "") + "_" + prefab.Scene;
				var index = 0;
				var name = $"{baseName}_{index}";
				index++;

				while (uniqueNameToPrefab.ContainsKey(name)) {
					name = $"{baseName}_{index}";
					index++;
				}

				uniqueNameToPrefab[name] = prefab.Prefab;
			}

			return uniqueNameToPrefab.GroupBy(x => x.Value.GetInstanceID()).Select(group => group.First()).ToDictionary(x => x.Value.GetInstanceID(), x => x.Key);
		});

		public static string GetUniqueScenePrefabName(GameObject prefab) {
			if (ScenePrefabNames.Value.TryGetValue(prefab.GetInstanceID(), out var name))
				return name;

			return $"{prefab.name}/{prefab.GetInstanceID()}";
		}
	}
}