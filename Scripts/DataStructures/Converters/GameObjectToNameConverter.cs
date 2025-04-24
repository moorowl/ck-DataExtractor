using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

namespace DataExtractor.DataStructures.Converters {
	public class GameObjectToNameConverter : JsonConverter {
		public override bool CanRead => false;

		public override bool CanConvert(Type objectType) {
			return objectType == typeof(GameObject);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
			var gameObject = (GameObject) value;

			if (gameObject.TryGetComponent<EntityMonoBehaviourData>(out var entityMonoBehaviourData)) {
				var o = new JObject(
					new JProperty("id", entityMonoBehaviourData.ObjectInfo.objectID.ToString()),
					new JProperty("variation", entityMonoBehaviourData.ObjectInfo.variation)
				);
				o.WriteTo(writer);
			}
			else {
				writer.WriteValue(gameObject.name);
			}
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
			JsonSerializer serializer) {
			throw new NotImplementedException();
		}
	}
}