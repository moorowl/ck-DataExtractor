using DataExtractor.DataStructures;
using DataExtractor.Utilities;
using DataExtractor.Utilities.Extensions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

namespace DataExtractor.Extractors {
	public class Objects : Extractor {
		public override string Name => "objects";

		public override JToken Extract() {
			var prefabs = Manager.ecs.pugDatabase.prefabList;
			var objects = prefabs.Select(prefab => new ObjectDataFromPrefab(prefab.gameObject)).OrderBy(data => data.SortKey);

			var objectsLookup = objects.GroupBy(data => data.ObjectInfo.objectID).Select(group => group.First()).ToDictionary(data => data.ObjectInfo.objectID, data => data);

			var data = objects
				.Where(data => !data.ObjectInfo.isCustomScenePrefab)
				.Select(data => {
					var objectInfo = data.ObjectInfo;
					var objectData = new ObjectDataCD {
						objectID = objectInfo.objectID,
						variation = objectInfo.variation,
						amount = 1
					};

					var serializedObjectInfo = Serialize(objectInfo, DefaultSettingsForObjects) as JObject;
					var serializedComponents = data.Components.OrderBy(entry => entry.Key.Name)
						.ToDictionary(entry => entry.Key.Name, entry => Serialize(entry.Value, DefaultSettingsForObjects) as JObject);

					// Post processing
					var (icon, smallIcon) = Utils.GetObjectIcons(objectInfo);
					if (icon != null)
						serializedObjectInfo["icon"] = icon.name;
					if (smallIcon != null)
						serializedObjectInfo["smallIcon"] = smallIcon.name;

					serializedObjectInfo["sellValue"] = GetCoinValue(data, objectsLookup, false);

					serializedObjectInfo.Remove("appearInMapUI");
					if (data.TryGetComponent<TileAuthoring>(out var tileAuthoring) && Utils.TryGetTileColor(tileAuthoring.tileset, tileAuthoring.tileType, out var color) && color.a > 0f) {
						serializedObjectInfo["mapColor"] = color.ToHex();
					} else if (!objectInfo.appearInMapUI || (serializedObjectInfo.ContainsKey("mapColor") && objectInfo.mapColor.a == 0f)) {
						serializedObjectInfo.Remove("mapColor");
					}

					var upgradedComponents = new List<object>();
					if (data.TryGetComponent<AreaLevelAuthoring>(out var levelAuthoring)) {
						for (var level = levelAuthoring.level; level <= LevelScaling.GetMaxLevel(); level++) {
							var hasUpgradedWeaponDamageAuthoring = GetWeaponDamageUpgrade(data, level, out var upgradedDamage);
							var hasUpgradedConditionsAuthoring = GetEquippedConditionsUpgrade(data, level, out var upgradedConditions);
							var hasUpgradedExplosivesAuthoring = GetExplosiveUpgrade(data, level, out var upgradedExplosiveDamage, out var upgradedMiningDamage);
							var hasUpgradedManaCostAuthoring = GetManaCostUpgrade(data, level, out var upgradedManaCost);
							var hasUpgradedExtraInventorySizeAuthoring = GetInventorySizeUpgrade(data, level, out var upgradedInventorySize);

							if (!hasUpgradedWeaponDamageAuthoring && !hasUpgradedConditionsAuthoring && !hasUpgradedExplosivesAuthoring)
								continue;

							var components = new Dictionary<string, object>();

							if (hasUpgradedWeaponDamageAuthoring) {
								components[nameof(WeaponDamageAuthoring)] = new {
									damage = upgradedDamage
								};
							}

							if (hasUpgradedConditionsAuthoring) {
								components[nameof(GivesConditionsWhenEquippedAuthoring)] = new {
									givesConditionsWhenEquipped = upgradedConditions
								};
							}

							if (hasUpgradedExplosivesAuthoring) {
								components[nameof(ExplosiveAuthoring)] = new {
									damage = upgradedExplosiveDamage,
									miningDamage = upgradedMiningDamage
								};
							}

							if (hasUpgradedManaCostAuthoring) {
								components[nameof(ConsumesManaAuthoring)] = new {
									manaCost = upgradedManaCost
								};
							}

							if (hasUpgradedExtraInventorySizeAuthoring) {
								components[nameof(ExtraInventorySizeAuthoring)] = new {
									value = upgradedInventorySize
								};
							}

							upgradedComponents.Add(new {
								Level = level,
								Components = components
							});
						}
					}

					return new {
						Id = (int) objectData.objectID,
						InternalName = objectData.objectID,
						AuthoringPrefabName = data.Prefab.name,
						GraphicalPrefabName = objectInfo.prefabInfos[0].prefab?.name,
						Variation = objectData.variation,
						DisplayName = Utils.GetObjectNames(objectData.objectID, objectData.variation),
						Description = Utils.GetObjectDescriptions(objectData.objectID, objectData.variation),
						ObjectInfo = serializedObjectInfo,
						Components = serializedComponents,
						Upgrades = upgradedComponents.Count > 0 ? upgradedComponents : null
					};
				});

			return Serialize(data);
		}

