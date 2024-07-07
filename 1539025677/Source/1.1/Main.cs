using System;
using Verse;
using HarmonyLib;
using System.Reflection;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace CutPlantsBeforeBuilding {
    [StaticConstructorOnStartup]
    class Main {
        public static bool autoDesignatePlantsCutMode = true;

        static Main() {
            var harmony = new Harmony("com.tammybee.cutplantsbeforebuilding");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [HarmonyPatch(typeof(PlaySettings))]
    [HarmonyPatch("DoPlaySettingsGlobalControls")]
    class PlaySettings_DoPlaySettingsGlobalControls_Patch {
        static void Prefix(WidgetRow row, bool worldView) {
            if (MyKeyBindingDefOf.TMB_ToggleAutoDesignatePlantsCutMode.JustPressed) {
                Main.autoDesignatePlantsCutMode = !Main.autoDesignatePlantsCutMode;
                if (Main.autoDesignatePlantsCutMode) {
                    SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
                } else {
                    SoundDefOf.Tick_Low.PlayOneShotOnCamera(null);
                }
            }

            if (!worldView) {
                bool autoDesignatePlantsCutMode = Main.autoDesignatePlantsCutMode;
                Texture2D texButton = MyTex.ButtonAutoDesignatePlantsCutMode;
                string hotkeyLabel = MyKeyBindingDefOf.TMB_ToggleAutoDesignatePlantsCutMode.MainKeyLabel;
                string modeLabel = autoDesignatePlantsCutMode ? "CutPlantsBeforeBuilding.ModeON".Translate() : "CutPlantsBeforeBuilding.ModeOFF".Translate();
                string tooltip = "CutPlantsBeforeBuilding.AutoDesignatePlantsCutModeToggleButton".Translate(hotkeyLabel, modeLabel);
                row.ToggleableIcon(ref autoDesignatePlantsCutMode, texButton, tooltip, SoundDefOf.Mouseover_ButtonToggle, null);
                Main.autoDesignatePlantsCutMode = autoDesignatePlantsCutMode;
            }
        }
    }

    [HarmonyPatch(typeof(Designator_Build))]
    [HarmonyPatch("DesignateSingleCell")]
    class Patch_Designator_Build_DesignateSingleCell {
        static void Postfix(Designator_Build __instance,IntVec3 c) {
            //Log.Message("Designator_Build.DesignateSingleCell");
            Traverse trv = Traverse.Create(__instance);
            Rot4 rot = trv.Field("placingRot").GetValue<Rot4>();
            Map map = trv.Property("Map").GetValue<Map>();
            ThingDef stuffDef = trv.Property("stuffDef").GetValue<ThingDef>();
            Util.DesignatePlants(c, rot, __instance.PlacingDef, stuffDef, map);
        }
    }

    [HarmonyPatch(typeof(Designator_Install))]
    [HarmonyPatch("DesignateSingleCell")]
    class Patch_Designator_Install_DesignateSingleCell {
        static void Postfix(Designator_Build __instance, IntVec3 c) {
            //Log.Message("Designator_Install.DesignateSingleCell");
            Traverse trv = Traverse.Create(__instance);
            Rot4 rot = trv.Field("placingRot").GetValue<Rot4>();
            Map map = trv.Property("Map").GetValue<Map>();
            ThingDef stuffDef = trv.Property("stuffDef").GetValue<ThingDef>();
            Util.DesignatePlants(c, rot, __instance.PlacingDef, stuffDef, map);
        }
    }

    [HarmonyPatch(typeof(Designator_AreaBuildRoof))]
    [HarmonyPatch("DesignateSingleCell")]
    class Patch_Designator_AreaBuildRoof_DesignateSingleCell {
        static void Postfix(Designator_AreaBuildRoof __instance, IntVec3 c) {
            foreach (Thing thing in __instance.Map.thingGrid.ThingsAt(c)) {
                if (thing.def.plant != null && thing.def.plant.interferesWithRoof) {
                    Util.DesignatePlant(c, __instance.Map);
                    break;
                }
            }
        }
    }
    
    [HarmonyPatch(typeof(AutoBuildRoofAreaSetter))]
    [HarmonyPatch("TryGenerateAreaNow")]
    class Patch_AutoBuildRoofAreaSetter_TryGenerateAreaNow {
        static void Postfix(AutoBuildRoofAreaSetter __instance) {
            //Log.Message("AutoBuildRoofAreaSetter.TryGenerateAreaNow");
            Traverse trv = Traverse.Create(__instance);
            Map map = trv.Field("map").GetValue<Map>();
            HashSet<IntVec3> cells = trv.Field("innerCells").GetValue<HashSet<IntVec3>>();
            foreach(IntVec3 c in cells) {
                foreach (Thing thing in map.thingGrid.ThingsAt(c)) {
                    if (thing.def.plant != null && thing.def.plant.interferesWithRoof) {
                        Util.DesignatePlant(c, map);
                        break;
                    }
                }
            }
        }
    }
}
