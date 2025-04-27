using System.Linq;
using DataExtractor.Utilities;
using Newtonsoft.Json.Linq;

namespace DataExtractor.Extractors {
	public class Pets : Extractor {
		public override string Name => "pets";

		public override JToken Extract() {
			var petsTable = Manager.ui.petInfosTable;

			var data = new {
				Talents = petsTable.petTalents.OrderBy(info => (int) info.petTalentID).Select(info => {
					var id = info.petTalentID;

					return new {
						Id = (int) id,
						InternalName = id,
						DisplayName = new {
							Melee = Utils.GetTranslations($"PetTalents/{id}Melee"),
							Range = Utils.GetTranslations($"PetTalents/{id}Range"),
							Buff = Utils.GetTranslations($"PetTalents/{id}Buff")
						},
						Icon = new {
							Melee = info.meleeIcon?.name,
							Range = info.rangeIcon?.name,
							Buff = info.buffIcon?.name
						},
						Condition = info.conditionID,
						Value = info.value,
						BuffValue = info.buffValue,
						MultiplierOverrides = info.multiplierOverrides.Select(ov => new {
							Pet = ov.petId,
							Multiplier = ov.multiplier
						})
					};
				}),
				Skins = petsTable.petSkins.Select(info => new {
					Pet = info.petId,
					Palettes = info.skins.Select(skin => skin.primaryGradientMap?.name)
				})
			};

			return Serialize(data);
		}
	}
}