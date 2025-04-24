using DataExtractor.Extractors;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using PugMod;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

namespace DataExtractor {
	public class Main : IMod {
		public const string Version = "1.0.0";
		public const string InternalName = "DataExtractor";

		public void EarlyInit() {
			Debug.Log($"[{InternalName}]: Mod version: {Version}");
		}

		public void Init() {
			SceneManager.activeSceneChanged += (current, next) => {
				if (!CommandLineArgs.Has("-dataex") || next.name != "Title")
					return;
				
				var outputDirectory = CommandLineArgs.GetParam("-dataex").Replace("{version}", Application.version);
				RunExtractors(outputDirectory);
				
				Application.Quit(0);
			};
		}

		public void Shutdown() { }

		public void ModObjectLoaded(UnityEngine.Object obj) { }

		public void Update() { }

		private static void RunExtractors(string outputDirectory) {
			var stopwatch = System.Diagnostics.Stopwatch.StartNew();

			Directory.CreateDirectory(outputDirectory);

			foreach (var file in Directory.GetFiles(outputDirectory, "*", SearchOption.AllDirectories))
				File.Delete(file);

			var extractors = GetExtractors().ToList();
			Debug.Log($"[{InternalName}]: Running {extractors.Count} extractors (output is at {outputDirectory})");

			foreach (var extractor in extractors) {
				Debug.Log($"[{InternalName}]: Extractor {extractor.Name} -> Running...");

				try {
					var extractedData = extractor.Extract();
					WriteJson(Path.Combine(outputDirectory, extractor.Name + ".json"), extractedData);
				}
				catch (Exception exception) {
					Debug.LogError($"[{InternalName}]: Extractor {extractor.Name} encountered an error:");
					Debug.LogException(exception);
				}
			}

			stopwatch.Stop();
			Debug.Log($"[{InternalName}]: Done in {stopwatch.Elapsed.TotalMilliseconds}ms");
		}

		private static void WriteJson(string path, JToken data) {
			using var writer = new StringWriter();
			using var jsonWriter = new JsonTextWriter(writer) {
				Formatting = Formatting.Indented,
				IndentChar = '\t',
				Indentation = 1
			};
			
			new JsonSerializer().Serialize(jsonWriter, data);
			File.WriteAllText(path, writer.ToString());
		}

		private static IEnumerable<Extractor> GetExtractors() {
			var type = typeof(Extractor);
			var types = type.Assembly.GetTypes()
				.Where(t => t.IsClass && !t.IsAbstract && type.IsAssignableFrom(t))
				.ToList();

			return types.Select(t => (Extractor) Activator.CreateInstance(t));
		}
	}
}