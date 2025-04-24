using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace DataExtractor.Extractors {
	public class ConditionEffects : Extractor {
		public override string Name => "conditionEffects";

		public override JToken Extract() {
			var conditionEffects = Enum.GetValues(typeof(ConditionEffect)).Cast<ConditionEffect>()
				.OrderBy(effect => (int) effect);

			var data = conditionEffects.Select(effect => new {
				Id = (int) effect,
				InternalName = effect
			});

			return Serialize(data);
		}
	}
}