using Newtonsoft.Json.Linq;
using System.Linq;

namespace DataExtractor.Extractors {
	public class Music : Extractor {
		public override string Name => "music";

		public override JToken Extract() {
			var data = new {
				Rosters = Manager.music.musicRosters.Select(roster => new {
					InternalName = roster.rosterType,
					MusicType = roster.musicType,
					Tracks = roster.tracks.Select(track => new {
						Audio = track.trackAssetReference?.LoadAssetAsync().WaitForCompletion()?.name,
						IntroAudio = track.introAssetReference?.LoadAssetAsync().WaitForCompletion()?.name
					})
				})
			};

			return Serialize(data);
		}
	}
}