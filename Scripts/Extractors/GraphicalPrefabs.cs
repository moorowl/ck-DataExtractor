using Newtonsoft.Json.Linq;
using System.Linq;
using UnityEngine;

namespace DataExtractor.Extractors {
	public class GraphicalPrefabs : Extractor {
		public override string Name => "graphicalPrefabs";

		public override JToken Extract() {
			var prefabs = Manager.ecs.pugDatabase.prefabList
				.Select(prefab => {
					if (!prefab.TryGetComponent<EntityMonoBehaviourData>(out var entityMonoBehaviourData))
						return null;

					return entityMonoBehaviourData.objectInfo.prefabInfos[0].prefab?.gameObject;
				})
				.OfType<GameObject>()
				.GroupBy(prefab => prefab.name)
				.Select(group => group.First())
				.OrderBy(prefab => prefab.name);

			var data = prefabs.Select(prefab => {
				var entityMonoBehaviour = prefab.GetComponent<EntityMonoBehaviour>();

				return new {
					InternalName = prefab.name,
					EntityMonoBehaviour = new {
						SoundOptions = entityMonoBehaviour.soundOptions
					}
				};
			});

			return Serialize(data);
		}
	}
}