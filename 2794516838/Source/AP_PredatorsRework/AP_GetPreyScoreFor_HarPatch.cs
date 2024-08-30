using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using HarmonyLib;
using Verse;
using UnityEngine;
using Verse.AI;

namespace AP_PredatorsRework
{
    [HarmonyPatch(typeof(FoodUtility), "BestPawnToHuntForPredator")]
    public static class AP_GetPreyScoreFor_HarPatch
    {
        public static bool predHuntMoreColonists = false;
        private static int GetMaxRegionsToScan(Pawn getter, bool forceScanWholeMap)
        {
            if (getter.RaceProps.Humanlike)
            {
                return -1;
            }
            if (forceScanWholeMap)
            {
                return -1;
            }
            if (getter.Faction == Faction.OfPlayer)
            {
                return 100;
            }
            return 30;
        }

        [HarmonyPostfix]
        public static void Fix(Pawn predator, bool forceScanWholeMap, ref Pawn __result)
        {
            if (predator.meleeVerbs.TryGetMeleeVerb(null) == null)
            {
                __result = null;
            }
            bool flag = false;
            if (predator.health.summaryHealth.SummaryHealthPercent < 0.25f)
            {
                flag = true;
            }
            List<Pawn> tempList = (List<Pawn>)typeof(FoodUtility).GetField("tmpPredatorCandidates", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(null);

            if (GetMaxRegionsToScan(predator, forceScanWholeMap) < 0)
            {
                tempList.AddRange(predator.Map.mapPawns.AllPawnsSpawned);
            }
            else
            {
                TraverseParms traverseParms = TraverseParms.For(predator);
                RegionTraverser.BreadthFirstTraverse(predator.Position, predator.Map, (Region from, Region to) => to.Allows(traverseParms, isDestination: true), delegate (Region x)
                {
                    List<Thing> list = x.ListerThings.ThingsInGroup(ThingRequestGroup.Pawn);
                    for (int j = 0; j < list.Count; j++)
                    {
                        tempList.Add((Pawn)list[j]);
                    }
                    return false;
                });
            }
            Pawn pawn = null;
            float num = 0f;
            bool tutorialMode = TutorSystem.TutorialMode;
            if (predHuntMoreColonists)
            {
                PLog.M("Predators hunt colonists more often option is ENABLED");
            }
            else
            {
                PLog.M("Predators hunt colonists more often option is DISABLED");
            }
            for (int i = 0; i < tempList.Count; i++)
            {
                Pawn pawn2 = tempList[i];
                if ( predator.GetDistrict() == pawn2.GetDistrict() && predator != pawn2 && (!flag || pawn2.Downed) && FoodUtility.IsAcceptablePreyFor(predator, pawn2) && predator.CanReach(pawn2, PathEndMode.ClosestTouch, Danger.Deadly) && !pawn2.IsForbidden(predator) && (!tutorialMode || pawn2.Faction != Faction.OfPlayer))
                {
                    float preyScoreFor = FoodUtility.GetPreyScoreFor(predator, pawn2);
                    if(predator.def.defName=="AP_Titanboa"&&pawn2.RaceProps.Humanlike)
                    {
                        preyScoreFor += 25f;
                    }
                    if(predHuntMoreColonists==true&& pawn2.RaceProps.Humanlike&&predator.def.defName!="AP_Titanboa")
                    {
                        preyScoreFor += 25f;
                    }
                    else if(!predHuntMoreColonists&& pawn2.RaceProps.Humanlike)
                    {
                        preyScoreFor -= 35f;
                    }
                    if (preyScoreFor > num || pawn == null)
                    {
                        num = preyScoreFor;
                        pawn = pawn2;
                    }
                }
            }
            tempList.Clear();
            __result = pawn;


        }
    }
}
