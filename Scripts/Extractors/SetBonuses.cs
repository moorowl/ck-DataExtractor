using Newtonsoft.Json.Linq;
using System.Linq;
using UnityEngine;

namespace DataExtractor.Extractors {
	public class SetBonuses : Extractor {
		public override string Name => "setBonuses";

		public override JToken Extract() {
			var setBonusesTable = Resources.Load<SetBonusesTable>("SetBonusesTable");
			setBonusesTable.UpdateSetBonusDatas();

			var data = setBonusesTable.setBonuses.OrderBy(info => (int) info.setBonusID).Select(info => {
				var id = info.setBonusID;

				return new {
					Id = (int) id,
					InternalName = id,
					Level = LevelScaling.GetLevelFromAreaLevelAndRarity(info.areaLevel, info.rarity),
					AvailablePieces = info.availablePieces,
					Bonuses = info.setBonusDatas.Select(bonus => new {
						RequiredPieces = bonus.requiredPieces,
						Condition = new {
							Id = bonus.conditionData.conditionID,
							Value = bonus.conditionData.value
						}
					})
				};
			});

			return Serialize(data);
		}
	}
}