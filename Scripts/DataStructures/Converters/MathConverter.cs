using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace DataExtractor.DataStructures.Converters {
	public class MathConverter : JsonConverter {
		public override bool CanRead => false;

		private static readonly List<Type> ConvertibleTypes = new() {
			typeof(Vector2),
			typeof(Vector2Int),
			typeof(Vector3),
			typeof(Vector3Int),
			typeof(int2),
			typeof(int2?),
			typeof(float2),
			typeof(float2?),
			typeof(float3),
			typeof(float3?)
		};

		public override bool CanConvert(Type objectType) {
			return ConvertibleTypes.Contains(objectType);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
			var o = value switch {
				Vector2 vector => new JObject(
					new JProperty("x", vector.x),
					new JProperty("y", vector.y)
				),
				Vector2Int vector => new JObject(
					new JProperty("x", vector.x),
					new JProperty("y", vector.y)
				),
				Vector3 vector => new JObject(
					new JProperty("x", vector.x),
					new JProperty("y", vector.y),
					new JProperty("z", vector.z)
				),
				Vector3Int vector => new JObject(
					new JProperty("x", vector.x),
					new JProperty("y", vector.y),
					new JProperty("z", vector.z)
				),
				int2 vector => new JObject(
					new JProperty("x", vector.x),
					new JProperty("y", vector.y)
				),
				float2 vector => new JObject(
					new JProperty("x", vector.x),
					new JProperty("y", vector.y)
				),
				float3 vector => new JObject(
					new JProperty("x", vector.x),
					new JProperty("y", vector.y),
					new JProperty("z", vector.z)
				),
				_ => throw new NotImplementedException()
			};

			o.WriteTo(writer);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
			JsonSerializer serializer) {
			throw new NotImplementedException();
		}
	}
}