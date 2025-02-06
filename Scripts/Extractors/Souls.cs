using DataExtractor.Utilities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataExtractor.Extractors {
	public class Souls : Extractor {
		public override string Name => "souls";

		public override JToken Extract() {
			var souls = Enum.GetValues(typeof(SoulID)).Cast<SoulID>().OrderBy(soul => (int) soul).Where(soul => soul != SoulID.__MAX_VALUE);
			var soulUiElements = Manager.ui.characterWindow.soulsWindow.GetComponentsInChildren<SoulsUIElement>()
				.Where(element => element.soulID != SoulID.None)
				.ToDictionary(element => element.soulID, element => element);

			var data = souls.Select(soul => {
				var condition = SoulsExtensions.GetSoulConditionData(soul);
				var hasCondition = condition.conditionID != ConditionID.None;
				var uiElement = soulUiElements.GetValueOrDefault(soul);

				return new {
					Id = (int) soul,
					InternalName = soul,
					DisplayName = uiElement != null ? Utils.GetText(uiElement.soulTitle.mTerm) : null,
					Description = uiElement != null ? Utils.GetText(uiElement.soulDesc.mTerm) : null,
					Condition = hasCondition ? new { Id = condition.conditionID, Value = condition.value } : null
				};
			});

			return Serialize(data);
		}
	}
}