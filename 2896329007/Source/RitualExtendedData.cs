using System;
using System.Collections;
using System.Reflection;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace Rachek128.RitualAttenuation
{

  [StaticConstructorOnStartup]
  public class RitualExtendedDataRef : IExposable
  {
    private static readonly FieldInfo _RitualEnded;
    private static readonly FieldInfo _Outcome;
    static RitualExtendedDataRef()
    {
      _RitualEnded = HarmonyLib.AccessTools.DeclaredField(typeof(LordJob_Ritual), "ended");
      _Outcome = HarmonyLib.AccessTools.DeclaredField(typeof(LordJob_BestowingCeremony), "outcome");
    }

    public RitualExtendedDataRef()
    {
    }

    public RitualExtendedDataRef(LordJob_Ritual ritual, RitualOutcomeComp_Quality comp)
    {
      lord = ritual?.lord;
      compIndex = ritual?.Ritual.outcomeEffect.def.comps.IndexOf(comp) ?? -1;

      if (compIndex == -1)
      {
        Log.Error("Failed to find the ritual quality comp for the given ritual.");
      }
    }

    public RitualExtendedDataRef(Lord lord, RitualOutcomeComp_Quality comp)
    {
      this.lord = lord;
      // We have to work a bit harder to get the outcome effect. So far, only bestowing ceremonies are wonky like this.
      if (lord.LordJob is LordJob_BestowingCeremony ceremony)
      {
        RitualOutcomeEffectWorker_Bestowing outcomeWorker = (RitualOutcomeEffectWorker_Bestowing)_Outcome.GetValue(ceremony);
        compIndex = outcomeWorker.def.comps.IndexOf(comp);
      }

      if (compIndex == -1)
      {
        Log.Error("Failed to find the ritual quality comp for the given ritual.");
      }
    }

    private Lord lord;
    private int compIndex = -1;

    public RitualExtendedData Data;

    public LordJob_Ritual Ritual => lord?.LordJob as LordJob_Ritual;
    public RitualOutcomeComp_Quality Comp
    {
      get 
      {
        var ritual = Ritual;
        if (ritual is LordJob_BestowingCeremony ceremony)
        {
          RitualOutcomeEffectWorker_Bestowing outcomeWorker = (RitualOutcomeEffectWorker_Bestowing)_Outcome.GetValue(ceremony);
          return outcomeWorker.def.comps[compIndex] as RitualOutcomeComp_Quality;
        }
        else
          return ritual.Ritual.outcomeEffect.def.comps[compIndex] as RitualOutcomeComp_Quality;
      }
    }

    public Lord Lord => lord;

    public void ExposeData()
    {
      if (Scribe.mode == LoadSaveMode.Saving && Ended)
      {
        Log.Error("Saved ritual extended data for an ended ritual.");
      }

      if (Scribe.mode == LoadSaveMode.Saving && Comp == null)
      {
        Log.Error("Saved ritual extended data for an invalid comp.");
      }

      Scribe_References.Look(ref lord, nameof(lord));
      Scribe_Values.Look(ref compIndex, nameof(compIndex));
      Scribe_Deep.Look(ref Data, nameof(Data));
    }

    /// <summary>
    /// Returns whether the ritual this data is for has ended. If so, it will not be saved as it's no longer needed.
    /// </summary>
    public bool Ended
    {
      get
      {
        return Ritual == null ? true : (bool)_RitualEnded.GetValue(Ritual);
      }
    }

    public bool ShouldBeSaved => compIndex > -1 && lord != null && !Ended;

    
  }

  public class RitualExtendedData : IExposable
    {
      public int RequiredAttendance;
      public int ScheduledAttendance;
      public float MaxQuality;
      public int BaseAttendance;

      public int? ActualAttendance;

      public void ExposeData()
      {
        Scribe_Values.Look(ref RequiredAttendance, nameof(RequiredAttendance));
        Scribe_Values.Look(ref ScheduledAttendance, nameof(ScheduledAttendance));
        Scribe_Values.Look(ref MaxQuality, nameof(MaxQuality));
        Scribe_Values.Look(ref BaseAttendance, nameof(BaseAttendance));
        Scribe_Values.Look(ref ActualAttendance, nameof(ActualAttendance));
      }

      public override string ToString() {
        return $"{ActualAttendance} ({ScheduledAttendance}) / {RequiredAttendance} ({BaseAttendance}): +{MaxQuality.ToStringPercent()}";
      }
    }

}