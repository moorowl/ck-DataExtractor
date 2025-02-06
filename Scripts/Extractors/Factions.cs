using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DataExtractor.Extractors {
	public class Factions : Extractor {
		public override string Name => "factions";

		public override JToken Extract() {
			var factions = Enum.GetValues(typeof(FactionID)).Cast<FactionID>()
				.OrderBy(faction => (int) faction)
				.Where(faction => faction != FactionID.__MAX_VALUE)
				.ToList();
			var canAttackMatrix = Manager.mod.FactionsLookupMatrix;

			var data = factions.Select(faction => {
				var canAttack = factions.Where(attackableFaction => canAttackMatrix[(int) faction, (int) attackableFaction]).ToList();

				return new {
					Id = (int) faction,
					InternalName = faction,
					DisplayName = Regex.Replace(faction.ToString(), "[a-z][A-Z]", x => x.Value[0] + " " + char.ToLower(x.Value[1])),
					CanAttack = canAttack,
					CannotAttack = factions.Except(canAttack)
				};
			});

			return Serialize(data);
		}
	}
}