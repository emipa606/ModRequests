using HarmonyLib;
using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;
using VFED;

namespace ImperialFunctionality
{
    [HarmonyPatch(typeof(IncidentWorker_RaidEnemy), "TryResolveRaidFaction")]
    public static class IncidentWorker_RaidEnemy_TryResolveRaidFaction_Patch
    {
        public static void Postfix(IncidentParms parms, ref bool __result)
        {
            if (__result && parms.target is Map map)
            {
                var stations = map.listerBuildings.AllBuildingsColonistOfDef(IF_DefOf.VFED_SurveillanceStation).ToList();
                foreach (var station in stations)
                {
                    var comp = station.GetComp<CompSurveillanceScanner>();
                    var pointsToReduce = Mathf.Min(comp.raidPointsFound, parms.points);
                    parms.points -= pointsToReduce;
                    comp.raidPointsFound -= pointsToReduce;
                    if (parms.points <= 0)
                    {
                        __result = false;
                    }
                }
            }
        }
    }
}
