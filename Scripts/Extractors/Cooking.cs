using DataExtractor.DataStructures;
using DataExtractor.Utilities;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

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
				.Select(x => x.ObjectInfo.objectID);
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

		public string DisplayName {
			get {
				var rarityKey = BaseRarity switch {
					Rarity.Rare => "Rarity/Rare",
					Rarity.Epic => "Rarity/Epic",
					_ => ""
				};

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

				var name = string.Format(Utils.GetText("foodFormat"),
					Utils.GetText("FoodAdjectives/" + secondaryIngredientKey),
					Utils.GetText("FoodNouns/" + primaryIngredientKey),
					Utils.GetText("Items/" + nameKey)
				);

				if (!string.IsNullOrEmpty(rarityKey))
					name = string.Format(Utils.GetText("rareItemFormat"), Utils.GetText(rarityKey), name);

				return name;
			}
		}

		public Rarity BaseRarity { get; set; }
	}
}