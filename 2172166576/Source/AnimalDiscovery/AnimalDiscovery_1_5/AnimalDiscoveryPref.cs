using System;
using System.Collections.Generic;
using System.IO;
using Verse;

namespace AnimalDiscovery
{
    public static class AnimalDiscoveryPref
    {
		public static void LoadPref()
		{
			if (!File.Exists(AnimalDiscoveryPref.PrefFilePath))
			{
				Log.Message(AnimalDiscoveryPref.PrefFilePath + " is not found.");
				return;
			}
			try
			{
				Scribe.loader.InitLoading(AnimalDiscoveryPref.PrefFilePath);
				try
				{
					ScribeMetaHeaderUtility.LoadGameDataHeader(ScribeMetaHeaderUtility.ScribeHeaderMode.None, true);
					List<AnimalDiscoveryPreset> list = new List<AnimalDiscoveryPreset>();
					Scribe_Collections.Look<AnimalDiscoveryPreset>(ref list, "presets", LookMode.Deep, Array.Empty<object>());
					AnimalDiscoveryPref.presets = list;
					Scribe.loader.FinalizeLoading();
				}
				catch
				{
					Scribe.ForceStop();
					throw;
				}
			}
			catch (Exception ex)
			{
				Log.Error("Exception loading AnimalDiscoveryPref: " + ex.ToString());
				AnimalDiscoveryPref.presets = new List<AnimalDiscoveryPreset>();
				Scribe.ForceStop();
			}
		}

		public static void SavePref()
		{
			try
			{
				SafeSaver.Save(AnimalDiscoveryPref.PrefFilePath, "AnimalDiscoveryPref", delegate
				{
					ScribeMetaHeaderUtility.WriteMetaHeader();
					List<AnimalDiscoveryPreset> list = AnimalDiscoveryPref.presets;
					Scribe_Collections.Look<AnimalDiscoveryPreset>(ref list, "presets", LookMode.Deep, Array.Empty<object>());
				}, false);
			}
			catch (Exception ex)
			{
				Log.Error("Exception while saving world: " + ex.ToString());
			}
		}

		public static List<AnimalDiscoveryPreset> presets = new List<AnimalDiscoveryPreset>();

        public static readonly string PrefFilePath = Path.Combine(GenFilePaths.ConfigFolderPath, "AnimalDiscovery.xml");
    }
}
