using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Verse;

namespace StatueOfAnimal {
    public static class StatueOfAnimalPref {
        public static List<StatueOfAnimalPreset> presets = new List<StatueOfAnimalPreset>();

        public static readonly string PrefFilePath = Path.Combine(GenFilePaths.ConfigFolderPath, "StatueOfAnimal.xml");

        public static void LoadPref() {
            if (!File.Exists(PrefFilePath)) {
                Log.Message(PrefFilePath + " is not found.");
                return;
            }

            try {
                Scribe.loader.InitLoading(PrefFilePath);
                try {
                    ScribeMetaHeaderUtility.LoadGameDataHeader(ScribeMetaHeaderUtility.ScribeHeaderMode.None, true);
                    List<StatueOfAnimalPreset> p = new List<StatueOfAnimalPreset>();
                    Scribe_Collections.Look<StatueOfAnimalPreset>(ref p, "presets", LookMode.Deep);
                    presets = p;
                    Scribe.loader.FinalizeLoading();
                } catch {
                    Scribe.ForceStop();
                    throw;
                }
            } catch (Exception ex) {
                Log.Error("Exception loading StatueOfAnimalPref: " + ex.ToString());
                presets = new List<StatueOfAnimalPreset>();
                Scribe.ForceStop();
            }
        }

        public static void SavePref() {
            try {
                SafeSaver.Save(PrefFilePath, "StatueOfAnimalPref", delegate {
                    ScribeMetaHeaderUtility.WriteMetaHeader();
                    List<StatueOfAnimalPreset> p = presets;
                    Scribe_Collections.Look<StatueOfAnimalPreset>(ref p, "presets", LookMode.Deep);
                });
            } catch (Exception ex) {
                Log.Error("Exception while saving world: " + ex.ToString());
            }
        }
    }
}
