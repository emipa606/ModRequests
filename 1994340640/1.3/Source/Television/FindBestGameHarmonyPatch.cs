using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace WallStuff.Television
{
    [HarmonyPatch(typeof(JoyGiver_InteractBuilding))]
    [HarmonyPatch("FindBestGame")]
    public class FindBestGameHarmonyPatch
    {  
        private static void Postfix(JoyGiver_WatchBuilding __instance,  ref Thing __result, Pawn pawn, bool inBed, IntVec3 gatheringSpot)
        {
            //jcLog.Warning(pawn.Name + " Find Best");
            List<Thing>  tmpCandidates = new List<Thing>();

            MethodInfo methodInfoGetSearchSet = typeof(JoyGiver_InteractBuilding).GetMethod("GetSearchSet", BindingFlags.NonPublic | BindingFlags.Instance);
            var parametersGetSearchSet = new object[] { pawn, tmpCandidates };

            methodInfoGetSearchSet.Invoke(__instance, parametersGetSearchSet); // GetSearchSet(pawn, tmpCandidates, __instance);
            if (tmpCandidates.Count == 0)
            {
                __result = null;
                return;
            }

            MethodInfo methodInfoCanInteractWith = typeof(JoyGiver_InteractBuilding).GetMethod("CanInteractWith", BindingFlags.NonPublic | BindingFlags.Instance);

            Predicate<Thing> predicate = (Thing t) => (bool)methodInfoCanInteractWith.Invoke(__instance, new object[] { pawn, t, inBed }); // CanInteractWith(pawn, t, inBed)
            if (gatheringSpot.IsValid)
            {
                Predicate<Thing> oldValidator = predicate;
                predicate = ((Thing x) => GatheringsUtility.InGatheringArea(x.Position, gatheringSpot, pawn.Map) && oldValidator(x));
            }
            IntVec3 position = pawn.Position;
            Map map = pawn.Map;
            List<Thing> searchSet = tmpCandidates;
            Predicate<Thing> validator = predicate;
            Thing thing = searchSet[0];
            __result = GenClosest.ClosestThing_Global(position, searchSet, 9999f, validator, null);
            tmpCandidates.Clear();
        }

    }
}
