using System.Linq;
using DataExtractor.Utilities;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace DataExtractor.Extractors {
	public class Credits : Extractor {
		public override string Name => "credits";

		public override JToken Extract() {
			var creditsData = Resources.Load<CreditsData>("CreditsData");

			var data = creditsData.creditsElements.Select(element => {
				return new {
					Layout = element.layout,
					Title = element.skipTitle ? null : Utils.GetText(CreditsData.TitleToTerm(element.title)),
					Names = element.creditNames.Select(name => Utils.GetText($"CreditsNames/{name}")),
					UseCommaSeparationForNames = element.useCommaSeparationForNames,
					EndWithComma = element.endWithComma,
				};
			});

			return Serialize(data);
		}
	}
}