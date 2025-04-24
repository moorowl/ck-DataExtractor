using Newtonsoft.Json;
using System;

namespace DataExtractor.DataStructures.Converters {
	public class SfxTableIdConverter : JsonConverter {
		public override bool CanRead => false;

		public override bool CanConvert(Type objectType) {
			return objectType == typeof(SFXTableIDField);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
			var id = (SFXTableIDField) value;
			var name = Manager.audio.sfxTable.GetSfxInfo(id.value)?.name;

			writer.WriteValue(name);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
			JsonSerializer serializer) {
			throw new NotImplementedException();
		}
	}
}