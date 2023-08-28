using System.Collections.Generic;
using System.IO;
using RimWorld;
using UnityEngine;
using Verse;

namespace NMeijer.OptionPresets
{
	public static class OptionPresets
	{
		#region Constants

		private const string PrefExtension = ".xml";

		#endregion

		#region Properties

		private static string PresetsFolder => Path.Combine(Application.persistentDataPath, "OptionPresets");

		private static bool PresetsFolderExists => Directory.Exists(PresetsFolder);

		#endregion

		#region Public Methods

		public static string[] GetAvailablePresets()
		{
			List<string> presets = new List<string>();

			if(PresetsFolderExists)
			{
				DirectoryInfo directoryInfo = new DirectoryInfo(PresetsFolder);
				FileInfo[] fileInfos = directoryInfo.GetFiles();

				for(int i = 0, length = fileInfos.Length; i < length; i++)
				{
					FileInfo fileInfo = fileInfos[i];
					if(fileInfo.Extension == PrefExtension)
					{
						presets.Add(Path.GetFileNameWithoutExtension(fileInfo.Name));
					}
				}
			}

			return presets.ToArray();
		}

		public static void SaveCurrentActivePreset(string name)
		{
			Prefs.Save();

			if(File.Exists(GenFilePaths.PrefsFilePath))
			{
				if(!PresetsFolderExists)
				{
					Directory.CreateDirectory(PresetsFolder);
				}

				File.Copy(GenFilePaths.PrefsFilePath, GetPresetFolder(name), true);
			}
			else
			{
				Log.Error("Options could not be made into preset because the options file can't be found.");
			}

			Messages.Message("OptionPresetsSavedAsMessage".Translate(name), MessageTypeDefOf.SilentInput);
		}

		public static void LoadPreset(string name)
		{
			string filePath = GetPresetFolder(name);
			if(!File.Exists(filePath))
			{
				Log.Error($"Options preset `{name}` couldn't be loaded from `{filePath}` because the file can't be found.");
				return;
			}

			File.Copy(filePath, GenFilePaths.PrefsFilePath, true);

			Prefs.Init();
			Prefs.Apply();

			Messages.Message("OptionPresetsLoadedMessage".Translate(name), MessageTypeDefOf.SilentInput);
		}

		public static void DeletePreset(string name)
		{
			string filePath = GetPresetFolder(name);
			if(!File.Exists(filePath))
			{
				Log.Error($"Options preset `{name}` couldn't be deleted from `{filePath} because the file can't be found.");
				return;
			}

			File.Delete(filePath);

			Messages.Message("OptionPresetsDeletedMessage".Translate(name), MessageTypeDefOf.SilentInput);
		}

		public static bool PresetExists(string name)
		{
			return File.Exists(GetPresetFolder(name));
		}

		#endregion

		#region Private Methods

		private static string GetPresetFolder(string name)
		{
			return Path.Combine(PresetsFolder, name + PrefExtension);
		}

		#endregion
	}
}