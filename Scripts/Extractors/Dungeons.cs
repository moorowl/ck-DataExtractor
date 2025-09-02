using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Pug.UnityExtensions;
using PugMod;
using PugWorldGen;
using Unity.Mathematics;
using UnityEngine;
using static PugWorldGen.DungeonSpawnTemplateAuthoring;

namespace DataExtractor.Extractors {
	public class Dungeons : Extractor {
		public override string Name => "dungeons";

		public override JToken Extract() {
			// Random dungeons
			var dungeonSpawnTable = Manager.ecs.serverAuthoringPrefab.GetComponentInChildren<DungeonSpawnTable>();
			// Pre-spawned dungeons
			var worldGenAuthorings = Manager.ecs.serverAuthoringPrefab
				.GetComponentsInChildren(typeof(PugWorldGenAuthoring))
				.Cast<PugWorldGenAuthoring>();

			var data = new {
				Unique = worldGenAuthorings.Select(authoring => {
					var markerData = authoring.markerPrefab?.GetComponent<EntityMonoBehaviourData>()?.objectInfo;

					return new {
						InternalName = authoring.name,
						ContentBundle = authoring.contentBundle,
						SpawnImmediatelyOnLoad = authoring.spawnImmediatelyOnLoad,
						PlacementType = authoring.placementType,
						Position = (WorldGenerationTypeDependentValue<int2>?) (authoring.placementType == UniqueScenePlacementType.ExactPosition ? authoring.spawnPosition : null),
						BiomeToSpawnIn = (WorldGenerationTypeDependentValue<Biome>?) ((authoring.placementType == UniqueScenePlacementType.AnywhereInBiome || authoring.placementType == UniqueScenePlacementType.DistanceFromCoreInBiome) ? authoring.biome : null),
						TargetDistanceFromCore = (WorldGenerationTypeDependentValue<int>?) (authoring.placementType == UniqueScenePlacementType.DistanceFromCoreInBiome ? authoring.targetDistanceFromCore : null),
						Marker = markerData != null
							? new { Id = markerData.objectID, Variation = markerData.variation }
							: null,
						DestroyMarkerAfterSpawn = authoring.destroyMarkerAfterSpawn,
						Dungeon = MapDungeon(authoring.prefab)
					};
				}),
				Random = dungeonSpawnTable.biomes.Select(group => {
					return new {
						InternalName = group.tableName,
						BiomeToSpawnIn = group.biome,
						MinDistanceFromCoreInClassicWorlds = group.minDistanceFromCoreInClassicWorlds,
						AnyDungeonSpawnChance = group.anyDungeonSpawnChance,
						SpawnEntries = group.spawnEntries.Select(spawnEntry => new {
							SpawnWeight = spawnEntry.spawnWeight,
							Dungeon = MapDungeon(spawnEntry.prefab)
						})
					};
				})
			};

			return Serialize(data);
		}

		private object MapDungeon(GameObject gameObject) {
			var dungeonAuthoring = gameObject.GetComponent<DungeonAuthoring>();
			var dungeonSingleCustomSceneAuthoring = gameObject.GetComponent<SingleCustomSceneDungeonAuthoring>();

			var dungeonShapeAuthoring = gameObject.GetComponent<DungeonShapeAuthoring>();
			var dungeonRoomAuthoring = gameObject.GetComponent<DungeonRoomAuthoring>();
			var dungeonPathAuthoring = gameObject.GetComponent<DungeonPathAuthoring>();
			var dungeonSpawnTemplateAuthoring = gameObject.GetComponent<DungeonSpawnTemplateAuthoring>();
			var dungeonCustomScenesAuthoring = gameObject.GetComponent<DungeonCustomScenesAuthoring>();
			var dungeonReplaceObjectsAuthoring = gameObject.GetComponent<DungeonReplaceObjectsAuthoring>();

