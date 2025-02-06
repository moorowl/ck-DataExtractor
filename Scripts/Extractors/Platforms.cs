using System.Globalization;
using System;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json.Linq;

namespace DataExtractor.Extractors {
	public class Platforms : Extractor {
		public override string Name => "platforms";

		public override JToken Extract() {
			var platformConfigurations = Resources.LoadAll<PlatformConfiguration>("Platform");
			
			var data = platformConfigurations.Select(config => new {
				PlatformVariant = config.name.Replace("PlatformConfiguration_", ""),
				PerformanceDeviceProfile = config.PerformanceDeviceProfile,
				SessionConfiguration = config.SessionConfiguration
			});

			return Serialize(data);
		}
	}
}