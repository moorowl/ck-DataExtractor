using System.Globalization;
using System;
using UnityEngine;
using Newtonsoft.Json.Linq;

namespace DataExtractor.Extractors {
	public class Version : Extractor {
		public override string Name => "version";

		public override JToken Extract() {
			var data = new {
				Version = Application.version,
				Branch = GetBranch(),
				UnityVersion = Application.unityVersion,
				BuildDate = GetBuildDate().ToString("O", CultureInfo.InvariantCulture)
			};

			return Serialize(data);
		}

		private static DateTime GetBuildDate() {
			var buildDateAsset = Resources.Load<TextAsset>("date");
			return DateTime.SpecifyKind(DateTime.ParseExact(buildDateAsset.text, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture), DateTimeKind.Utc);
		}

		private static string GetBranch() {
			if (Manager.platform.platformImpl is SteamPlatform steam && !string.IsNullOrEmpty(steam.BetaBranch))
				return steam.BetaBranch;

			return "public";
		}
	}
}