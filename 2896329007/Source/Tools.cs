using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Rachek128.RitualAttenuation
{
  [HarmonyPatch]
  internal static class Tools
  {
    /// <summary>
    /// A representation of the current interpolation function, for display in the GUI.
    /// </summary>
    internal static SimpleCurve interpolationCurve = null;
    private static float interpolationCurveExponent = 3.0f;

    /// <summary>
    /// Attempts to clamp a float to a specific number of decimal places. Additional precision will be lost.
    /// </summary>
    public static float ClampToFixed(this float value, int decimalPlaces)
    {
      return float.Parse(value.ToString("F" + decimalPlaces));
    }

    /// <summary>
    /// Calculates the bonus contribution for excess ritual participants. If not using the bonus uncap mode, the result will always be zero.
    /// </summary>
    /// <param name="currentParticipants">The number of active participants in the ritual.</param>
    /// <param name="requiredParticipants">The required number of active participants in the ritual, based off of scaled population numbers.</param>
    /// <param name="vanillaCap">The required number of participants in the ritual, according to vanilla settings.</param>
    public static float CalculateBonusContribution(float currentParticipants, float possibleParticipants, float vanillaCap)
    {
      if (Mod.settings.EffectiveUncapMode != UncapMode.Bonus)
        return 0;

      currentParticipants -= vanillaCap;
      possibleParticipants -= vanillaCap;

      float requiredParticipants = possibleParticipants * Mod.settings.requiredPopulationPercentage;

      if (currentParticipants > requiredParticipants)
        currentParticipants = requiredParticipants;

      if (currentParticipants <= 0)
        return 0;

      float percentage = Mathf.Clamp01(requiredParticipants > 0 ? currentParticipants / requiredParticipants : 1);
      float interpolated = 1.0f - Mathf.Pow(1.0f - percentage, Mod.settings.bonusExponent);

      return Mod.settings.maximumBonusPercentage * interpolated;
    }

    /// <summary>
    /// Updates the interpolation curve used by the settings GUI, if needed.
    /// </summary>
    public static void RefreshInterpolationCurve()
    {
      if (interpolationCurve != null && interpolationCurveExponent == Mod.settings.bonusExponent)
        return;

      interpolationCurve = new SimpleCurve(GetInterpolationCurvePoints());
      interpolationCurveExponent = Mod.settings.bonusExponent;
    }

    /// <summary>
    /// Samples the current interpolation curve for the settings GUI.
    /// </summary>
    private static IEnumerable<CurvePoint> GetInterpolationCurvePoints()
    {
      for (var i = 0;i <= 32;++i)
      {
        float x = i / 32.0f;
        float y = 1.0f - Mathf.Pow(1.0f - x, Mod.settings.bonusExponent);

        yield return new CurvePoint(x, y);
      }
    }

    public static string UncapModeDescriptionKey(UncapMode mode) {
      return $"Rachek_RSA_{mode}_Description";
    }

    public static string UncapModeNameKey(UncapMode mode) {
      return $"Rachek_RSA_{mode}";
    }

    [HarmonyReversePatch]
    [HarmonyPatch(typeof(RitualOutcomeComp_ParticipantCount), "Counts")]
    internal static bool ParticipantCount_Counts(object instance, RitualRoleAssignments assignments, Pawn p)
    {
      return false;
    }

    [HarmonyReversePatch]
    [HarmonyPatch(typeof(RitualOutcomeComp_Quality), "ExpectedOffsetDesc")]
    internal static string ExpectedOffsetDesc(object instance, bool positive, float quality = 0.0F)
    {
      return null;
    }

    /// <summary>
    /// Gets the possible number of participants for the ritual, scaled by population requirement.
    /// </summary>
    public static int GetPossibleParticipants(RitualRoleAssignments assignments, RitualOutcomeComp_ParticipantCount instance)
    {
      return assignments.AllPawns.Count(p => ParticipantCount_Counts(instance, assignments, p));
    }

    /// <summary>
    /// Gets the possible number of participants for the ritual, scaled by population requirement.
    /// </summary>
    public static int GetPossibleParticipants(RitualRoleAssignments assignments, RitualOutcomeComp_NumParticipantsWithTag instance)
    {
      return assignments.AllPawns.Count(p => p.RaceProps.Humanlike);
    }

    /// <summary>
    /// Gets the possible number of participants for the ritual, scaled by population requirement.
    /// </summary>
    public static int GetPossibleParticipants(LordJob_Ritual ritual, RitualOutcomeComp_ParticipantCount instance)
    {
      if (ritual.assignments == null && ritual is LordJob_BestowingCeremony bestow)
      {
        return bestow.colonistParticipants.Count;
      }
      else
      {
        return GetPossibleParticipants(ritual.assignments, instance);
      }
    }

    /// <summary>
    /// Gets the possible number of drums for the ritual, scaled by population requirement.
    /// </summary>
    public static int GetPossibleDrums(RitualOutcomeComp_NumPlayedDrums instance, TargetInfo ritualTarget)
    {
      return ritualTarget.Map.listerBuldingOfDefInProximity.GetForCell(ritualTarget.Cell, (float) instance.maxDistance, ThingDefOf.Drum).Count;
    }

    /// <summary>
    /// Gets the required number of participants for the ritual, scaled by population requirement.
    /// </summary>
    public static int GetRequiredParticipants(LordJob_Ritual ritual, RitualOutcomeComp_ParticipantCount instance)
    {
      int count = 0;
      if (ritual.assignments == null && ritual is LordJob_BestowingCeremony bestow)
      {
        count = bestow.PawnsToCountTowardsPresence.Count();
      }
      else
      {
        count = GetPossibleParticipants(ritual.assignments, instance);
      }
      return Mathf.CeilToInt(count * Mod.settings.requiredPopulationPercentage);
    }

    /// <summary>
    /// Gets the required number of participants for the ritual, scaled by population requirement.
    /// </summary>
    public static int GetRequiredParticipants(RitualRoleAssignments assignments, RitualOutcomeComp_ParticipantCount instance)
    {
      return Mathf.CeilToInt(GetPossibleParticipants(assignments, instance) * Mod.settings.requiredPopulationPercentage);
    }

    /// <summary>
    /// Gets the required number of participants for the ritual, scaled by population requirement.
    /// </summary>
    public static int GetRequiredParticipants(RitualRoleAssignments assignments, RitualOutcomeComp_NumParticipantsWithTag instance)
    {
      return Mathf.CeilToInt(GetPossibleParticipants(assignments, instance) * Mod.settings.requiredPopulationPercentage);
    }

    /// <summary>
    /// Gets the current number of participants for the ritual.
    /// </summary>
    public static int GetCurrentParticipants(RitualRoleAssignments assignments, RitualOutcomeComp_ParticipantCount instance)
    {
      return assignments.Participants.Count<Pawn>(p => ParticipantCount_Counts(instance, assignments, p));
    }

    /// <summary>
    /// Gets the required number of drums for the ritual, scaled by population requirement.
    /// </summary>
    public static int GetRequiredDrums(RitualRoleAssignments assignments, RitualOutcomeComp_NumPlayedDrums instance, float vanillaCap)
    {
      // Scale drums assuming a default population of 10.
      var currentPop = assignments.Participants.Count<Pawn>(p => p.RaceProps.Humanlike);
      var ratio = vanillaCap / 10f;
      return Mathf.CeilToInt(currentPop * ratio * Mod.settings.requiredPopulationPercentage);
    }

    /// <summary>
    /// Gets the current number of participants for the ritual.
    /// </summary>
    public static int GetCurrentParticipants(RitualRoleAssignments assignments, RitualOutcomeComp_NumParticipantsWithTag instance)
    {
      return assignments.Participants.Count;
    }

    public static int CountActiveParticipants(LordJob_Ritual ritual, RitualOutcomeComp_Data data, RitualOutcomeComp_ParticipantCount instance)
    {
      int participated = 0;
      RitualOutcomeComp_DataThingPresence dataThingPresence = (RitualOutcomeComp_DataThingPresence)data;
      float duration = ritual.DurationTicks != 0 ? (float)ritual.DurationTicks : ritual.TicksPassedWithProgress;

      foreach (var presentForTick in dataThingPresence.presentForTicks)
      {
        Pawn key = (Pawn)presentForTick.Key;
        if (Tools.ParticipantCount_Counts(instance, ritual.assignments, key) && (double)presentForTick.Value >= (double)duration / 2.0)
          ++participated;
      }

      return participated;
    }

    public static int CountActiveParticipants(LordJob_Ritual ritual, RitualOutcomeComp_Data data, RitualOutcomeComp_NumParticipantsWithTag instance)
    {
      int participated = 0;
      foreach (Pawn participant in ritual.assignments.Participants)
      {
        if (ritual.PawnTagSet(participant, instance.tag))
          ++participated;
      }
      return participated;
    }

    public static int CountPlayedDrums(LordJob_Ritual ritual, RitualOutcomeComp_Data data, RitualOutcomeComp_NumPlayedDrums instance)
    {
      int participated = 0;
      RitualOutcomeComp_DataThingPresence dataThingPresence = (RitualOutcomeComp_DataThingPresence)data;
      foreach (var presentForTick in dataThingPresence.presentForTicks)
      {
        if (presentForTick.Value >= (ritual.DurationTicks / 2f))
          ++participated;
      }
      return participated;
    }

    /// <summary>
    /// Calculates the effective number of participants for a ritual.
    /// </summary>
    /// <param name="currentParticipants">The number of active participants in the ritual.</param>
    /// <param name="requiredParticipants">The required number of active participants in the ritual, based off of scaled population numbers.</param>
    /// <param name="vanillaCap">The required number of participants in the ritual, according to vanilla settings.</param>
    public static float CalculateEffectiveParticipants(float currentParticipants, float requiredParticipants, float vanillaCap)
    {
      if (currentParticipants < 1f)
        return requiredParticipants < 1f ? vanillaCap : 0f;
      switch (Mod.settings.EffectiveUncapMode)
      {
        case UncapMode.None:
          return CalculateEffectiveCapped(currentParticipants, requiredParticipants, vanillaCap);
        case UncapMode.Bonus:
          return CalculateEffectiveCapped(currentParticipants, requiredParticipants, vanillaCap);
        case UncapMode.Scaled:
          return CalculateEffectiveUncapped(currentParticipants, requiredParticipants, vanillaCap);
      }
      return 0;
    }

    private static float CalculateEffectiveCapped(float currentParticipants, float requiredParticipants, float vanillaCap)
    {
      if (requiredParticipants >= vanillaCap)
        return Math.Min(currentParticipants, vanillaCap);
      float percentage = Mathf.Clamp01((requiredParticipants > 0) ? currentParticipants / requiredParticipants : 1f);
      return vanillaCap * percentage;
    }

    private static float CalculateEffectiveUncapped(float currentParticipants, float requiredParticipants, float vanillaCap)
    {
      float percentage = Mathf.Clamp01((requiredParticipants > 0) ? currentParticipants / requiredParticipants : 1f);
      return vanillaCap * percentage;
    }

    internal static float MaxValue(this SimpleCurve curve)
    {
      return curve[curve.PointsCount - 1].y;
    }

    internal static float MaxDomain(this SimpleCurve curve)
    {
      return curve[curve.PointsCount - 1].x;
    }
  }
}