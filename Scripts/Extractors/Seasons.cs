using DataExtractor.Utilities;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace DataExtractor.Extractors {
	public class Seasons : Extractor {
		public override string Name => "seasons";

		public override JToken Extract() {
			var seasonsTable = Manager.prefs.seasonsTable;

			var data = seasonsTable.seasonDates.OrderBy(info => (int) info.season).Select(info => {
				var id = info.season;
				var defaultDate = info.date;

				return new {
					Id = (int) id,
					InternalName = id,
					DisplayName = Utils.GetTranslations("Seasons/" + id),
					Date = new {
						StartDay = defaultDate.startDay,
						StartMonth = defaultDate.startMonth,
						EndDay = defaultDate.endDay,
						EndMonth = defaultDate.endMonth
					},
					SpecificYearDates = info.specificYearDates.Where(specificYearDate => !DatesMatch(specificYearDate.date, defaultDate)).Select(specificYearDate => new {
						Year = specificYearDate.year,
						Date = new {
							StartDay = specificYearDate.date.startDay,
							StartMonth = specificYearDate.date.startMonth,
							EndDay = specificYearDate.date.endDay,
							EndMonth = specificYearDate.date.endMonth
						}
					})
				};
			});

			return Serialize(data);
		}

		private static bool DatesMatch(SeasonsTable.SeasonsDate a, SeasonsTable.SeasonsDate b) {
			return a.startDay == b.startDay && a.endDay == b.endDay && a.startMonth == b.startMonth && a.endMonth == b.endMonth;
		}
	}
}