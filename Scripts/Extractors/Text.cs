using DataExtractor.Utilities;
using I2.Loc;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace DataExtractor.Extractors {
	public class Text : Extractor {
		public override string Name => "text";

		public override JToken Extract() {
			var data = LocalizationManager.GetTermsList()
				.OrderBy(term => term)
				.ToDictionary(term => term, Utils.GetTranslations);

			return Serialize(data);
		}
	}
}