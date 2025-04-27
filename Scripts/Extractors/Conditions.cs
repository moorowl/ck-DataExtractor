using System;
using System.Collections.Generic;
using System.Linq;
using DataExtractor.Utilities;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace DataExtractor.Extractors {
	public class Conditions : Extractor {
		private static readonly Lazy<HashSet<ConditionID>> IsUsedBySetBonus = new(() => {
			var setBonusesTable = Resources.Load<SetBonusesTable>("SetBonusesTable");
			var list = new HashSet<ConditionID>();

			foreach (var setBonus in setBonusesTable.setBonuses) {
				foreach (var data in setBonus.setBonusDatas) {
					list.Add(data.conditionData.conditionID);
				}
			}

			return list;
		});

		public override string Name => "conditions";

		public override JToken Extract() {
			var conditions = ConditionsTable.GetTable().conditionCategories
				.SelectMany(category => category.conditions)
				.OrderBy(condition => (int) condition.Id);
			var conditionEffects = Enum.GetValues(typeof(ConditionEffect)).Cast<ConditionEffect>();

			var setBonusesTable = Resources.Load<SetBonusesTable>("SetBonusesTable");
			setBonusesTable.UpdateSetBonusDatas();

			var data = conditions.Select(condition => {
				var id = condition.Id;
				var effect = condition.effect;

				var idForDescription = (condition.useSameDescAsId != 0) ? condition.useSameDescAsId : id;
				var description = Utils.GetTranslations("Conditions/" + idForDescription);
				var englishDescription = Utils.GetTranslation("Conditions/" + idForDescription, "English");
				var descriptionIsAPercentage = IsDescriptionPercentage(englishDescription);

				var descriptionForSetBonus = IsUsedBySetBonus.Value.Contains(id) ? Utils.GetTranslations("Conditions/" + id) : null;
				var englishDescriptionForSetBonus = IsUsedBySetBonus.Value.Contains(id) ? Utils.GetTranslation("Conditions/" + id, "English") : null;
				
				if (englishDescriptionForSetBonus == englishDescription)
					descriptionForSetBonus = null;

				return new {
					Id = (int) id,
					InternalName = id,
					Description = description,
					DescriptionForSetBonus = descriptionForSetBonus,
					DescriptionIsAPercentage = descriptionIsAPercentage,
					Effect = effect,
					Icon = condition.icon?.name,
					IsAdditiveWithSelf = condition.isAdditiveWithSelf,
					IsPermanent = condition.isPermanent,
					IsNegative = condition.isNegative,
					IsUnique = condition.isUnique,
					IsInheritedByProjectiles = condition.isInheritedByProjectiles,
					ShowDecimal = condition.showDecimal,
					SkipShowingSignInfrontOfValue = condition.skipShowingSignInfrontOfValue,
					SkipShowingStatText = condition.skipShowingStatText
				};
			});

			return Serialize(data);
		}

		private static bool IsDescriptionPercentage(string description) {
			if (string.IsNullOrEmpty(description))
				return false;

			var formatArgIndex = description.IndexOf("{0}", StringComparison.Ordinal);
			if (formatArgIndex >= 0)
				return formatArgIndex + 3 < description.Length && description[formatArgIndex + 3] == '%';

			return false;
		}
	}
}