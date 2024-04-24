using HarmonyLib;
using RimWorld;
using Verse;

namespace Rachek128.RitualAttenuation.Patches
{
  [HarmonyPatch(typeof(RitualOutcomeComp_ParticipantCount))]
  [HarmonyPatch(nameof(RitualOutcomeComp_ParticipantCount.GetExpectedOutcomeDesc))]
  static class RitualOutcomeComp_ParticipantCount_GetExpectedOutcomeDesc
  {
    static void Postfix(RitualOutcomeComp_ParticipantCount __instance, RitualRoleAssignments assignments, ref ExpectedOutcomeDesc __result)
    {
      var data = RitualExtendedDataManager.Instance.GetOrCreateFor(__instance, assignments, (d)=>{
        int required = PopulationUtility.GetScaledAttendanceFor(assignments, __instance);
        d.RequiredAttendance = required;
        d.BaseAttendance = PopulationUtility.GetBaseAttendanceFor(__instance);
        d.MaxQuality = PopulationUtility.GetBaseQualityFor(__instance);
      });

      data.ScheduledAttendance = PopulationUtility.GetCurrentAttendanceFor(assignments);
      
      __result.count = $"{data.ScheduledAttendance} / {data.RequiredAttendance}";
      __result.quality = PopulationUtility.EvaluateQualityFor(data, __instance);
      __result.positive = __result.quality > 0f;
      __result.effect = Tools.ExpectedOffsetDesc(__instance, __result.positive, __result.quality);
    }
  }

  [HarmonyPatch(typeof(RitualOutcomeComp_ParticipantCount))]
  [HarmonyPatch(nameof(RitualOutcomeComp_ParticipantCount.Count))]
  static class RitualOutcomeComp_ParticipantCount_Count
  {
    static void Postfix(RitualOutcomeComp_ParticipantCount __instance, LordJob_Ritual ritual, RitualOutcomeComp_Data data, ref float __result)
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
      RitualOutcomeComp_DataThingPresence dataThingPresence = (RitualOutcomeComp_DataThingPresence)data;
      float num2 = ritual.DurationTicks != 0 ? (float) ritual.DurationTicks : ritual.TicksPassedWithProgress;
      foreach (var presentForTick in dataThingPresence.presentForTicks)
      {
        Pawn key = (Pawn) presentForTick.Key;
        if (Tools.ParticipantCount_Counts(__instance, ritual.assignments, key) && presentForTick.Value >= num2 / 2.0f)
          ++present;
      }
      __result = present;

      ext.Data.ActualAttendance = present;
    }
  }
}