		// adapted from InventoryUtility.GetCoinValue
		private static int GetCoinValue(ObjectDataFromPrefab data, Dictionary<ObjectID, ObjectDataFromPrefab> otherObjects, bool buy) {
			var objectInfo = data.ObjectInfo;
			var objectId = objectInfo.objectID;

			if (objectId == ObjectID.None || data.HasComponent<CantBeSoldAuthoring>() || objectInfo.rarity == Rarity.Legendary)
				return 0;

			var sellValue = objectInfo.sellValue;
			if (sellValue < 0) {
				sellValue = GetRaritySellValue(objectInfo.rarity);
				if (data.HasComponent<CookedFoodAuthoring>()) {
					// removed
				} else {
					var extraSellFromIngredients = 0;
					var requiredObjectsToCraft = objectInfo.requiredObjectsToCraft;

					for (var i = 0; i < requiredObjectsToCraft.Count; i++) {
						var ingredientObjectInfo = otherObjects[requiredObjectsToCraft[i].objectID].ObjectInfo;
						if (ingredientObjectInfo.sellValue != 0)
							extraSellFromIngredients += GetRaritySellValue(ingredientObjectInfo.rarity) * requiredObjectsToCraft[i].amount;
					}

					if (extraSellFromIngredients > 0)
						sellValue = (int) math.round(math.max(1f, sellValue * 0.3f) + extraSellFromIngredients);

					// removed
				}

				var randomization = Unity.Mathematics.Random.CreateFromIndex((uint) objectId).NextFloat(-0.1f, 0.1f);
				sellValue = math.max(1, sellValue + (int) math.round(sellValue * randomization));
			}

			if (buy) {
				sellValue = math.max(1, sellValue);
				float buyValueMultiplier = objectInfo.buyValueMultiplier;
				return (int) math.round(sellValue * 5f * buyValueMultiplier);
			}

			return sellValue;
		}

		private static int GetRaritySellValue(Rarity rarity) {
			return 1 + math.max(0, (int) rarity) * 5;
		}

		#region Upgrade stuff

		private static uint GetUpgradeSeed(ObjectID id) {
			return (uint) id + 1;
		}

		private static bool GetWeaponDamageUpgrade(ObjectDataFromPrefab data, int level, out int upgradedDamage) {
			upgradedDamage = 0;

			if (!data.TryGetComponent<WeaponDamageAuthoring>(out var weaponDamageAuthoring))
				return false;

			upgradedDamage = weaponDamageAuthoring.LevelToDamage(level, GetUpgradeSeed(data.ObjectInfo.objectID), weaponDamageAuthoring.ComputeCooldown());
			return true;
		}

		private static bool GetEquippedConditionsUpgrade(ObjectDataFromPrefab data, int level, out List<EquipmentCondition> upgradedConditions) {
			upgradedConditions = null;

			if (!data.TryGetComponent<GivesConditionsWhenEquippedAuthoring>(out var equippedConditionsAuthoring))
				return false;

			upgradedConditions = new List<EquipmentCondition>();
			upgradedConditions.AddRange(equippedConditionsAuthoring.givesConditionsWhenEquipped);
			upgradedConditions = ConditionExtensions.LevelToEquipmentConditionValues(upgradedConditions,
				new List<ConditionData>(),
				level,
				GetUpgradeSeed(data.ObjectInfo.objectID),
				equippedConditionsAuthoring.givesConditionsWhenHeldInHand,
				equippedConditionsAuthoring.isArmor,
				isEnemy: false,
				ConditionsTable.GetTable()
			);

			return true;
		}

		private static bool GetExplosiveUpgrade(ObjectDataFromPrefab data, int level, out int upgradedDamage, out int upgradedMiningDamage) {
			upgradedDamage = 0;
			upgradedMiningDamage = 0;

			var hasExplosiveAuthoring = data.TryGetComponent<ExplosiveAuthoring>(out var explosiveAuthoring);
			// var hasExplosiveProjectileWithWindup = (data.TryGetComponent<ProjectileAuthoring>(out var projectileAuthoring) && hasExplosiveAuthoring) || (projectileAuthoring != null && projectileAuthoring.mayExplodeWithWindup);
			var hasExplosiveProjectileWithWindup = false;

			if (!hasExplosiveAuthoring && !hasExplosiveProjectileWithWindup)
				return false;

			var damageMultiplier = hasExplosiveAuthoring ? explosiveAuthoring.damageMultiplier : 1f;
			var miningDamageMultiplier = hasExplosiveAuthoring ? explosiveAuthoring.miningDamageMultiplier : 1f;
			var windupDamageMultiplier = hasExplosiveProjectileWithWindup ? 0.3f : 1f;

			upgradedDamage = (int) Math.Round(ExplosiveAuthoring.LevelToExplosionDamage(level, damageMultiplier) * windupDamageMultiplier);
			upgradedMiningDamage = (int) Math.Round(ExplosiveAuthoring.LevelToMiningDamage(level, miningDamageMultiplier, isEnemyOrProjectile: false) * windupDamageMultiplier);

			return true;
		}

		private static bool GetManaCostUpgrade(ObjectDataFromPrefab data, int level, out int upgradedManaCost) {
			upgradedManaCost = 0;

			var hasManaAuthoring = data.TryGetComponent<ConsumesManaAuthoring>(out var manaAuthoring);
			if (!hasManaAuthoring)
				return false;

			upgradedManaCost = manaAuthoring.ComputeManaCostFromLevel(level);

			return true;
		}
		
		private static bool GetInventorySizeUpgrade(ObjectDataFromPrefab data, int level, out int upgradedInventorySize) {
			upgradedInventorySize = 0;
			
			if (!data.TryGetComponent<ExtraInventorySizeAuthoring>(out var extraInventorySizeAuthoring) || extraInventorySizeAuthoring.sameSizeForAllLevels.hasValue)
				return false;

			upgradedInventorySize = ExtraInventorySizeAuthoring.GetSizeFromLevel(level);

			return true;
		}

		#endregion
	}
}