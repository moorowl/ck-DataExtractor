using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace DataExtractor.Extractors {
	public class Sfx : Extractor {
		public override string Name => "sfx";

		private const float DefaultMaxSpatialDistance = 16f;
		private const float DefaultMaxSpatialBlendDistance = 10f;

		public override JToken Extract() {
			var data = GetSfxTableElements().OrderBy(element => element.name).Select(element => {
				var id = Animator.StringToHash(element.name);
				var internalName = element.name;

				var dontUseSpatialSound = element.settings.dontUseSpatialSound;
				var overrideMaxSpatialDistance = element.settings.overrideMaxSpatialDistance;
				var overrideMaxSpatialBlendDistance = element.settings.overrideMaxSpatialBlendDistance;

				if (overrideMaxSpatialDistance == 0f)
					overrideMaxSpatialDistance = DefaultMaxSpatialDistance;
				if (overrideMaxSpatialBlendDistance == 0f)
					overrideMaxSpatialBlendDistance = DefaultMaxSpatialBlendDistance;

				var soundGroups = element.variants.Select(variant => variant.soundVariant).Prepend(element.sounds);

				return new {
					Id = id,
					InternalName = internalName,
					Settings = new {
						Stackable = element.settings.stackable,
						IgnoreAudioIfOutsideOfViewport = element.settings.ignoreAudioIfOutsideOfViewport,
						DontUseSpatialSound = dontUseSpatialSound,
						MaxSpatialDistance = overrideMaxSpatialDistance,
						MaxSpatialBlendDistance = overrideMaxSpatialBlendDistance
					},
					Variants = soundGroups.Select(sounds => new {
						Sounds = sounds.Select(sound => new {
							Sound = sound.sfx,
							Volume = sound.volume,
							Pitch = sound.pitch,
							PitchDev = sound.pitchDev
						})
					})
				};
			});

			return Serialize(data);
		}

		private static List<SfxTableElement> GetSfxTableElements() {
			var field = typeof(SfxTable).GetField("sfxTableElements", BindingFlags.Instance | BindingFlags.NonPublic);
			return (List<SfxTableElement>) field.GetValue(Manager.audio.sfxTable);
		}
	}
}