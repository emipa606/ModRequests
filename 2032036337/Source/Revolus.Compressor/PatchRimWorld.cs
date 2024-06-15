using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Verse;

namespace Revolus.Compressor {
    internal class PatchRimWorld {
        internal static void DoPatch(Harmony harmony) {
            void Patch(Type type, string name, string prefix, string postfix, params Type[] args) {
                Log.Message($"Patch: {type.Name} {name}");
                harmony.Patch(
                    AccessTools.Method(type, name, args),
                    prefix: prefix is null ? null : new HarmonyMethod(typeof(PatchRimWorld), prefix),
                    postfix: postfix is null ? null : new HarmonyMethod(typeof(PatchRimWorld), postfix)
                );
            }
            
            // ScribeLoader
            Patch(
                typeof(ScribeLoader), "InitLoading",
                nameof(ScribeLoader__InitLoading__Prefix), null,
                typeof(string)
            );
            Patch(
                typeof(ScribeLoader), "InitLoadingMetaHeaderOnly",
                nameof(ScribeLoader__InitLoadingMetaHeaderOnly__Prefix), null,
                typeof(string)
            );

            // ScribeMetaHeaderUtility
            Patch(
                typeof(ScribeMetaHeaderUtility), "GameVersionOf",
                nameof(ScribeMetaHeaderUtility__GameVersionOf__Prefix), null,
                typeof(FileInfo)
            );
            Patch(
                typeof(ScribeMetaHeaderUtility), "WriteMetaHeader",
                nameof(ScribeMetaHeaderUtility__WriteMetaHeader__Prefix), null
            );

            // ScribeSaver
            Patch(
                typeof(SafeSaver), "DoSave",
                nameof(SafeSaver__DoSave__Prefix), nameof(SafeSaver__DoSave__Postfix),
                typeof(string), typeof(string), typeof(Action)
            );
            Patch(
                typeof(ScribeSaver), "InitSaving",
                nameof(ScribeSaver__InitSaving__Prefix), null,
                typeof(string), typeof(string)
            );

            // DataExposeUtility
            Patch(
                typeof(DataExposeUtility), "AddLineBreaksToLongString",
                nameof(DataExposeUtility__AddLineBreaksToLongString__Prefix), null,
                typeof(string)
            );
            Patch(
                typeof(DataExposeUtility), "ByteArray",
                nameof(DataExposeUtility__ByteArray__Prefix), null,
                (Type[]) null
            );
        }

        internal static bool ScribeLoader__InitLoading__Prefix(ref ScribeLoader __instance, string filePath) {
            ScribeLoader instance = __instance;

            if (Scribe.mode != 0) {
                Log.Error("Called InitLoading() but current mode is " + Scribe.mode);
                Scribe.ForceStop();
            }

            if (instance.curParent != null) {
                Log.Error("Current parent is not null in InitLoading");
                instance.curParent = null;
            }

            if (instance.curPathRelToParent != null) {
                Log.Error("Current path relative to parent is not null in InitLoading");
                instance.curPathRelToParent = null;
            }

            try {
                instance.curXmlParent = Utils.WithUncompressedXmlTextReader(filePath, (XmlTextReader reader) => {
                    var xmlDocument = new XmlDocument();
                    xmlDocument.Load(reader);
                    return xmlDocument.DocumentElement;
                });
                Scribe.mode = LoadSaveMode.LoadingVars;
            } catch (Exception ex) {
                Log.Error("Exception while init loading file: " + filePath + "\n" + ex);
                instance.ForceStop();
                throw;
            }

            return false;
        }

        internal static bool ScribeLoader__InitLoadingMetaHeaderOnly__Prefix(ref ScribeLoader __instance, string filePath) {
            ScribeLoader instance = __instance;

            if (Scribe.mode != LoadSaveMode.Inactive) {
                Log.Error("Called InitLoadingMetaHeaderOnly() but current mode is " + Scribe.mode);
                Scribe.ForceStop();
            }

            try {
                Utils.WithUncompressedXmlTextReader<object>(filePath, (XmlTextReader xml) => {
                    if (ScribeMetaHeaderUtility.ReadToMetaElement(xml)) {
                        using (XmlReader reader = xml.ReadSubtree()) {
                            var xmlDocument = new XmlDocument();
                            xmlDocument.Load(reader);

                            XmlElement xmlElement = xmlDocument.CreateElement("root");
                            xmlElement.AppendChild(xmlDocument.DocumentElement);

                            instance.curXmlParent = xmlElement;
                        }
                    }
                    return null;
                });
                Scribe.mode = LoadSaveMode.LoadingVars;
            } catch (Exception ex) {
                Log.Error("Exception while init loading meta header: " + filePath + "\n" + ex);
                __instance.ForceStop();
                throw;
            }

            return false;
        }

