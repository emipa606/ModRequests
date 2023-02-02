using HarmonyLib;
using RimWorld;
using Verse;

namespace CM_Less_Shitty_Ambush
{
    [StaticConstructorOnStartup]
    public static class IncidentWorker_CaravanDemand_Patches
    {
        [HarmonyPatch(typeof(IncidentWorker_CaravanDemand))]
        [HarmonyPatch("TryExecuteWorker", MethodType.Normal)]
        public static class IncidentWorker_CaravanDemand_TryExecuteWorker
        {
            [HarmonyPrefix]
            public static void Prefix(IncidentWorker_CaravanDemand __instance, ref IncidentParms parms)
            {
                float newPoints = parms.points * LessShittyAmbushMod.settings.enemyFactionMultiplier;
                Logger.MessageFormat(__instance, "Muliplying enemy faction points: {0} * {1} = {2}", parms.points, LessShittyAmbushMod.settings.enemyFactionMultiplier, newPoints);
                parms.points = newPoints;
            }
        }
    }
}
