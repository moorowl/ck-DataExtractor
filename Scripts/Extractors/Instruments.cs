using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DataExtractor.Extractors {
	public class Instruments : Extractor {
		public override string Name => "instruments";

		public override JToken Extract() {
			var keyInputsMap = GetKeyInputsMap();
			var playingNotesList = Manager.ui.instrumentUI.playingNotesList;

			var data = new {
				Notes = keyInputsMap.Select(entry => new {
					Id = entry.Key,
					InternalName = entry.Value
				}),
				Melodies = MelodyData.melodies.Select(melody => new {
					Id = (int) melody.id,
					InternalName = melody.id,
					Notes = melody.melody.Select(id => keyInputsMap[id])
				})
			};

			return Serialize(data);
		}

		private static Dictionary<int, PlayerInput.InputType> GetKeyInputsMap() {
			var field = typeof(PlayInstrumentHandler).GetField("KEY_INPUTS", BindingFlags.Static | BindingFlags.NonPublic);
			var array = (PlayerInput.InputType[]) field.GetValue(null);

			return Enumerable.Range(0, array.Length).ToDictionary(x => x, x => array[x]);
		}
	}
}