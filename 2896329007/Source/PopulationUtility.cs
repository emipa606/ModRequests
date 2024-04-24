using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Rachek128.RitualAttenuation
{
  public static class PopulationUtility
  {
    public static int GetCurrentColonyPopulation()
    {
      return Find.CurrentMap.mapPawns.ColonistCount;
    }

    public static int GetPossiblePopulationFor(RimWorld.RitualRoleAssignments assignments, bool excludeFixed = false)
    {
      if (!excludeFixed)
        return assignments.AllPawns.Count;

      int count = 0;
      foreach (var pawn in assignments.AllPawns)
      {
        if (assignments.Forced(pawn) || assignments.Required(pawn))
          continue;
        count++;
      }

      return count;
    }

    public static int GetBaseAttendanceFor(RimWorld.RitualOutcomeComp_Quality comp)
    {
      // Because there's no real standard way of distinguishing attendance requirements from other quality outcome comps, we have to handle them manually.
      // Might break this out into an event later to allow for mod compatibility patches.

      if (comp is RimWorld.RitualOutcomeComp_ParticipantCount participantCount)
      {
        return (int)participantCount.curve.MaxDomain();
      }

      if (comp is RimWorld.RitualOutcomeComp_NumParticipantsWithTag participantTags)
      {
        return (int)participantTags.curve.MaxDomain();
      }

      if (comp is RimWorld.RitualOutcomeComp_NumPlayedDrums playedDrums)
      {
        return (int)playedDrums.curve.MaxDomain();
      }

      return 0;
    }

    public static int GetMaxAttendanceFor(RimWorld.RitualOutcomeComp_Quality comp) {
      if (comp is RimWorld.RitualOutcomeComp_NumPlayedDrums)
        return GetBaseAttendanceFor(comp);

      if (Mod.settings.uncapMode == UncapMode.None || Mod.settings.uncapMode == UncapMode.Bonus)
        return GetBaseAttendanceFor(comp);

      return int.MaxValue;
    }

    public static int GetScaledAttendanceFor(RimWorld.RitualRoleAssignments assignments, RimWorld.RitualOutcomeComp_Quality comp)
    {
      int baseCount = GetBaseAttendanceFor(comp);
      if (baseCount <= 0)
        return 0;

      int available = GetPossiblePopulationFor(assignments, true);

      return Math.Max(0, Math.Min(Mathf.CeilToInt(available * Mod.settings.requiredPopulationPercentage), GetMaxAttendanceFor(comp)));
    }

    public static int GetCurrentAttendanceFor(RimWorld.RitualRoleAssignments assignments)
    {
      return assignments.AllPawns.Where(x => !assignments.Forced(x) && !assignments.Required(x) && assignments.PawnParticipating(x)).Count();
    }

    public static int GetCurrentDrumsFor(TargetInfo ritualTarget, int maxDistance)
    {
      return ritualTarget.Map.listerBuldingOfDefInProximity.GetForCell(ritualTarget.Cell, (float) maxDistance, ThingDefOf.Drum).Count;
    }

    public static float GetBaseQualityFor(RimWorld.RitualOutcomeComp_Quality comp) {
      return comp.curve.MaxValue();
    }

    public static float EvaluateQualityFor(RitualExtendedData data, RimWorld.RitualOutcomeComp_Quality comp)
    {
      float required = data.RequiredAttendance;
      float current = data.ActualAttendance ?? data.ScheduledAttendance;

      var baseAmt = data.BaseAttendance;

      var percentage = Mathf.Clamp01(required > 0 ? current / required : 1.0f);

      switch (Mod.settings.EffectiveUncapMode)
      {
        case UncapMode.Bonus:
          if (current <= baseAmt)
            return comp.curve.Evaluate(percentage * baseAmt);
            
          current -= baseAmt;
          required = baseAmt * Mod.settings.requiredPopulationPercentage;

          percentage = Mathf.Clamp01(required > 0 ? current / required : 1.0f);

          float baseQuality = data.MaxQuality;
          float interpolated = 1.0f - Mathf.Pow(1.0f - percentage, Mod.settings.bonusExponent);

          return baseQuality + comp.curve.Evaluate(interpolated * baseAmt);
        case UncapMode.None:
        case UncapMode.Scaled:
        default:
          return comp.curve.Evaluate(percentage * baseAmt);
      }
    }
  }
}