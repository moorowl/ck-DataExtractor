﻿using Newtonsoft.Json.Linq;
using System.Linq;
using DataExtractor.Utilities;
using UnityEngine;

namespace DataExtractor.Extractors {
	public class ContentBundles : Extractor {
		public override string Name => "contentBundles";

		public override JToken Extract() {
			var contentBundlesTable = Resources.Load<ContentBundleTable>("ContentBundleTable");

			var data = contentBundlesTable.contentBundles.OrderBy(contentBundle => (int) contentBundle.id)
				.Select(contentBundle => new {
					Id = (int) contentBundle.id,
					InternalName = contentBundle.id,
					DisplayName = Utils.GetText($"Menu/ContentBundle{contentBundle.id}Title"),
					Description = Utils.GetText($"Menu/ContentBundle{contentBundle.id}Desc"),
					CanBeActivatedByPlayer = contentBundle.canBeActivatedByPlayer,
					Dependencies = contentBundle.dependencies
				});

			return Serialize(data);
		}
	}
}