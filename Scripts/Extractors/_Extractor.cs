using DataExtractor.DataStructures;
using DataExtractor.DataStructures.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Unity.Physics.Authoring;
using UnityEngine;

namespace DataExtractor.Extractors {
	public abstract class Extractor {
		public abstract string Name { get; }

		public abstract JToken Extract();

		protected static JsonSerializerSettings DefaultSettings = new() {
			NullValueHandling = NullValueHandling.Ignore,
			ContractResolver = new DefaultContractResolver {
				NamingStrategy = new CamelCaseNamingStrategy(false, true, true)
			},
			Converters = new JsonConverter[] {
				new ColorHexConverter(),
				new DirectionConverter(),
				new MathConverter(),
				new GameObjectToNameConverter(),
				new UnityObjectToNameConverter<Sprite>(),
				new SfxTableIdConverter(),
				new PhysicsCategoryTagsConverter(),
				new EnumFlagsConverter()
			}
		};

		protected static JsonSerializerSettings DefaultSettingsForObjects = new() {
			NullValueHandling = NullValueHandling.Ignore,
			ContractResolver = new IgnorePropertiesResolver {
				NamingStrategy = new CamelCaseNamingStrategy(false, true, true)
			},
			Converters = new JsonConverter[] {
				new ColorHexConverter(),
				new DirectionConverter(),
				new MathConverter(),
				new GameObjectToNameConverter(),
				new UnityObjectToNameConverter<Sprite>(),
				new SfxTableIdConverter(),
				new PhysicsCategoryTagsConverter(),
				new EnumFlagsConverter(),
				new IncludeCraftedObjectsConverter()
			}
		};

		static Extractor() {
			var resolver = DefaultSettingsForObjects.ContractResolver as IgnorePropertiesResolver;

			resolver.Ignore<PhysicsShapeAuthoring>();
			resolver.Ignore<PhysicsBodyAuthoring>();
			resolver.Ignore<EquippedObjectAuthoring>();
			resolver.Ignore<Material>();

			resolver.IgnoreProperty("gameObject", "useGUILayout", "enabled", "isActiveAndEnabled", "transform", "tag", "name", "hideFlags", "destroyCancellationToken", "runInEditMode");
			resolver.IgnoreProperty<CraftingAuthoring.CraftableObject>("craftingConsumesEntityAmount");
			resolver.IgnoreProperty<AreaLevelAuthoring>("entityMono");
			resolver.IgnoreProperty<GivesConditionsWhenEquippedAuthoring>("level", "conditionsTable");
			resolver.IgnoreProperty<GivesConditionsWhenConsumedAuthoring>("level", "conditionsTable");
			resolver.IgnoreProperty<DamageReductionAuthoring>("level");
			resolver.IgnoreProperty<HealthAuthoring>("level");
			resolver.IgnoreProperty<ExplodeStateAuthoring>("level");
			resolver.IgnoreProperty<MeleeAttackStateAuthoring>("level");
			resolver.IgnoreProperty<RangeAttackStateAuthoring>("level");
			resolver.IgnoreProperty<JumpAttackStateAuthoring>("level");
			resolver.IgnoreProperty<DamageObjectStateAuthoring>("level");
			resolver.IgnoreProperty<ExplodeOnImpactAuthoring>("level");
			resolver.IgnoreProperty<ShootMortarProjectileStateAuthoring>("level");
			resolver.IgnoreProperty<ChargeAttackStateAuthoring>("level");
			resolver.IgnoreProperty<SlimeBossJumpStateAuthoring>("level");
			resolver.IgnoreProperty<ExplosiveAuthoring>("level");
			resolver.IgnoreProperty<AttackContinuouslyAuthoring>("level");
			resolver.IgnoreProperty<GiantCicadaBossAuthoring>("level");
			resolver.IgnoreProperty<OffHandAuthoring>("level", "entityMono", "cooldown");
			resolver.IgnoreProperty<ScarabBossAuthoring>("level");
			resolver.IgnoreProperty<SupportsConditionsAuthoring>("level", "conditionsTable");
			resolver.IgnoreProperty<BossLarvaAuthoring>("level");
			resolver.IgnoreProperty<PlaceableObjectAuthoring>("prefabTileSize", "prefabCornerOffset", "centerIsAtEntityPosition", "appearInMapUI", "mapColor");
			resolver.IgnoreProperty<PetWalkStateAuthoring>("belongsToShape");
			resolver.IgnoreProperty<ChaseStateAuthoring>("belongsToShape");
			resolver.IgnoreProperty<WeaponDamageAuthoring>("level", "entityMono", "cooldown");
			resolver.IgnoreProperty<ObjectInfo>("objectID", "variation", "variationIsDynamic", "level", "onlyExistsInSeason", "iconSkinAsset", "additionalSprites", "tileset", "tileType", "prefabInfos", "isCustomScenePrefab", "languageGenders");
			resolver.IgnoreProperty<EntityMonoBehaviourData>("objectInfo", "optionalPreviewPrefab");
			resolver.IgnoreProperty<ConsumesManaAuthoring>("levelAuthoring", "cooldownAuth", "weaponDamage", "secondaryUse");
			resolver.IgnoreProperty<DurabilityAuthoring>("entityMono");
		}

		protected JToken Serialize(object value, JsonSerializerSettings settings = null) {
			var serializer = JsonSerializer.Create(settings ?? DefaultSettings);
			return JToken.FromObject(value, serializer);
		}
	}
}