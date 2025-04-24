using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace DataExtractor.DataStructures.Converters {
	public class DirectionConverter : JsonConverter {
		public override bool CanRead => false;

		public override bool CanConvert(Type objectType) {
			return objectType == typeof(Direction);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
			var direction = (Direction) value;
			var o = new JObject(
				new JProperty("x", direction.i2.x),
				new JProperty("y", direction.i2.y)
			);

			o.WriteTo(writer);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
			throw new NotImplementedException();
		}
	}
}