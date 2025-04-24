using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Pug.Sprite;
using UnityEngine;

namespace DataExtractor.Extractors {
	public class SpriteAssets : Extractor {
		public override string Name => "spriteAssets";

		private static readonly Lazy<SpriteAssetManifest> Manifest = new(SpriteAssetManifest.GetManifest);

		private static readonly Lazy<Dictionary<Guid, string>> GuidToAnimationName = new(() => {
			var guidToAnimationName = new Dictionary<Guid, string>();

			foreach (var spriteAssetBase in Manifest.Value.spriteAssets) {
				if (spriteAssetBase is not SpriteAsset spriteAsset)
					continue;

				var assetName = spriteAsset.name;
				for (var i = 0; i < spriteAsset.animationCount; i++) {
					var animation = spriteAsset.GetAnimationAt(i);
					var name = SpriteAsset.PrettifyName(animation.spriteData.GetAssetName(), assetName);

					guidToAnimationName[animation.guid] = name;
				}
			}

			return guidToAnimationName;
		});

		private static readonly Lazy<Dictionary<int, string>> HashToAnimationName = new(() => {
			var hashToAnimationName = new Dictionary<int, string>();

			foreach (var spriteAssetBase in Manifest.Value.spriteAssets) {
				if (spriteAssetBase is not SpriteAsset spriteAsset)
					continue;

				var assetName = spriteAsset.name;
				for (int i = 0; i < spriteAsset.animationCount; i++) {
					var animation = spriteAsset.GetAnimationAt(i);
					var name = SpriteAsset.PrettifyName(animation.spriteData.GetAssetName(), assetName);

					hashToAnimationName[Animator.StringToHash(name)] = name;
				}
			}

			return hashToAnimationName;
		});

		public override JToken Extract() {
			var manifest = SpriteAssetManifest.GetManifest();

			var data = manifest.spriteAssets.Where(spriteAssetBase => spriteAssetBase is SpriteAsset).Cast<SpriteAsset>().Select(spriteAsset => {
				var events = Enumerable.Range(0, spriteAsset.eventCount).Select(spriteAsset.GetEventAt).ToList();
				var sprites = Enumerable.Range(0, spriteAsset.staticVariantCount)
					.Select(spriteAsset.GetStaticVariant)
					.Prepend(spriteAsset.staticSpriteData)
					.Where(spriteData => spriteData.hasAnyTexture);

				return new {
					Name = spriteAsset.name,
					DefaultGradientMap = spriteAsset.defaultGradientMap?.name,
					Sprites = sprites.Select(spriteData => new {
						Texture = spriteData.texture?.name,
						TextureEmissive = spriteData.emissiveTexture?.name,
						TextureNormal = spriteData.normalTexture?.name,
						Pivot = spriteData.pivot
					}),
					Animations = Enumerable.Range(0, spriteAsset.animationCount).Select(animationIndex => {
						var animation = spriteAsset.GetAnimationAt(animationIndex);
						var name = GetAnimationName(animation);

						return new {
							Name = name,
							FramesPerSecond = animation.fps,
							Loop = animation.loop,
							Frames = animation.frameData.Select(frameData => {
								var eventMask = new BitArray(new[] { frameData.eventMask });

								return new {
									Duration = frameData.holdFrames + 1,
									Events = new List<string>()
									//Events = Enumerable.Range(0, eventMask.Count).Where(bitIndex => eventMask[bitIndex]).Select(bitIndex => events[bitIndex])
								};
							}),
							Sprites = Enumerable.Range(0, animation.variantCount).Select(variantIndex => animation.GetVariant(variantIndex)).Prepend(animation.spriteData).Select(spriteData => new {
								Texture = spriteData.texture?.name,
								TextureEmissive = spriteData.emissiveTexture?.name,
								TextureNormal = spriteData.normalTexture?.name,
								Pivot = spriteData.pivot
							}),
							Transitions = Enumerable.Range(0, animation.transitionCount).Select(transitionIndex => {
								var transition = animation.GetTransition(transitionIndex);
								transition.GetHashes(spriteAsset, out var toHash, out var fromHash);

								return new {
									From = GetAnimationName(fromHash),
									To = GetAnimationName(toHash)
								};
							}),
							ExitAnimation = GetAnimationName(animation.runtimeExitAnimationHash)
						};
					}),
					Events = events,
					Skins = manifest.spriteAssets.Where(spriteAssetBase => spriteAssetBase is SpriteAssetSkin spriteAssetSkin && spriteAssetSkin.targetAsset == spriteAsset).Cast<SpriteAssetSkin>().Select(spriteAssetSkin => {
						return new {
							Name = spriteAssetSkin.name,
							TargetAssetName = spriteAssetSkin.targetAsset?.name,
							DefaultGradientMap = spriteAssetSkin.defaultGradientMap?.name,
							Replacements = Enumerable.Range(0, spriteAssetSkin.replacementDataCount).Select(spriteAssetSkin.GetReplacementAt).Select(replacementData => {
								var sprites = Enumerable.Range(0, replacementData.variantCount)
									.Select(variantIndex => replacementData.GetVariant(variantIndex))
									.Prepend(replacementData.spriteData)
									.Where(spriteData => spriteData.hasAnyTexture);

								return new {
									Animation = GetAnimationName(replacementData.guid),
									Sprites = sprites.Select(spriteData => new {
										Texture = spriteData.texture?.name,
										TextureEmissive = spriteData.emissiveTexture?.name,
										TextureNormal = spriteData.normalTexture?.name,
										Pivot = spriteData.pivot
									})
								};
							}).ToDictionary(x => x.Animation, x => x.Sprites)
						};
					})
				};
			});

			return Serialize(data);
		}

		private static string GetAnimationName(Guid guid) {
			return GuidToAnimationName.Value.GetValueOrDefault(guid);
		}

		private static string GetAnimationName(string guid) {
			if (guid == null)
				return null;

			return GetAnimationName(Guid.Parse(guid));
		}

		private static string GetAnimationName(FrameAnimation animation) {
			return GetAnimationName(animation.guid);
		}

		private static string GetAnimationName(int hash) {
			return HashToAnimationName.Value.GetValueOrDefault(hash);
		}
	}
}