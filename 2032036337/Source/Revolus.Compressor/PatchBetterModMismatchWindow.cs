using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using Verse;

namespace Revolus.Compressor {
    internal class PatchBetterModMismatchWindow {
        private static List<(string name, string version)> cachedModList = null;

        private static Assembly BetterModMismatchWindow;

        internal static void DoPatch(Harmony harmony) {
            BetterModMismatchWindow = (
                from assembly in AppDomain.CurrentDomain.GetAssemblies()
                where assembly.GetName().Name == "ModMisMatchWindowPatch"
                select assembly
            ).FirstOrDefault();
            if (BetterModMismatchWindow is null) {
                return;
            }
            
            Log.Message(string.Format("Mod 'Better ModMismatch Window' was found -> patching", BetterModMismatchWindow));

            if (!Utils.GetType(BetterModMismatchWindow, "Madeline.ModMismatchFormatter.ModContentPackExtension", out var ModContentPackExtension)) {
                Log.Error("!Madeline.ModMismatchFormatter.ModContentPackExtension");
            } else if (!Utils.GetMethod(ModContentPackExtension, "ReadModsFromSaveHeader", true, out var ReadModsFromSaveHeader)) {
                Log.Error("!Madeline.ModMismatchFormatter.MetaHeaderUtility ReadModsFromSaveHeader");
            } else if (!Utils.GetType(BetterModMismatchWindow, "Madeline.ModMismatchFormatter.MetaHeaderUtility", out var MetaHeaderUtility)) {
                Log.Error("!Madeline.ModMismatchFormatter.MetaHeaderUtility");
            } else if (!Utils.GetMethod(MetaHeaderUtility, "UpdateModVersionMetaHeader", true, out var UpdateModVersionMetaHeader)) {
                Log.Error("!Madeline.ModMismatchFormatter.MetaHeaderUtility UpdateModVersionMetaHeader");
            } else {
                harmony.Patch(
                    ReadModsFromSaveHeader,
                    prefix: new HarmonyMethod(typeof(PatchBetterModMismatchWindow), nameof(ReadModsFromSaveHeader__Prefix))
                );
                harmony.Patch(
                    UpdateModVersionMetaHeader,
                    prefix: new HarmonyMethod(typeof(PatchBetterModMismatchWindow), nameof(UpdateModVersionMetaHeader__Prefix))
                );
            }
        }

        private static List<(string name, string version)> GetModsList() {
            var result = cachedModList;
            if (result == null) {
                result = new List<(string name, string version)>();
                cachedModList = result;

                try {
                    Utils.GetType(BetterModMismatchWindow, "Madeline.ModMismatchFormatter.ModContentPackExtension", out var ModContentPackExtension);
                    Utils.GetMethod(ModContentPackExtension, "GetMetaData", true, out var GetMetaData);

                    Utils.GetType(BetterModMismatchWindow, "Madeline.ModMismatchFormatter.MetaHeaderUtility", out var MetaHeaderUtility);
                    Utils.GetMethod(MetaHeaderUtility, "GetVersionFromManifestFile", true, out var GetVersionFromManifestFile);

                    foreach (ModContentPack modContentPack in LoadedModManager.RunningMods) {
                        var metaData = (ModMetaData) GetMetaData.Invoke(null, new object[] { modContentPack });
                        var version = (string) GetVersionFromManifestFile.Invoke(null, new object[] { modContentPack });
                        result.Add((metaData.Name, version ?? "Unknown"));
                    }
                } catch (Exception ex) {
                    Log.Warning("Cannot get modlist: " + ex);
                }
            }
            return result;
        }

        internal static void WriteMetaHeader() {
            if (BetterModMismatchWindow is null) {
                return;
            }

            var mods = GetModsList();
            if (mods.Count > 0 && Scribe.EnterNode("modMetaDatas")) {
                try {
                    foreach (var data in mods) {
                        if (Scribe.EnterNode("li")) {
                            try {
                                (var name, var version) = data;
                                Scribe_Values.Look(ref name, "ModName");
                                Scribe_Values.Look(ref version, "version");
                            } finally {
                                Scribe.ExitNode();
                            }
                        }
                    }
                } finally {
                    Scribe.ExitNode();
                }
            }
        }

        private static Dictionary<string, string> ReadModMetaDatas(string filePath) {
            return Utils.WithUncompressedXmlTextReader(filePath, (XmlTextReader xml) => {
                var result = new Dictionary<string, string>();
                var modMetaDatas = XDocument.Load(xml).Element("savegame")?.Element("meta")?.Element("modMetaDatas");
                if (!(modMetaDatas is null)) {
                    foreach (var modMetaData in modMetaDatas.Elements("li")) {
                        var name = modMetaData.Element("ModName")?.Value;
                        if (!(name is null)) {
                            result.Add(name, modMetaData.Element("version")?.Value);
                        }
                    }
                }
                return result;
            });
        }

        public static bool ReadModsFromSaveHeader__Prefix(ref object __result, bool readModVersion) {
            if (
                !Utils.GetType(BetterModMismatchWindow, "Madeline.ModMismatchFormatter.MetaHeaderUtility", out var MetaHeaderUtility) ||
                !Utils.GetProp(MetaHeaderUtility, "LastAccessedSaveFilePathInLoadSelection", true, out var LastAccessedSaveFilePathInLoadSelection)
            ) {
                Log.Error("'MetaHeaderUtility' not found or incompatible");
                return true;
            }
            
            if (
                !Utils.GetType(BetterModMismatchWindow, "Madeline.ModMismatchFormatter.Mod", out var Mod) || 
                !Utils.GetProp(Mod, "Version", false, out var ModVersion)
            ) {
                Log.Error("'Mod' not found or incompatible");
                return true;
            }

            Dictionary<string, string> modsInSelection = null;
            if (readModVersion) {
                modsInSelection = ReadModMetaDatas((string) LastAccessedSaveFilePathInLoadSelection.GetValue(null));
            }

            var saveMods = new List<object>();
            var loadedModNamesList = ScribeMetaHeaderUtility.loadedModNamesList;
            var loadedModIdsList = ScribeMetaHeaderUtility.loadedModIdsList;
            for (int index = 0, count = loadedModNamesList.Count; index < count; ++index) {
                var id = loadedModIdsList[index];
                var name = loadedModNamesList[index];
                var mod = Activator.CreateInstance(Mod, new object[] { id, name, index });
                if (readModVersion) {
                    ModVersion.SetValue(mod, modsInSelection.TryGetValue(name, null) ?? "Unknown");
                }
                saveMods.Add(mod);
            }
            __result = saveMods;

            return false;
        }

        public static bool UpdateModVersionMetaHeader__Prefix() {
            return false;
        }
    }
}
