using Newtonsoft.Json.Linq;
using System.Linq;
using DataExtractor.Utilities.Extensions;

namespace DataExtractor.Extractors {
	public class Miscellaneous : Extractor {
		public override string Name => "miscellaneous";

		public override JToken Extract() {
			var data = new {
				GlowColors = Manager.effects.glowColors.ToDictionary(entry => entry.glowCondition, entry => entry.color.ToHex()),
				PlayerColors = Manager.ui.playerColors.Select(color => color.ToHex())
			};

			return Serialize(data);
		}
	}
}