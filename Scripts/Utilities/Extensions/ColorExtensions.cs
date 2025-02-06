using UnityEngine;

namespace DataExtractor.Utilities.Extensions {
	public static class ColorExtensions {
		public static string ToHex(this Color color) {
			var r = (int) (color.r * 255f);
			var g = (int) (color.g * 255f);
			var b = (int) (color.b * 255f);
			var a = (int) (color.a * 255f);

			var hexColor = "#" + r.ToString("X2") + g.ToString("X2") + b.ToString("X2");
			if (a < 255)
				hexColor += a.ToString("X2");

			return hexColor.ToLower();
		}

		public static string ToHex(this Color32 color) {
			var hexColor = "#" + color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
			if (color.a < 255)
				hexColor += color.a.ToString("X2");

			return hexColor.ToLower();
		}
	}
}