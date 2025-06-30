using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace InfoCardPatches
{
    [StaticConstructorOnStartup]
    public static class Startup
    {
        static Startup()
        {
            new Harmony("InfoCardPatches.Mod").PatchAll();
        }
    }

    [HarmonyPatch(typeof(StatWorker), nameof(StatWorker.GetExplanationFinalizePart))]
    public static class StatWorker_GetExplanationFinalizePart_Patch
    {
        public static bool Prefix(ref string __result, StatWorker __instance, StatRequest req, ToStringNumberSense numberSense, float finalVal)
        {
            if (Current.Game?.World?.factionManager is null)
            {
                __result = GetExplanationFinalizePart(__instance, req, numberSense, finalVal);
                return false;
            }
            return true;
        }

        public static string GetExplanationFinalizePart(StatWorker __instance, StatRequest req, ToStringNumberSense numberSense, float finalVal)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (__instance.stat.parts != null)
            {
                for (int i = 0; i < __instance.stat.parts.Count; i++)
                {
                    string text = __instance.stat.parts[i].ExplanationPart(req);
                    if (!text.NullOrEmpty())
                    {
                        stringBuilder.AppendLine(text);
                    }
                }
            }
            if (__instance.stat.postProcessCurve != null)
            {
                float value = __instance.GetValue(req, applyPostProcess: false);
                float num = __instance.stat.postProcessCurve.Evaluate(value);
                if (!Mathf.Approximately(value, num))
                {
                    string text2 = __instance.ValueToString(value, finalized: false);
                    string text3 = __instance.stat.ValueToString(num, numberSense);
                    stringBuilder.AppendLine("StatsReport_PostProcessed".Translate() + ": " + text2 + " => " + text3);
                }
            }
            if (__instance.stat.postProcessStatFactors != null)
            {
                stringBuilder.AppendLine("StatsReport_OtherStats".Translate());
                for (int j = 0; j < __instance.stat.postProcessStatFactors.Count; j++)
                {
                    StatDef statDef = __instance.stat.postProcessStatFactors[j];
                    stringBuilder.AppendLine($"    {statDef.LabelCap}: x{statDef.Worker.GetValue(req).ToStringPercent()}");
                }
            }
            stringBuilder.Append("StatsReport_FinalValue".Translate() + ": " + __instance.stat.ValueToString(finalVal, __instance.stat.toStringNumberSense));
            return stringBuilder.ToString();
        }
    }

    [HarmonyPatch(typeof(BuildingProperties), nameof(BuildingProperties.SpecialDisplayStats))]
    public static class BuildingProperties_SpecialDisplayStats_Patch
    {
        private static List<string> tmpFenceBlockedAnimals = new List<string>();
        public static bool Prefix(ref IEnumerable<StatDrawEntry> __result, BuildingProperties __instance, ThingDef parentDef, StatRequest req)
        {
            if (Current.Game?.World?.factionManager is null)
            {
                __result = SpecialDisplayStats(__instance, parentDef, req).ToList();
                return false;
            }
            return true;
        }

        public static IEnumerable<StatDrawEntry> SpecialDisplayStats(BuildingProperties __instance, ThingDef parentDef, StatRequest req)
        {
            if (__instance.joyKind != null)
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine();
                stringBuilder.AppendLine("Stat_RecreationType_Desc".Translate());
                stringBuilder.AppendLine();
                stringBuilder.AppendLine("Stat_JoyKind_AllTypes".Translate() + ":");
                foreach (JoyKindDef allDef in DefDatabase<JoyKindDef>.AllDefs)
                {
                    stringBuilder.AppendLine("  - " + allDef.LabelCap);
                }
                yield return new StatDrawEntry(StatCategoryDefOf.Building, "StatsReport_JoyKind".Translate(), __instance.joyKind.LabelCap, stringBuilder.ToString(), 4750, __instance.joyKind.LabelCap);
            }
            if (parentDef.Minifiable)
            {
                yield return new StatDrawEntry(StatCategoryDefOf.Building, "StatsReport_WorkToUninstall".Translate(), __instance.uninstallWork.ToStringWorkAmount(), "Stat_Thing_WorkToUninstall_Desc".Translate(), 1102);
            }
            if (typeof(Building_TrapDamager).IsAssignableFrom(parentDef.thingClass))
            {
                float f = StatDefOf.TrapMeleeDamage.Worker.GetValue(req) * 0.015f;
                yield return new StatDrawEntry(StatCategoryDefOf.Building, "TrapArmorPenetration".Translate(), f.ToStringPercent(), "ArmorPenetrationExplanation".Translate(), 3000);
            }
            if (__instance.isFence)
            {
                TaggedString taggedString = "Stat_Thing_Fence_Desc".Translate();
                tmpFenceBlockedAnimals.Clear();
                tmpFenceBlockedAnimals.AddRange(from k in DefDatabase<PawnKindDef>.AllDefs
                                                where k.RaceProps.Animal && k.RaceProps.FenceBlocked
                                                select k into g
                                                select g.LabelCap.Resolve() into s
                                                orderby s
                                                select s);
                taggedString += ":\n\n";
                taggedString += tmpFenceBlockedAnimals.ToLineList("- ");
                yield return new StatDrawEntry(StatCategoryDefOf.Building, "StatsReport_Fence".Translate(), "Yes".Translate(), taggedString, 4800);
                tmpFenceBlockedAnimals.Clear();
            }
            if (!ModsConfig.IdeologyActive)
            {
                yield break;
            }
        }
    }

    [HarmonyPatch(typeof(StatsReportUtility), "Reset")]
    public static class StatsReportUtility_Reset_Patch
    {
        public static bool Prefix()
        {
            if (Current.Game?.World?.factionManager is null)
            {
                Reset();
                return false;
            }
            return true;
        }

        public static void Reset()
        {
            StatsReportUtility.scrollPosition = default(Vector2);
            StatsReportUtility.scrollPositionRightPanel = default(Vector2);
            StatsReportUtility.selectedEntry = null;
            StatsReportUtility.scrollPositioner.Arm(armed: false);
            StatsReportUtility.mousedOverEntry = null;
            StatsReportUtility.cachedDrawEntries.Clear();
            StatsReportUtility.quickSearchWidget.Reset();
            PermitsCardUtility.selectedPermit = null;
            PermitsCardUtility.selectedFaction = null;
        }
    }
}
