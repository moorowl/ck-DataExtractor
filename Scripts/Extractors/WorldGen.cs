using Newtonsoft.Json.Linq;
using System.Linq;
using UnityEngine;
using PugWorldGen;
using WorldGen;

namespace DataExtractor.Extractors {
	public class WorldGen : Extractor {
		public override string Name => "worldGen";

		public override JToken Extract() {
			var defaultParameters = Manager.worldGen.defaultWorldParameters;
			var tileTypeMapping = Resources.Load<TileTypeMapping>("TileTypeMapping");

			var data = new {
				DefaultParameters = new {
					WorldScale = defaultParameters.worldScale,
					Rings = new object[] {
						new {
							Size = defaultParameters.ring1Size,
							Chaos = defaultParameters.ring1Chaos
						},
						new {
							Size = defaultParameters.ring2Size,
							Chaos = defaultParameters.ring2Chaos
						},
						new {
							Size = defaultParameters.ring3Size,
							Chaos = defaultParameters.ring3Chaos
						},
						new {
							Size = defaultParameters.ring4Size,
							Chaos = defaultParameters.ring4Chaos
						}
					},
					Biomes = new {
						Dirt = MapBiomeParameters(defaultParameters.dirt),
						Clay = MapBiomeParameters(defaultParameters.clay),
						Stone = MapBiomeParameters(defaultParameters.stone),
						Nature = MapBiomeParameters(defaultParameters.forest),
						Sea = MapBiomeParameters(defaultParameters.sea),
						Desert = MapBiomeParameters(defaultParameters.desert),
						Crystal = MapBiomeParameters(defaultParameters.crystal),
						Passage = MapBiomeParameters(defaultParameters.passage),
					}
				},
				TileTypeMapping = new {
					Rules = tileTypeMapping.mapping.Select(rule => new {
						Biome = rule.biome switch {
							PugWorldGen.CoreKeeper.Biome.Dirt => Biome.Slime,
							PugWorldGen.CoreKeeper.Biome.Clay => Biome.Larva,
							PugWorldGen.CoreKeeper.Biome.Stone => Biome.Stone,
							PugWorldGen.CoreKeeper.Biome.Forest => Biome.Nature,
							PugWorldGen.CoreKeeper.Biome.Sea => Biome.Sea,
							PugWorldGen.CoreKeeper.Biome.Desert => Biome.Desert,
							PugWorldGen.CoreKeeper.Biome.Crystal => Biome.Crystal,
							PugWorldGen.CoreKeeper.Biome.Passage => Biome.Passage,
							_ => Biome.None,
						},
						ProceduralTileType = rule.proceduralTileType,
						FloorFlag = rule.floorFlag,
						RoofHoleFlag = rule.roofHoleFlag,
						GreatWallFlag = rule.greatWallFlag,
						ResourceIndex = rule.resourceIndex,
						OutputTile = rule.outputTile,
					})
				}
			};

			return Serialize(data);
		}

		private object MapBiomeParameters(CoreKeeperWorldParameters.BiomeParameters biomeParameters) {
			return new {
				ResourceDistribution = Enumerable.Range(0, biomeParameters.ResourceCount).Select(index => biomeParameters.resourceDistribution[index]),
				ResourceThreshold = biomeParameters.resourceThreshold,
				RiverSize = biomeParameters.riverSize,
				RiverAmount = biomeParameters.riverAmount,
				LakeThreshold = biomeParameters.lakeThreshold,
				ChamberThreshold = biomeParameters.chamberThreshold,
				ScatteredWallThreshold = biomeParameters.scatteredWallThreshold,
				CeilingHoleThreshold = biomeParameters.ceilingHoleThreshold,
				TunnelThreshold = biomeParameters.tunnelThreshold,
				TunnelAmount = biomeParameters.tunnelAmount,
				SandThreshold = biomeParameters.sandThreshold,
				SandAmount = biomeParameters.sandAmount,
				PitThreshold = biomeParameters.pitThreshold,
				BiomeEdgePitSize = biomeParameters.biomeEdgePitSize,
				BiomeEdgePitLedgeSize = biomeParameters.biomeEdgePitLedgeSize,
				BiomeSubTileTreshold = biomeParameters.biomeSubTileTreshold,
				ExplosiveWallAmount = biomeParameters.explosiveWallAmount,
			};
		}
	}
}