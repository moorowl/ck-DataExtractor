using DataExtractor.Utilities;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace DataExtractor.Extractors {
	public class Roles : Extractor {
		public override string Name => "roles";

		public override JToken Extract() {
			var rolePerksTable = ((CharacterCustomizationMenu) Manager.menu.characterCustomizationMenu).roleSelection.perksTable;

			var data = rolePerksTable.perks.OrderBy(perks => (int) perks.role).Select(perks => {
				var role = perks.role;
				// This is hardcoded in PlayerController
				var starterSkill = (SkillID?) (role != CharacterRole.Nomad ? perks.starterSkill : null);

				return new {
					Id = (int) role,
					InternalName = role,
					DisplayName = Utils.GetText($"Roles/{role}"),
					Description = Utils.GetText($"Roles/{role}Desc"),
					StarterSkill = starterSkill,
					StarterItems = perks.starterItems.Select(item => new {
						Id = item.objectID,
						Variation = item.variation,
						Amount = item.amount
					})
				};
			});

			return Serialize(data);
		}
	}
}