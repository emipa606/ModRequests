using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace SimpleWarrants
{
    [HarmonyPatch(typeof(IncidentWorker_Raid), "TryGenerateRaidInfo")]
    public static class IncidentWorker_Raid_TryGenerateRaidInfo_Patch
    {
        public static bool huntForWarrant;

        public static void Postfix(IncidentParms parms, bool __result)
        {
            if (__result && parms.faction != null && parms.faction.HostileTo(Faction.OfPlayer) && parms.faction.def.humanlikeFaction && parms.target is Map map)
            {
                if (huntForWarrant || Rand.Chance(0.3f))
                {
                    var warrants = WarrantsManager.Instance.availableWarrants.OfType<Warrant_Pawn>().Where(x => x.pawn.Faction == Faction.OfPlayer && !x.pawn.Dead && x.pawn.Map == map).ToList();
                    if (warrants.TryRandomElement(out var warrant))
                    {
                        RaidStrategyWorker_MakeLords_Patch.warrantToHunt = warrant;
                    }
                }
            }
        }
    }
}
