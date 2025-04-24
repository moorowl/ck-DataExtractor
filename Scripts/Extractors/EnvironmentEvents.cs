using Newtonsoft.Json.Linq;
using System.Linq;
using UnityEngine;

namespace DataExtractor.Extractors {
	public class EnvironmentEvents : Extractor {
		public override string Name => "environmentEvents";

		public override JToken Extract() {
			var environmentEvents = Resources.Load<EnvironmentEventsTable>("EnvironmentEventsTable");

			var data = environmentEvents.eventRequirements.OrderBy(requirements => (int) requirements.eventType).Select(
				requirements => {
					return new {
						Id = (int) requirements.eventType,
						InternalName = requirements.eventType,
						Biomes = requirements.biomes,
						MinDistanceFromCore = requirements.minDistanceFromCore,
						MaxAmountOfNearbyObjects = requirements.maxAmountOfNearbyObjects,
						TileRequirements = requirements.tileRequirements.Select(tileRequirement => new {
							TileType = tileRequirement.tileType,
							Tilesets = tileRequirement.tilesets,
							MinAmountOfTiles = tileRequirement.minimumAmountOfTiles
						}),
						MinTotalTilesFulfillingRequirements = requirements.minTotalTilesFulfillingRequirements,
					};
				});

			return Serialize(data);
		}
	}
}