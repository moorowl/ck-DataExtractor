using DataExtractor.Utilities;
using DataExtractor.Utilities.Extensions;
using Newtonsoft.Json.Linq;
using PugTilemap;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;

namespace DataExtractor.Extractors {
	public class Tiles : Extractor {
		public override string Name => "tiles";

		public override JToken Extract() {
			var tilesets = Enum.GetValues(typeof(Tileset)).Cast<Tileset>()
				.OrderBy(tileset => (int) tileset)
				.ToList();
			var tileTypes = Enum.GetValues(typeof(TileType)).Cast<TileType>()
				.OrderBy(tileType => (int) tileType)
				.ToList();

			var colors = new Dictionary<Tileset, Dictionary<TileType, string>>();

			foreach (var tileset in tilesets) {
				foreach (var tileType in tileTypes) {
					if (!Utils.TryGetTileColor(tileset, tileType, out var color))
						continue;

					if (!colors.ContainsKey(tileset))
						colors[tileset] = new Dictionary<TileType, string>();

					colors[tileset][tileType] = color.ToHex();
				}
			}

			var data = new {
				Tilesets = tilesets.Select(tileset => new {
					Id = (int) tileset,
					InternalName = tileset
				}),
				TileTypes = tileTypes.Select(tileType => {
					var neededTiles = new NativeList<TileType>(4, Allocator.Temp);
					var invalidTiles = new NativeList<TileType>(4, Allocator.Temp);

					tileType.GetNeededTile(ref neededTiles);
					tileType.GetInvalidTile(ref invalidTiles);

					return new {
						Id = (int) tileType,
						InternalName = tileType,
						Properties = new {
							SurfacePriority = tileType.GetSurfacePriorityFromJob(),
							IsBaseGround = tileType.IsBaseGroundTile(),
							IsNonSolid = tileType.IsNonSolidTile(),
							IsWall = tileType.IsWallTile(),
							IsWallOrThinWall = tileType.IsWallOrThinWall(),
							IsWalkable = tileType.IsWalkableTile(),
							IsContainedResource = tileType.IsContainedResource(),
							IsLowCollider = tileType.IsLowCollider(),
							IsThinCollider = tileType.HasThinCollider(),
							IsMediumCollider = tileType.HasMediumCollider(),
							IsDamageable = tileType.IsDamageableTile(),
							IsBlocking = tileType.IsBlockingTile(),
							IsBlockingWithoutLow = tileType.IsBlockingTile(false),
							IsBlockingOnWalkable = tileType.IsBlockingOnWalkableTile(),
							IsBlockingParticles = tileType.IsBlockingParticlesTile(),
							BlockingAdaptsToAllTilesets = tileType.BlockingAdaptsToAllTilesets(),
							CanSpawnCritter = tileType.CanSpawnCritter(),
							CantBeDugWithShovelWhileStandingOn = tileType.CantBeDugWithShovelWhileStandingOn(),
							CanGrowOn = tileType.CanGrowOn(),
							ShouldUseFenceLikeAdaption = tileType.ShouldUseFenceLikeAdaption(),
							NeededTiles = Enumerable.Range(0, neededTiles.Length).Select(index => neededTiles[index]),
							InvalidTiles = Enumerable.Range(0, invalidTiles.Length).Select(index => invalidTiles[index])
						}
					};
				}),
				MapColors = colors
			};

			return Serialize(data);
		}
	}
}