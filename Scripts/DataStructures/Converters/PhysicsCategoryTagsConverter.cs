using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using Unity.Physics.Authoring;

namespace DataExtractor.DataStructures.Converters {
	public class PhysicsCategoryTagsConverter : JsonConverter {
		public override bool CanRead => false;

		public override bool CanConvert(Type objectType) {
			return objectType == typeof(PhysicsCategoryTags);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
			var tags = (PhysicsCategoryTags) value;
			var o = new JObject(
				new JProperty("DefaultCollider", tags.Category00),
				new JProperty("PlayerTrigger", tags.Category01),
				new JProperty("PlayerCollider", tags.Category02),
				new JProperty("EnemyTrigger", tags.Category03),
				new JProperty("EnemyCollider", tags.Category04),
				new JProperty("PickUpObject", tags.Category05),
				new JProperty("DefaultTrigger", tags.Category06),
				new JProperty("LightTrigger", tags.Category07),
				new JProperty("DefaultLowCollider", tags.Category08),
				new JProperty("DefaultLowTrigger", tags.Category09),
				new JProperty("DefaultLowTriggerNonBlocking", tags.Category10),
				new JProperty("ProjectileTrigger", tags.Category11),
				new JProperty("ElectricalTrigger", tags.Category12),
				new JProperty("EnvironmentTrigger", tags.Category13),
				new JProperty("VelocityAffectorTrigger", tags.Category14),
				new JProperty("CritterCollider", tags.Category15),
				new JProperty("PlacementBlocker", tags.Category16),
				new JProperty("Category17", tags.Category17),
				new JProperty("Category18", tags.Category18),
				new JProperty("Category19", tags.Category19),
				new JProperty("Category20", tags.Category20),
				new JProperty("Category21", tags.Category21),
				new JProperty("Category22", tags.Category22),
				new JProperty("Category23", tags.Category23),
				new JProperty("Category24", tags.Category24),
				new JProperty("Category25", tags.Category25),
				new JProperty("Category26", tags.Category26),
				new JProperty("Category27", tags.Category27),
				new JProperty("Category28", tags.Category28),
				new JProperty("Category29", tags.Category29),
				new JProperty("Category30", tags.Category30),
				new JProperty("Category31", tags.Category31)
			);

			o.WriteTo(writer);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
			JsonSerializer serializer) {
			throw new NotImplementedException();
		}
	}
}