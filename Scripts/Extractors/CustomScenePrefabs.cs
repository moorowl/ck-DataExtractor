using DataExtractor.DataStructures;
using DataExtractor.Utilities;
using Newtonsoft.Json.Linq;
using System.Linq;
using UnityEngine;

namespace DataExtractor.Extractors {
	public class CustomScenePrefabs : Extractor {
		public override string Name => "customScenePrefabs";

		public override JToken Extract() {
			var prefabs = Manager.ecs.pugDatabase.prefabList;
			var objects = prefabs.Select(prefab => new ObjectDataFromPrefab(prefab.gameObject))
				.OrderBy(data => data.SortKey);

			var scenesDataTable = Resources.Load<CustomScenesDataTable>("Scenes/CustomScenesDataTable");

			var baseObjects = objects
				.Where(data => !data.HasComponent<CustomScenePrefabAuthoring>())
				.Select(data => {
					var objectInfo = data.ObjectInfo;
					var objectData = new ObjectDataCD {
						objectID = objectInfo.objectID,
						variation = objectInfo.variation,
						amount = 1
					};

					var serializedComponents = data.Components.OrderBy(entry => entry.Key.Name)
						.ToDictionary(entry => entry.Key.Name, entry => Serialize(entry.Value, DefaultSettingsForObjects) as JObject);

					return (objectData, serializedComponents);
				})
				.ToDictionary(entry => entry.objectData, entry => entry.serializedComponents);

			var customScenePrefabs = scenesDataTable.scenes.SelectMany(scene => scene.prefabs)
				.Select(prefab => new ObjectDataFromPrefab(prefab))
				.Where(data => data.HasComponent<CustomScenePrefabAuthoring>())
				.GroupBy(data => Utils.GetUniqueScenePrefabName(data.Prefab))
				.Select(group => group.First())
				.OrderBy(data => data.SortKey)
				.Select(data => {
					var baseObjectData = new ObjectDataCD {
						objectID = data.ObjectInfo.objectID,
						variation = data.ObjectInfo.variation
					};

					var baseSerializedComponents = baseObjects[baseObjectData];
					var serializedComponents = data.Components.OrderBy(entry => entry.Key.Name)
						.ToDictionary(entry => entry.Key.Name, entry => Serialize(entry.Value, DefaultSettingsForObjects) as JObject);

					var overrideComponents = serializedComponents
						.Where(x => !baseSerializedComponents.ContainsKey(x.Key) || x.Value.ToString() != baseSerializedComponents[x.Key].ToString())
						.ToDictionary(x => x.Key, x => x.Value);

					return new {
						AuthoringPrefabName = Utils.GetUniqueScenePrefabName(data.Prefab),
						Object = new {
							Id = data.ObjectInfo.objectID,
							Variation = data.ObjectInfo.variation
						},
						ComponentOverrides = overrideComponents
					};
				});

			return Serialize(customScenePrefabs);
		}
	}
}