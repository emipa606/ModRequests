using System;
using System.Reflection;
using RimWorld;
using Verse;

namespace Rachek128.RitualAttenuation.Patches
{
  [Verse.StaticConstructorOnStartup]
  static class RitualOutcomeComp_ParticipantCount_Overrides
  {
    private static FieldInfo _label;
    static RitualOutcomeComp_ParticipantCount_Overrides()
    {
      Overrides.RegisterQualityDescriptor<RitualOutcomeComp_ParticipantCount>(GetDesc);
      Overrides.RegisterQualityOffset<RitualOutcomeComp_ParticipantCount>(QualityOffset);
      _label = HarmonyLib.AccessTools.DeclaredField(typeof(RitualOutcomeComp), "label");
    }

    private static float QualityOffset(RitualOutcomeComp_ParticipantCount comp, LordJob_Ritual ritual, RitualOutcomeComp_Data data, float result)
    {
      if (ritual == null)
        return result;
      
      var ext = RitualExtendedDataManager.Instance.GetFor(ritual, comp);

      if (ext == null)
      {
        ext = RitualExtendedDataManager.Instance.GetFor(ritual.lord, comp);
        if (ext == null)
        {
          return result;
        }
      }

      return PopulationUtility.EvaluateQualityFor(ext.Data, comp);
    }

    private static string GetDesc(RitualOutcomeComp_ParticipantCount comp, LordJob_Ritual ritual, RitualOutcomeComp_Data data, string result)
    {
      if (ritual == null)
        return result;

      var ext = RitualExtendedDataManager.Instance.GetFor(ritual, comp);

      if (ext == null)
      {
        ext = RitualExtendedDataManager.Instance.GetFor(ritual.lord, comp);
        if (ext == null)
        {
          return result;
        }
      }

      float qualityOffset = PopulationUtility.EvaluateQualityFor(ext.Data, comp);

      return (string) ($"{ext.Data.ActualAttendance} / {ext.Data.RequiredAttendance} {_label.GetValue(comp)}: " + "OutcomeBonusDesc_QualitySingleOffset".Translate((NamedArgument) ("+" + qualityOffset.ToStringPercent())) + ".");
    }
  }
}