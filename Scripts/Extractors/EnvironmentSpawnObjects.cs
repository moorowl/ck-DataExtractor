using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using PugTilemap;
using System.Linq;

namespace DataExtractor.Extractors {
	public class EnvironmentSpawnObjects : Extractor {
		public override string Name => "environmentSpawnObjects";

		public override JToken Extract() {
			var spawnTable = Manager.mod.SpawnTable;

			var data = new {
				Initial = spawnTable.spawnObjects.Select(spawnObject => {
					var spawnCheck = spawnObject.spawnCheck;
					var spawnChance = spawnCheck.spawnChance;

					return new {
						SpawnCheck = new {
							SpawnChance = new {
								Source = spawnChance.source,
								Values = (object) (spawnChance.source == EnvironmentalSpawnChance.Source.Constant ? spawnChance.constantValue : spawnChance.worldGenDependentValue)
							},
							Biome = spawnCheck.biome,
							TileType = spawnCheck.tileType,
							Tilesets = spawnCheck.tilesets,
							AdjacentTiles = spawnCheck.adjacentTiles.list.Select(adjacentTile => new {
								TileType = adjacentTile.tileType,
								Tileset = adjacentTile.mustAlsoMatchTileset ? adjacentTile.tileset : (Tileset?) null
							}),
							CanSpawnInBlockedArea = spawnCheck.canSpawnInBlockedArea,
							SkipSpawnForPartialMaps = spawnCheck.skipSpawnForPartialMaps
						},
						Spawns = spawnObject.spawns.Select(spawn => {
							var variations = new List<ValueWithWeight<int>>();
							if (spawn.advancedVariationControl) {
								variations.AddRange(spawn.weightedVariations.value);
							} else {
								for (var i = spawn.variation.min; i <= spawn.variation.max; i++)
									variations.Add(new ValueWithWeight<int>(i, 1f));
							}
							
							return new {
								SpawnType = spawn.spawnType,
								Id = spawn.objectID,
								Variation = variations.Select(variation => new {
									Variation = variation.value,
									Weight = variation.weight
								}),
								Amount = spawn.amount,
								ClusterSpawnChance = spawn.spawnType == EnvironmentSpawnType.Cluster
									? spawn.clusterSpawnChance
									: (float?) null,
								ClusterSpreadChance = spawn.spawnType == EnvironmentSpawnType.Cluster
									? spawn.clusterSpreadChance
									: (float?) null,
								ClusterSpreadFourWayOnly = spawn.spawnType == EnvironmentSpawnType.Cluster
									? spawn.clusterSpreadFourWayOnly
									: (bool?) null
							};
						}),
					};
				}),
				Respawn = spawnTable.respawnObjects.Select(respawnObject => {
					var spawnCheck = respawnObject.spawnCheck;
					var spawnChance = spawnCheck.spawnChance;

					return new {
						SpawnCheck = new {
							SpawnChance = new {
								Source = spawnChance.source,
								Values = (object) (spawnChance.source == EnvironmentalSpawnChance.Source.Constant ? spawnChance.constantValue : spawnChance.worldGenDependentValue)
							},
							Biome = spawnCheck.biome,
							SpawnChanceDecay = spawnCheck.spawnChanceDecay,
							MaxSpawnPerTile = spawnCheck.maxSpawnPerTile,
							MaxSpawnsPerRespawn = spawnCheck.maxSpawnsPerRespawn,
							MinTilesRequired = spawnCheck.minTilesRequired,
							TileType = spawnCheck.tileType,
							Tilesets = spawnCheck.tilesets,
							AdjacentTiles = spawnCheck.adjacentTiles.list.Select(adjacentTile => new {
								TileType = adjacentTile.tileType,
								Tileset = adjacentTile.mustAlsoMatchTileset ? adjacentTile.tileset : (Tileset?) null
							})
						},
						Spawns = respawnObject.spawns.Select(spawn => {
							var variations = new List<ValueWithWeight<int>>();
							if (spawn.advancedVariationControl) {
								variations.AddRange(spawn.weightedVariations.value);
							} else {
								for (var i = spawn.variation.min; i <= spawn.variation.max; i++)
									variations.Add(new ValueWithWeight<int>(i, 1f));
							}
							
							return new {
								SpawnType = spawn.spawnType,
								Id = spawn.objectID,
								Variation = variations.Select(variation => new {
									Variation = variation.value,
									Weight = variation.weight
								}),
								Amount = spawn.amount,
								ClusterSpawnChance = spawn.spawnType == EnvironmentSpawnType.Cluster
									? spawn.clusterSpawnChance
									: (float?) null,
								ClusterSpreadChance = spawn.spawnType == EnvironmentSpawnType.Cluster
									? spawn.clusterSpreadChance
									: (float?) null,
								ClusterSpreadFourWayOnly = spawn.spawnType == EnvironmentSpawnType.Cluster
									? spawn.clusterSpreadFourWayOnly
									: (bool?) null
							};
						})
					};
				})
			};

			return Serialize(data);
		}
	}
}