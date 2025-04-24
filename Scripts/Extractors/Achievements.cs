using System;
using System.Linq;
using DataExtractor.Utilities;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace DataExtractor.Extractors {
	public class Achievements : Extractor {
		public override string Name => "achievements";

		public override JToken Extract() {
			var achievements = Enum.GetValues(typeof(AchievementID))
				.Cast<AchievementID>()
				.OrderBy(achievement => (int) achievement);
			var achievementsMapper = Resources.Load<AchievementsMapper>("AchievementsMapper");

			var data = achievements.Select(achievement => {
				var internalName = achievement.ToString();
				var data = achievementsMapper.GetAchievementData(achievement);

				return new {
					Id = (int) achievement,
					InternalName = internalName,
					DisplayName = Utils.GetText("Achievements/" + internalName),
					LockedDescription = Utils.GetText("AchievementsLockedDescription/" + internalName),
					UnlockedDescription = Utils.GetText("AchievementsUnlockedDescription/" + internalName),
					Identifiers = data != null
						? new {
							Steam = data.SteamID,
							Epic = data.EpicID,
							GOG = data.GOGID,
							Ps4 = data.Ps4Id,
							Ps5 = data.Ps5Id,
							Xbox = data.XboxId
						}
						: null
				};
			});

			return Serialize(data);
		}
	}
}