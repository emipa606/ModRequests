using HarmonyLib;
using RimWorld;
using Verse;

namespace NoobertNebulous
{
    [HarmonyPatch(typeof(IncidentWorker_RaidEnemy), "ResolveRaidPoints")]
    public static class IncidentWorker_RaidEnemy_ResolveRaidPoints_Patch
    {
        public static void Postfix(IncidentParms parms)
        {
            if (NoobertNebulousStoryteller.StorytellerActive)
            {
                parms.points *= Rand.Range(1f, 2f);
            }
        }
    }
}
