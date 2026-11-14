using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UINotIncluded.Widget.Configs;
using UINotIncluded.Widget.Workers;
using Verse;

namespace UINotIncluded
{
    [StaticConstructorOnStartup]
    public static class UINI
    {
        public static readonly Harmony Harmony;

        public static List<List<ElementConfig>> ListsToCheck = new List<List<ElementConfig>>();

        static UINI()
        {
            Harmony = new Harmony(Id);
            Harmony.PatchAll();

            if (!Settings.initializedDefaultBar)
            {
                Log("Firts usage of the mod. Initializing.");
                Settings.RestoreDefaultMainBar();
                Settings.RestoreDesignationLists();
                Settings.initializedDefaultBar = true;
                Settings.lastVersion = Version;
                LoadedModManager.GetMod<UINI_Mod>().WriteSettings();
            }
            else
            {
                VersionPatcher();
            }

            CompatibilityPatches();
            LoadMainButtonsSettings();
        }

        public static string Author => "GonDragon";
        public static string Name => Assembly.GetName().Name;
        public static string Id => Author + "." + Name;

        public static string Version => Assembly.GetName().Version.ToString();

        private static Assembly Assembly => Assembly.GetAssembly(typeof(UINI));

        private static void LoadMainButtonsSettings()
        {
            if (Settings.TopBarElements.Count == 0 && Settings.BottomBarElements.Count == 0)
            {
                Warning("Empty bottom and topbar detected. Repopulating");
                Settings.RestoreDefaultMainBar();
            }

            bool DeleteNonExistentButtons(List<ElementConfig> configList)
            {
                var indexToDelete = new List<int>();
                var deleted = false;

                var i = 0;
                foreach (var config in configList)
                    try
                    {
                        if (config.GetType() == typeof(ButtonConfig)) ((ButtonConfig)config).Def.GetType();
                        else if (config.GetType() == typeof(DropdownMenuConfig))
                            deleted |= DeleteNonExistentButtons(((DropdownMenuConfig)config).elements);
                    }
                    catch
                    {
                        Warning(string.Format("Def {0} not found. Marked for removal.",
                            ((ButtonConfig)config).defName));
                        indexToDelete.Add(i);
                    }
                    finally
                    {
                        i++;
                    }

                foreach (var k in indexToDelete.OrderByDescending(k => k))
                {
                    configList.RemoveAt(k);
                    deleted |= true;
                }

                return deleted;
            }

            var somethingDeleted = DeleteNonExistentButtons(Settings.TopBarElements);
            somethingDeleted |= DeleteNonExistentButtons(Settings.BottomBarElements);
            foreach (var list in ListsToCheck) somethingDeleted |= DeleteNonExistentButtons(list);

            if (somethingDeleted) LoadedModManager.GetMod<UINI_Mod>().WriteSettings();
        }

        public static void Log(string message)
        {
            Verse.Log.Message(PrefixMessage(message));
        }

        public static void Warning(string message)
        {
            Verse.Log.Warning(PrefixMessage(message));
        }

        public static void Error(string message)
        {
            Verse.Log.Error(PrefixMessage(message));
        }

        public static void ErrorOnce(string message, string key)
        {
            Verse.Log.ErrorOnce(PrefixMessage(message), key.GetHashCode());
        }

        public static void Message(string message)
        {
            Messages.Message(message, MessageTypeDefOf.TaskCompletion, false);
        }

        private static string PrefixMessage(string message)
        {
            return $"[{Name} v{Version}] {message}";
        }

        private static void CompatibilityPatches()
        {
            try
            {
                if (LoadedModManager.RunningModsListForReading.Any(x => x.Name == "Smart Speed"))
                    Timespeed_Worker.SetSmartspeedMode();
            }
            catch (TypeLoadException ex)
            {
                Error(string.Format("Error checking if SmartSpeed its installed.\n{0}", ex.ToString()));
            }
        }

        private static void VersionPatcher()
        {
            var lastVersionUsed = new Version(Settings.lastVersion ?? "0.0.0.0");

            //if (lastVersionUsed < new Version(1, 1, 0, 0))
            //{
            //    UINI.Log("Old config-file detected. Updating to 1.1.0.0+");
            //    try
            //    {
            //        Utility.Deprecated.DeprecationManager.UpdateBarsToNewVersion();
            //    } catch
            //    {
            //        UINI.Warning("Something went wrong. Save file it's probably corrupted. Restoring default tabs.");
            //        Settings.RestoreDefaultMainBar();
            //    }

            //}

            if (lastVersionUsed < Assembly.GetName().Version)
            {
                Settings.lastVersion = Version;
                LoadedModManager.GetMod<UINI_Mod>().WriteSettings();
            }
        }
    }
}