using HarmonyLib;
using RimWorld;
using Verse;
using VFED;

namespace ImperialFunctionality
{
    [HarmonyPatch(typeof(Thing), nameof(Thing.SetFactionDirect))]
    public static class Thing_SetFactionDirect
    {
        public static void Postfix(Thing __instance)
        {
            if (__instance.Faction != null && __instance.Faction == Faction.OfPlayerSilentFail)
            {
                var comp = __instance.TryGetComp<CompIntelExtract>();
                if (comp != null)
                {
                    comp.intelExtracted = true;
                }
            }
        }
    }
}
