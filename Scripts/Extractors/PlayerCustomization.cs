using Newtonsoft.Json.Linq;
using System.Linq;

namespace DataExtractor.Extractors {
	public class PlayerCustomization : Extractor {
		public override string Name => "playerCustomization";

		public override JToken Extract() {
			var customizationTable = ((CharacterCustomizationMenu) Manager.menu.characterCustomizationMenu).roleSelection.customizationTable;

			var data = new {
				Helm = new {
					Skins = customizationTable.helmSkins.Select((skin, index) => new {
						Id = customizationTable.GetIdFromIndex(PlayerCustomizationTable.CustomizableBodyPartType.HELM, index),
						Texture = skin.helmTexture?.name,
						EmissiveTexture = skin.emissiveHelmTexture?.name,
						HairType = skin.hairType,
						Offset = new {
							X = skin.pixelOffset.x,
							Y = skin.pixelOffset.y
						}
					})
				},
				BreastArmor = new {
					Skins = customizationTable.breastArmorSkins.Select((skin, index) => new {
						Id = customizationTable.GetIdFromIndex(PlayerCustomizationTable.CustomizableBodyPartType.BREAST_ARMOR, index),
						Texture = skin.breastTexture?.name,
						EmissiveTexture = skin.emissiveBreastTexture?.name,
						ShirtVisibility = skin.shirtVisibility
					})
				},
				PantsArmor = new {
					Skins = customizationTable.pantsArmorSkins.Select((skin, index) => new {
						Id = customizationTable.GetIdFromIndex(PlayerCustomizationTable.CustomizableBodyPartType.PANTS_ARMOR, index),
						Texture = skin.pantsTexture?.name,
						EmissiveTexture = skin.emissivePantsTexture?.name,
						PantsVisibility = skin.pantsVisibility
					})
				}
			};

			return Serialize(data);
		}
	}
}