			return new {
				InternalName = gameObject.name,
				Seed = dungeonAuthoring?.seed ?? 0,
				Radius = dungeonAuthoring?.radius ?? dungeonSingleCustomSceneAuthoring?.reservedRadiusDuringDungeonPlacement,
				DontBlockOtherSpawns = dungeonAuthoring?.dontBlockOtherSpawns ?? false,
				Shape = dungeonShapeAuthoring != null
					? new {
						ShapeAmplitude = dungeonShapeAuthoring.shapeAmplitude,
						ShapeFrequency = dungeonShapeAuthoring.shapeFrequency,
						RoomFillSize = dungeonShapeAuthoring.roomFillSize,
						PathFillSize = dungeonShapeAuthoring.pathFillSize,
						DefineShapeByRooms = dungeonShapeAuthoring.defineShapeByRooms,
						RectShaped = dungeonShapeAuthoring.rectShaped,
						Tiles = dungeonShapeAuthoring.tiles,
						RoomShapeSpawnTemplate = MapSpawnTemplate(dungeonShapeAuthoring.roomShapeSpawnTemplate),
						PathShapeSpawnTemplate = MapSpawnTemplate(dungeonShapeAuthoring.pathShapeSpawnTemplate)
					}
					: null,
				Rooms = dungeonRoomAuthoring?.roomConfigurations?.Select(roomConfig => new {
					Placement = roomConfig.placement,
					RoomType = roomConfig.roomType,
					Amount = roomConfig.amount,
					Size = roomConfig.size,
					Spacing = roomConfig.spacing,
					Angle = roomConfig.angle,
					AlignAngleWithCoreRadial = roomConfig.alignAngleWithCoreRadial,
					StraightPaths = roomConfig.straightPaths,
					CanIntersectOtherRooms = roomConfig.canIntersectOtherRooms,
					IntersectableRooms = roomConfig.intersectableRooms
				}),
				Paths = dungeonPathAuthoring?.pathConfigurations?.Select(pathConfig => new {
					Placement = pathConfig.placement,
					RoomType = pathConfig.roomType,
					Amount = pathConfig.amount,
					CanIntersectPaths = pathConfig.canIntersectPaths,
					IntersectablePaths = pathConfig.intersectablePaths,
					CanIntersectRooms = pathConfig.canIntersectRooms,
					IntersectableRooms = pathConfig.intersectableRooms,
					StraightPaths = pathConfig.straightPaths,
					PathStartRoomType = pathConfig.pathStartRoomType,
					PathEndRoomType = pathConfig.pathEndRoomType,
					Width = pathConfig.width
				}),
				SpawnTemplate = dungeonSpawnTemplateAuthoring != null
					? new {
						NodeTemplates =
							dungeonSpawnTemplateAuthoring.nodeTemplates?.Select(MapSpawnTemplateConfiguration),
						PathTemplates =
							dungeonSpawnTemplateAuthoring.pathTemplates?.Select(MapSpawnTemplateConfiguration)
					}
					: null,
				CustomScenes = dungeonCustomScenesAuthoring?.groups?.Select(group => new {
					RoomType = group.roomType,
					NumberToSpawn = group.numberToSpawn,
					Scenes = group.scenes.Select(scene => SceneReferenceToName(scene.scene)),
				}),
				CustomScene = dungeonSingleCustomSceneAuthoring != null ? SceneReferenceToName(dungeonSingleCustomSceneAuthoring.scene) : null,
				ObjectReplacements = dungeonReplaceObjectsAuthoring?.replacements?.Select(replacement => {
					var variations = new List<ValueWithWeight<int>>();

					if (replacement.advancedVariationControl) {
						variations.AddRange(replacement.weightedVariations.value);
					} else {
						for (var i = replacement.variation.min; i <= replacement.variation.max; i++)
							variations.Add(new ValueWithWeight<int>(i, 1f));
					}
					
					return new {
						SourceId = replacement.replaceID,
						TargetId = replacement.replaceID,
						TargetVariation = variations.Select(variation => new {
							Variation = variation.value,
							Weight = variation.weight
						})
					};
				})
			};
		}

		private object MapSpawnTemplateConfiguration(SpawnTemplateConfiguration config) {
			return new {
				Flags = config.flags,
				MinimumSizeRequirement = config.minimumSizeRequirement,
				Templates = config.templates?.Select(MapSpawnTemplate)
			};
		}

		private object MapSpawnTemplate(SpawnTemplate template) {
			if (template == null)
				return null;

			return new {
				InternalName = template.name,
				ShapeAmplitude = template.shapeAmplitude,
				ShapeFrequency = template.shapeFrequency,
				SpawnEntries = template.spawnEntries?.Select(spawnEntry => {
					var variations = new List<ValueWithWeight<int>>();

					if (spawnEntry.advancedVariationControl) {
						variations.AddRange(spawnEntry.weightedVariations);
					} else {
						for (var i = spawnEntry.variation.min; i <= spawnEntry.variation.max; i++)
							variations.Add(new ValueWithWeight<int>(i, 1f));
					}
					
					return new {
						ObjectId = spawnEntry.objectID,
						ObjectAmount = spawnEntry.objectAmount,
						ObjectVariation = variations.Select(variation => new {
							Variation = variation.value,
							Weight = variation.weight
						}),
						ChanceToAppear = spawnEntry.chanceToAppearAtAll,
						CanSpawnOn = spawnEntry.canSpawnOn,
						CanSpawnNextTo = spawnEntry.canSpawnNextTo,
						CannotSpawnOnAnyPreviousObject = spawnEntry.cannotSpawnOnAnyPreviousObject,
						CannotSpawnOn = spawnEntry.canNotSpawnOn,
						ContainLoot = spawnEntry.containLoot,
						Algorithm = spawnEntry.algorithm,
						PlacementAlignment = spawnEntry.placementAlignement,
						RandomPlacement = spawnEntry.randomPlacement,
						RandomChance = spawnEntry.randomChance,
						ShapeSize = spawnEntry.shapeSize,
						Amount = spawnEntry.amount,
						PlacementRadius = spawnEntry.placementRadius,
						RotateTowardsCenter = spawnEntry.rotateTowardsCenter,
						Tilt = spawnEntry.tilt,
						Oblongness = spawnEntry.oblongness
					};
				}),
			};
		}

		private static string SceneReferenceToName(SceneReference reference) {
			var path = reference.ScenePath;
			var a = path.LastIndexOf('/') + 1;
			var b = path.LastIndexOf('.');
			if (b < 0)
				b = path.Length;

			return path.Substring(a, b - a);
		}
	}
}