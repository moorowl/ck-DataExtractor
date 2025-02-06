using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataExtractor.Extractors {
	public class Enums : Extractor {
		public override string Name => "enums";

		private static readonly List<Type> EnumTypes = new() {
			typeof(ObjectID),
			typeof(SoulID),
			typeof(StateID),
			typeof(PuffID),
			typeof(Biome),
			typeof(Rarity),
			typeof(AreaLevel),
			typeof(PaintableColor),
			typeof(ObjectType),
			typeof(ObjectCategoryTag)
		};

		public override JToken Extract() {
			var data = EnumTypes.OrderBy(type => type.Name).Select(type => {
				var name = type.Name;
				var values = new Dictionary<string, int>();

				foreach (var value in Enum.GetValues(type))
					values[value.ToString()] = (int) value;

				return (name, values);
			}).ToDictionary(info => info.name, info => info.values);

			return Serialize(data);
		}
	}
}