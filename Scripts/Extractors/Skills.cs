using DataExtractor.Utilities;
using DataExtractor.Utilities.Extensions;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace DataExtractor.Extractors {
	public class Skills : Extractor {
		private static readonly MethodInfo GetSkillMulFactor = typeof(SkillExtensions).GetMethod("GetSkillMulFactor", BindingFlags.NonPublic | BindingFlags.Static);
		private static readonly MethodInfo GetSkillBase = typeof(SkillExtensions).GetMethod("GetSkillBase", BindingFlags.NonPublic | BindingFlags.Static);

		public override string Name => "skills";

		public override JToken Extract() {
			var skillTalentsTable = Manager.mod.SkillTalentsTable;
			var skillIconsTable = Resources.Load<SkillIconsTable>("SkillIconsTable");

			var data = skillTalentsTable.skillTalentTrees.OrderBy(info => (int) info.skillID).Select(info => {
				var id = info.skillID;
				var condition = SkillExtensions.GetConditionDataForSkill(id, SkillExtensions.GetSkillFromLevel(id, 1));
				var iconSet = skillIconsTable.GetIcon(id);

				return new {
					Id = (int) id,
					InternalName = id,
					DisplayName = Utils.GetText($"Skills/{id}"),
					Description = Utils.GetText($"Skills/{id}Desc"),
					Icon = iconSet.icon?.name,
					goldIcon = iconSet.goldIcon?.name,
					IconColor = Manager.ui.GetSkillColor(id).ToHex(),
					Condition = condition.conditionID,
					ConditionValuePerLevel = condition.value,
					MulFactor = GetSkillMulFactor.Invoke(null, new object[] { id }),
					BaseFactor = GetSkillBase.Invoke(null, new object[] { id }),
					MaxLevel = SkillExtensions.GetMaxSkillLevel(id),
					Talents = info.skillTalents.Select(talent => new {
						InternalName = talent.name,
						DisplayName = Utils.GetText($"SkillTalents/{talent.name}"),
						Condition = talent.givesCondition,
						ConditionValuePerPoint = talent.conditionValuePerPoint,
						Icon = talent.icon?.name
					})
				};
			});

			return Serialize(data);
		}
	}
}