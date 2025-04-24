using DataExtractor.Utilities;
using Newtonsoft.Json.Linq;
using System.Linq;
using UnityEngine;

namespace DataExtractor.Extractors {
	public class ObjectCategories : Extractor {
		public override string Name => "objectCategories";

		public override JToken Extract() {
			var categories = Resources.LoadAll<ObjectIDCategory>("ObjectIDCategories");

			var data = categories.Select(info => {
				var isParentCategory = info.ParentCategory == null;

				return new {
					Category = GetCategoryName(info),
					ParentCategory = GetParentCategoryName(info),
					DisplayName = Utils.GetText("ItemCategory/" + info.name),
					Icon = info.icon?.name,
					Objects = isParentCategory ? null : info.ObjectIds // Parents have all the objects of their children
				};
			});

			return Serialize(data);
		}

		private static string GetCategoryName(ObjectIDCategory category) {
			return category.name.Replace((category.ParentCategory?.name ?? "") + "_", "");
		}

		private static string GetParentCategoryName(ObjectIDCategory category) {
			return category.ParentCategory?.name;
		}
	}
}