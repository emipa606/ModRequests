using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Verse;

namespace StatueOfColonist {
    public static class StatueOfColonistPref {
        public static List<StatueOfColonistPreset> presets = new List<StatueOfColonistPreset>();

        public static readonly string PrefFilePath = Path.Combine(GenFilePaths.ConfigFolderPath, "StatueOfColonist.xml");

        public static void LoadPref() {
            if (!File.Exists(PrefFilePath)) {
                Log.Message(PrefFilePath + " is not found.");
                return;
            }

            try {
                Scribe.loader.InitLoading(PrefFilePath);
                try {
                    ScribeMetaHeaderUtility.LoadGameDataHeader(ScribeMetaHeaderUtility.ScribeHeaderMode.None, true);
                    List<StatueOfColonistPreset> p = new List<StatueOfColonistPreset>();
                    Scribe_Collections.Look<StatueOfColonistPreset>(ref p, "presets", LookMode.Deep);
                    presets = p;
                    Scribe.loader.FinalizeLoading();
                } catch {
                    Scribe.ForceStop();
                    throw;
                }
            } catch (Exception ex) {
                Log.Error("Exception loading StatueOfColonistPref: " + ex.ToString());
                presets = new List<StatueOfColonistPreset>();
                Scribe.ForceStop();
            }
        }

        public static void SavePref() {
            try {
                SafeSaver.Save(PrefFilePath, "statueOfColonistPref", delegate {
                    ScribeMetaHeaderUtility.WriteMetaHeader();
                    List<StatueOfColonistPreset> p = presets;
                    Scribe_Collections.Look<StatueOfColonistPreset>(ref p, "presets", LookMode.Deep);
                });
            } catch (Exception ex) {
                Log.Error("Exception while saving world: " + ex.ToString());
            }
        }
    }
}
