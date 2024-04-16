using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Verse;

namespace AppearanceClothes {
    public static class AppearanceClothesPref {
        public static List<AppearanceClothesPreset> presets = new List<AppearanceClothesPreset>();

        public static readonly string PrefFilePath = Path.Combine(GenFilePaths.ConfigFolderPath, "AppearanceClothes.xml");

        public static void LoadPref() {
            if (!File.Exists(PrefFilePath)) {
                Log.Message(PrefFilePath + " is not found.");
                return;
            }

            try {
                Scribe.loader.InitLoading(PrefFilePath);
                try {
                    ScribeMetaHeaderUtility.LoadGameDataHeader(ScribeMetaHeaderUtility.ScribeHeaderMode.None, true);
                    List<AppearanceClothesPreset> p = new List<AppearanceClothesPreset>();
                    Scribe_Collections.Look<AppearanceClothesPreset>(ref p, "presets", LookMode.Deep);
                    presets = p;
                    Scribe.loader.FinalizeLoading();
                } catch {
                    Scribe.ForceStop();
                    throw;
                }
            } catch (Exception ex) {
                Log.Error("Exception loading AppearanceClothesPref: " + ex.ToString());
                presets = new List<AppearanceClothesPreset>();
                Scribe.ForceStop();
            }
        }

        public static void SavePref() {
            try {
                SafeSaver.Save(PrefFilePath, "appearanceClothesPref", delegate {
                    ScribeMetaHeaderUtility.WriteMetaHeader();
                    List<AppearanceClothesPreset> p = presets;
                    Scribe_Collections.Look<AppearanceClothesPreset>(ref p, "presets", LookMode.Deep);
                });
            } catch (Exception ex) {
                Log.Error("Exception while saving world: " + ex.ToString());
            }
        }
    }
}
