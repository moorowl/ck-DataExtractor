using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PugMod;
using System;
using System.Linq;

namespace DataExtractor.DataStructures.Converters {
	public class EnumFlagsConverter : JsonConverter {
		public override bool CanRead => false;

		public override bool CanConvert(Type objectType) {
			if (objectType.IsEnum)
				return true;

			var underlyingType = Nullable.GetUnderlyingType(objectType);
			return underlyingType != null && underlyingType.IsEnum;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
			var underlyingType = Nullable.GetUnderlyingType(value.GetType()) ?? value.GetType();

			if (underlyingType.HasAttributeChecked<FlagsAttribute>()) {
				var flags = value.ToString().Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries).ToList();

				if (flags.Count == 1) {
					if (flags[0] == "-1") {
						flags.Clear();

						foreach (int enumValue in Enum.GetValues(underlyingType)) {
							if (enumValue != 0) {
								flags.Add(Enum.GetName(underlyingType, enumValue));
							}
						}
					} else if (Enum.TryParse(underlyingType, flags[0], out var parsedValue) && (int) parsedValue == 0) {
						flags.Clear();
					}
				}

				new JArray(flags).WriteTo(writer);
			} else {
				writer.WriteValue(value.ToString());
			}
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
			JsonSerializer serializer) {
			throw new NotImplementedException();
		}
	}
}