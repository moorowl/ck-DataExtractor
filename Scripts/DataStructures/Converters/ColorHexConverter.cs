using DataExtractor.Utilities.Extensions;
using Newtonsoft.Json;
using System;
using UnityEngine;

namespace DataExtractor.DataStructures.Converters {
	public class ColorHexConverter : JsonConverter {
		public override bool CanRead => false;

		public override bool CanConvert(Type objectType) {
			return objectType == typeof(Color);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
			var color = (Color) value;

			writer.WriteValue(color.ToHex());
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
			JsonSerializer serializer) {
			throw new NotImplementedException();
		}
	}
}