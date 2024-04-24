using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Rachek128.RitualAttenuation.Patches
{
  [HarmonyPatch(typeof(RitualOutcomeComp_NumPlayedDrums))]
  [HarmonyPatch(nameof(RitualOutcomeComp_NumPlayedDrums.GetExpectedOutcomeDesc))]
  static class RitualOutcomeComp_NumPlayedDrums_GetExpectedOutcomeDesc
  {
    static void Postfix(RitualOutcomeComp_NumPlayedDrums __instance, TargetInfo ritualTarget, RitualRoleAssignments assignments, ref ExpectedOutcomeDesc __result)
    {
      var data = RitualExtendedDataManager.Instance.GetOrCreateFor(__instance, assignments, (d)=>{
        int required = PopulationUtility.GetScaledAttendanceFor(assignments, __instance);
        d.BaseAttendance = PopulationUtility.GetBaseAttendanceFor(__instance);
        d.RequiredAttendance = Math.Max(1, Math.Min(required, d.BaseAttendance));
        d.MaxQuality = PopulationUtility.GetBaseQualityFor(__instance);
      });

      data.ScheduledAttendance = Math.Min(PopulationUtility.GetCurrentDrumsFor(ritualTarget, __instance.maxDistance), data.RequiredAttendance);
      
      __result.count = $"{data.ScheduledAttendance} / {data.RequiredAttendance}";
      __result.quality = PopulationUtility.EvaluateQualityFor(data, __instance);
      __result.positive = __result.quality > 0f;
      __result.effect = Tools.ExpectedOffsetDesc(__instance, __result.positive, __result.quality);
    }
  }

  [HarmonyPatch(typeof(RitualOutcomeComp_NumPlayedDrums))]
  [HarmonyPatch(nameof(RitualOutcomeComp_NumPlayedDrums.Count))]
  static class RitualOutcomeComp_NumPlayedDrums_Count
  {
    static void Postfix(RitualOutcomeComp_NumPlayedDrums __instance, LordJob_Ritual ritual, RitualOutcomeComp_Data data, ref float __result)
    {
      var ext = RitualExtendedDataManager.Instance.GetFor(ritual, __instance);

      if (ext == null)
      {
        ext = RitualExtendedDataManager.Instance.GetFor(ritual.lord, __instance);
        if (ext == null)
        {
          return;
        }
      }

      int present = 0;
      foreach (var presentForTick in ((RitualOutcomeComp_DataThingPresence) data).presentForTicks)
      {
        if ((double) presentForTick.Value >= (double) (ritual.DurationTicks / 2))
          ++present;
      }
      __result = Math.Min(present, ext.Data.RequiredAttendance);

      ext.Data.ActualAttendance = Math.Min(present, ext.Data.RequiredAttendance);
    }
  }
}