using HarmonyLib;
using RimWorld;

namespace Rachek128.RitualAttenuation.Patches
{
  [HarmonyPatch(typeof(RitualOutcomeComp_Quality))]
  [HarmonyPatch(nameof(RitualOutcomeComp_Quality.GetDesc))]
  static class RitualOutcomeComp_Quality_GetDesc
  {
    static void Postfix(RitualOutcomeComp_Quality __instance, LordJob_Ritual ritual, RitualOutcomeComp_Data data, ref string __result)
    {
      var desc = Overrides.GetQualityDescriptor(__instance.GetType());
      if (desc == null)
        return;

      __result = desc(__instance, ritual, data, __result);
    }
  }
}