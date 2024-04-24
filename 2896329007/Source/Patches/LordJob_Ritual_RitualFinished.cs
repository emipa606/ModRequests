using HarmonyLib;
using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Rachek128.RitualAttenuation.Patches
{

  [HarmonyPatch]
  static class LordJob_Ritual_RitualFinished
  {
    [HarmonyPatch(typeof(LordJob_Ritual), "RitualFinished")]
    static void Prefix(LordJob_Ritual __instance) {
      foreach (var comp in __instance.Ritual.outcomeEffect.def.comps)
      {
        if (comp is RitualOutcomeComp_Quality qual)
        {
          var data = RitualExtendedDataManager.Instance.GetFor(qual, __instance.assignments);
          if (data == null)
            continue;
          var ext = RitualExtendedDataManager.Instance.GetOrCreateFor(__instance, qual, (d)=>{
            d.Data = data;
          });

          RitualExtendedDataManager.Instance.Remove(qual, __instance.assignments);
        }
      }
    }
  }
}