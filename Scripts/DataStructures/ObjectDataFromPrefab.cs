using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DataExtractor.DataStructures {
	public class ObjectDataFromPrefab {
		public GameObject Prefab { get; private set; }
		public ObjectInfo ObjectInfo { get; private set; }
		public Dictionary<Type, MonoBehaviour> Components { get; private set; }

		public int SortKey => (int) ObjectInfo.objectID * 10000 + ObjectInfo.variation;

		public ObjectDataFromPrefab(GameObject gameObject) {
			Prefab = gameObject;
			ObjectInfo = GetEntityMonoBehaviourData(gameObject).ObjectInfo;
			Components = GetAuthoringComponents(gameObject);
		}

		public bool HasComponent<T>() where T : MonoBehaviour {
			return Components.ContainsKey(typeof(T));
		}

		public T GetComponent<T>() where T : MonoBehaviour {
			return (T) Components.GetValueOrDefault(typeof(T));
		}

		public bool TryGetComponent<T>(out T component) where T : MonoBehaviour {
			if (Components.TryGetValue(typeof(T), out var baseComponent)) {
				component = (T) baseComponent;
				return true;
			}

			component = null;
			return false;
		}

		private static IEntityMonoBehaviourData GetEntityMonoBehaviourData(GameObject gameObject) {
			return gameObject.GetComponent<EntityMonoBehaviourData>();
		}

		private static Dictionary<Type, MonoBehaviour> GetAuthoringComponents(GameObject gameObject) {
			return gameObject.GetComponents(typeof(MonoBehaviour))
				.Cast<MonoBehaviour>()
				.Where(component => {
					var name = component.GetType().Name;
					return name.EndsWith("Authoring") || name.EndsWith("Authering");
				})
				.GroupBy(component => component.GetType().Name)
				.Select(group => group.First())
				.ToDictionary(component => component.GetType(), component => component);
		}
	}
}