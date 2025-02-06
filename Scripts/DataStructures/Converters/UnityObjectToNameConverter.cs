using Newtonsoft.Json;
using System;

namespace DataExtractor.DataStructures.Converters {
	public class UnityObjectToNameConverter<T> : JsonConverter where T : UnityEngine.Object {
		public override bool CanRead => false;

		public override bool CanConvert(Type objectType) {
			return objectType == typeof(T);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
			var unityObject = (UnityEngine.Object) value;

			writer.WriteValue(unityObject.name);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
			JsonSerializer serializer) {
			throw new NotImplementedException();
		}
	}
}