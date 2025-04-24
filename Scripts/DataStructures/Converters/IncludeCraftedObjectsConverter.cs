using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataExtractor.DataStructures.Converters {
	public class IncludeCraftedObjectsConverter : JsonConverter {
		public override bool CanRead => false;

		public override bool CanConvert(Type objectType) {
			return objectType == typeof(List<CraftingAuthoring>);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
			var craftingAuthorings = (List<CraftingAuthoring>) value;

			var o = new JArray(
				craftingAuthorings.Select(authoring => authoring.gameObject.GetComponent<EntityMonoBehaviourData>())
					.Where(data => data != null)
					.Select(data => data.ObjectInfo.objectID.ToString())
			);

			o.WriteTo(writer);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
			JsonSerializer serializer) {
			throw new NotImplementedException();
		}
	}
}