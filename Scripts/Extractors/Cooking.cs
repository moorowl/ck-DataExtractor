using DataExtractor.DataStructures;
using DataExtractor.Utilities;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using I2.Loc;

namespace DataExtractor.Extractors {
	public class Cooking : Extractor {
		public override string Name => "cooking";

		public override JToken Extract() {
			var objects = Manager.ecs.pugDatabase.prefabList
				.Select(prefab => new ObjectDataFromPrefab(prefab.gameObject))
				.GroupBy(x => x.ObjectInfo.objectID)
				.Select(x => x.First())
				.ToDictionary(x => x.ObjectInfo.objectID, x => x);

			var allIngredientTypes = objects.Values.Where(x => x.HasComponent<CookingIngredientAuthoring>())
				.Select(x => x.ObjectInfo.objectID)
				.ToList();
			var allUniqueMeals = new Dictionary<int, Meal>();

			foreach (var ingredientA in allIngredientTypes) {
				foreach (var ingredientB in allIngredientTypes) {
					var variation = CookedFoodCD.GetFoodVariation(ingredientA, ingredientB);
					var primaryIngredient = CookedFoodCD.GetPrimaryIngredientFromVariation(variation);
					var secondaryIngredient = CookedFoodCD.GetSecondaryIngredientFromVariation(variation);
					var foodItem = objects[primaryIngredient].GetComponent<CookingIngredientAuthoring>().turnsIntoFood;

					allUniqueMeals[variation] = new Meal {
						PrimaryIngredient = primaryIngredient,
						SecondaryIngredient = secondaryIngredient,
						FoodItem = foodItem,
						Variation = variation,
						BaseRarity = objects[foodItem].ObjectInfo.rarity,
					};
				}
			}

			return Serialize(allUniqueMeals.Values.OrderBy(meal => meal.Variation));
		}
	}

	class Meal {
		public ObjectID PrimaryIngredient { get; set; }
		public ObjectID SecondaryIngredient { get; set; }
		public ObjectID FoodItem { get; set; }
		public int Variation { get; set; }

		public Dictionary<string, string> DisplayName {
			get {
				var names = LocalizationManager.GetAllLanguages()
					.Select(language => (LocalizationManager.GetLanguageCode(language), GetDisplayName(language)))
					.Where(tuple => tuple.Item2 != null)
					.ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);
			
				return names.Any() ? names : null;
			}
		}

		public Rarity BaseRarity { get; set; }

		private string GetDisplayName(string language) {
			var rarityKey = BaseRarity switch {
				Rarity.Rare => "Rarity/Rare",
				Rarity.Epic => "Rarity/Epic",
				_ => ""
			};
			
			var gender = PugDatabase.GetObjectInfo(FoodItem).GetLanguageGender(language);
			if (gender == Gender.Female)
				rarityKey += "Female";
			if (gender == Gender.Male)
				rarityKey += "Male";

			var nameKey = Utils.GetObjectDisplayTerm(FoodItem, 0);
			var primaryIngredientKey =
				PlayerController.GetAnyObjectIDReplaceForNameAndDesc(PrimaryIngredient).ToString();
			var secondaryIngredientKey =
				PlayerController.GetAnyObjectIDReplaceForNameAndDesc(SecondaryIngredient).ToString();

			if (primaryIngredientKey.EndsWith("Rare"))
				primaryIngredientKey = primaryIngredientKey.Remove(primaryIngredientKey.Length - 4, 4);
			if (secondaryIngredientKey.EndsWith("Rare"))
				secondaryIngredientKey = secondaryIngredientKey.Remove(secondaryIngredientKey.Length - 4, 4);
			if (nameKey.EndsWith("Rare") || nameKey.EndsWith("Epic"))
				nameKey = nameKey.Remove(nameKey.Length - 4, 4);
			
			var name = string.Format(Utils.GetTranslation("foodFormat", language),
				Utils.GetTranslation("FoodAdjectives/" + secondaryIngredientKey, language),
				Utils.GetTranslation("FoodNouns/" + primaryIngredientKey, language),
				Utils.GetTranslation("Items/" + nameKey, language)
			);

			if (!string.IsNullOrEmpty(rarityKey))
				name = string.Format(Utils.GetTranslation("rareItemFormat", language), Utils.GetTranslation(rarityKey, language), name);

			return name;
		}
	}
}