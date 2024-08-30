using HarmonyLib;
using RimWorld;
using System;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Revolus.MoreAutosaveSlots {
    public class MoreAutosaveSlotsMod : Mod {
        public static MoreAutosaveSlotsSettings Settings;

        public MoreAutosaveSlotsMod(ModContentPack content) : base(content) {
            Settings = this.GetSettings<MoreAutosaveSlotsSettings>();

            var harmony = new Harmony(typeof(MoreAutosaveSlotsMod).AssemblyQualifiedName);
            harmony.Patch(
                AccessTools.Method(typeof(Autosaver), "AutosaverTick", new Type[] { }),
                prefix: new HarmonyMethod(typeof(MoreAutosaveSlotsMod), nameof(Patch__Autosaver__AutosaverTick__Prefix))
            );
            harmony.Patch(
                AccessTools.Method(typeof(Autosaver), "NewAutosaveFileName", new Type[] { }),
                prefix: new HarmonyMethod(typeof(MoreAutosaveSlotsMod), nameof(Patch__Autosaver__NewAutosaveFileName__Prefix))
            );
            harmony.Patch(
                AccessTools.Method(typeof(GenText), "IsValidFilename", new Type[] { typeof(string) }),
                prefix: new HarmonyMethod(typeof(MoreAutosaveSlotsMod), nameof(Patch__GenText__IsValidFilename__Prefix))
            );
            harmony.Patch(
                AccessTools.Constructor(typeof(Dialog_SaveFileList_Save), new Type[] { }),
                postfix: new HarmonyMethod(typeof(MoreAutosaveSlotsMod), nameof(Patch__Dialog_SaveFileList_Save__Ctor__Postfix))
            );
        }

        public override void DoSettingsWindowContents(Rect rect) {
            bool changed;
            
            var listing = new Listing_Standard();
            listing.Begin(rect);
            try {
                TextAnchor oldAnchorValue = Text.Anchor;
                try {
                    changed = Settings.ShowAndChangeSettings(listing);
                } finally {
                    Text.Anchor = oldAnchorValue;
                }
            } finally {
                listing.End();
            }

            if (changed) {
                SoundDefOf.DragSlider.PlayOneShotOnCamera();
            }
            
            base.DoSettingsWindowContents(rect);
        }

        public override string SettingsCategory() {
            return "MoreAutosaveSlots.SettingsName".Translate();
        }

        private static bool Patch__Autosaver__AutosaverTick__Prefix(ref Autosaver __instance) {
            var hours = Settings.hours;
            if (hours <= 0) {
                return true;  // use default implementation
            }

            FieldInfo ticksSinceSaveField = AccessTools.Field(typeof(Autosaver), "ticksSinceSave");
            if (ticksSinceSaveField is null) {
                return true;  // use default implementation
            }

            Autosaver instance = __instance;

            var ticksSinceSave = (int) ticksSinceSaveField.GetValue(instance);
            var ticksPerDay = 60000;
            var ticksPerHour = ticksPerDay / 24;
            var doAutosaveThen = hours * ticksPerHour;

            if (++ticksSinceSave >= doAutosaveThen) {
                LongEventHandler.QueueLongEvent(instance.DoAutosave, "Autosaving", doAsynchronously: false, null);
                ticksSinceSave = 0;
            }
            ticksSinceSaveField.SetValue(instance, ticksSinceSave);

            return false;  // don't call default implementation
        }

        private static bool Patch__Autosaver__NewAutosaveFileName__Prefix(ref Autosaver __instance, ref string __result) {
            __result = MoreAutosaveSlotsSettings.NextName();
            return false;  // don't call default implementation
        }

        private static bool Patch__GenText__IsValidFilename__Prefix(ref bool __result, string str) {
            __result = (str.Length <= 60) && !new Regex("[" + Regex.Escape(GenText.GetInvalidFilenameCharacters()) + "]").IsMatch(str);
            return false;  // don't call default implementation
        }

        private static void Patch__Dialog_SaveFileList_Save__Ctor__Postfix(ref Dialog_SaveFileList_Save __instance) {
            if (!Settings.useNextSaveName) {
                return;
            }

            var typingName = AccessTools.Field(typeof(Dialog_SaveFileList_Save), "typingName");
            if (typingName is null) {
                return;
            }

            typingName.SetValue(__instance, MoreAutosaveSlotsSettings.NextName());
        }
    }
}