        internal static bool SafeSaver__DoSave__Prefix(string fullPath, string documentElementName, Action saveAction) {
            CompressorMod.CurrentlySavingSavegame = true;
            return true;  // proceed to original implementation
        }

        internal static void SafeSaver__DoSave__Postfix() {
            CompressorMod.CurrentlySavingSavegame = false;
        }

        internal static bool ScribeMetaHeaderUtility__GameVersionOf__Prefix(ref string __result, FileInfo file) {
            string result = null;
            try {
                result = Utils.WithUncompressedXmlTextReader(file.FullName, (XmlTextReader reader) => {
                    return ScribeMetaHeaderUtility.ReadToMetaElement(reader) && reader.ReadToDescendant("gameVersion")
                        ? VersionControl.VersionStringWithoutRev(reader.ReadString())
                        : null;
                });
            } catch (Exception ex) {
                Log.Error("Exception getting game version of " + file.Name + ": " + ex.ToString());
            }
            __result = result;
            return false;  // don't call original implementation
        }

        internal static bool ScribeMetaHeaderUtility__WriteMetaHeader__Prefix() {
            if (Scribe.EnterNode("meta")) {
                try {
                    var gameVersion = VersionControl.CurrentVersionStringWithRev;
                    Scribe_Values.Look(ref gameVersion, "gameVersion");

                    var modIds = LoadedModManager.RunningMods.Select((ModContentPack mod) => mod.PackageId).ToList();
                    Scribe_Collections.Look(ref modIds, "modIds", LookMode.Undefined);

                    var modNames = LoadedModManager.RunningMods.Select((ModContentPack mod) => mod.Name).ToList();
                    Scribe_Collections.Look(ref modNames, "modNames", LookMode.Undefined);

                    PatchBetterModMismatchWindow.WriteMetaHeader();
                } finally {
                    Scribe.ExitNode();
                }
            }
            return false;
        }

        public static bool ScribeSaver__InitSaving__Prefix(ref ScribeSaver __instance, string filePath, string documentElementName) {
            if (!CompressorMod.CurrentlySavingSavegame) {
                return true;  // use original implementation for everything but save games
            }

            bool GotField(string name, out FieldInfo result) {
                FieldInfo field = typeof(ScribeSaver).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
                result = field;
                return !(field is null);
            }

            ScribeSaver instance = __instance;

            if (!GotField("writer", out var writerField) || !GotField("saveStream", out var saveStreamField)) {
                Log.Error("Verse.ScribeSaver is incompatible. Not Compressing.");
                return true;
            }

            if (Scribe.mode != LoadSaveMode.Inactive) {
                Log.Error("Called InitSaving() but current mode is " + Scribe.mode);
                Scribe.ForceStop();
            }

            if (GotField("curPath", out var curPathField) && curPathField.GetValue(instance) != null) {
                Log.Error("Current path is not null in InitSaving");

                curPathField.SetValue(instance, null);

                if (GotField("savedNodes", out var savedNodesField)) {
                    ((HashSet<string>) savedNodesField.GetValue(instance)).Clear();
                }

                if (GotField("nextListElementTemporaryId", out var nextListElementTemporaryIdField)) {
                    nextListElementTemporaryIdField.SetValue(instance, 0);
                }
            }

            var compressedStream = new CompressorStream(filePath);
            saveStreamField.SetValue(instance, compressedStream);

            try {
                Scribe.mode = LoadSaveMode.Saving;

                var settings = CompressorMod.Settings;

                var xmlWriterSettings = new XmlWriterSettings();
                if (settings.pretty) {
                    xmlWriterSettings.Indent = true;
                    xmlWriterSettings.IndentChars = "\t";
                }

                var xmlWriter = XmlWriter.Create(compressedStream.stream, xmlWriterSettings);
                writerField.SetValue(instance, xmlWriter);

                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement(documentElementName);
            } catch (Exception ex) {
                Log.Error(string.Format("Exception while init saving file: {0}\n{1}", filePath, ex));
                instance.ForceStop();
                throw;
            }

            return false;  // don't call original implementation
        }

        internal static bool DataExposeUtility__AddLineBreaksToLongString__Prefix(ref string __result, string str) {
            if (CompressorMod.Settings.pretty) {
                return true;
            } else {
                __result = str;
                return false;
            }
        }

        internal static bool DataExposeUtility__ByteArray__Prefix(ref byte[] arr, string label) {
            if (Scribe.mode != LoadSaveMode.Saving) {
                return true;  // loading
            } else if (arr is null) {
                return false;  // nothing to do
            } else if (CompressorMod.Settings.level < 0) {
                return true;  // use default
            } else {
                var b64 = Convert.ToBase64String(arr);
                b64 = DataExposeUtility.AddLineBreaksToLongString(b64);
                Scribe_Values.Look(ref b64, label);
                return false;  // don't call original implementation
            }
        }
    }
}
