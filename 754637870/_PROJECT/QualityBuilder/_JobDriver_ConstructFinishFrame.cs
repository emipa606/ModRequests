using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using RimWorld;
using Verse.AI;
using Verse;
using HarmonyLib;

namespace QualityBuilder
{
    [HarmonyPatch(typeof(JobDriver_ConstructFinishFrame), "MakeNewToils")]
    public class _JobDriver_ConstructFinishFrame
    {
        public static IEnumerable<Toil> Postfix(IEnumerable<Toil> __result, JobDriver_ConstructFinishFrame __instance)
        {
            Thing thing = __instance.job.targetA.Thing;
            if (thing == null)
            {
                return __result;
            }
            Map map = thing.Map;
            LocalTargetInfo localTargetInfo = __instance.job.targetA;
            CompQualityBuilder compQuality = null;
            try
            {
                compQuality = QualityBuilder.getCompQualityBuilder(thing);
            }
            catch (Exception ex)
            {
                Log.Warning("QualityBuilder: cant enhance constuctFinishFrame toil cause thing is not compatible", false);
            }
            if (compQuality == null)
            {
                return __result;
            }
            List<Toil> list = new List<Toil>(__result);
            Toil toil = list.Last<Toil>();
            toil.AddFinishAction(delegate
            {
                _JobDriver_ConstructFinishFrame.afterFinishToil(compQuality, map, localTargetInfo);
            });
            return list;
        }

        private static CompQualityBuilder getComp(Toil toil, ref LocalTargetInfo targetInfo)
        {
            Pawn actor = toil.actor;
            Job curJob = actor.jobs.curJob;
            targetInfo = curJob.targetA;
            ThingWithComps target = targetInfo.Thing as ThingWithComps;
            if (target == null)
            {
                Log.Error("QualityBuilder: Target not available to get QualityBuilder settings");
                return null;
            }
            return QualityBuilder.getCompQualityBuilder(target);
        }

        public static void afterFinishToil(CompQualityBuilder cmp, Map curMap, LocalTargetInfo targetInfo)
        {
            if (cmp == null)
                return;
            String targetDefName = targetInfo.Thing.def.defName;
            if (targetDefName.EndsWith("_ReplaceStuff"))
                targetDefName = targetDefName.Replace("_ReplaceStuff", "");
            Building building = null;
            try
            {
                building = curMap.thingGrid.ThingsListAt(targetInfo.Cell).First(t => QualityBuilder.getCompQualityBuilder(t) != null) as Building;
            }catch(Exception e)
            {
                
            }
            if (building == null) // Possible it got butchered
            {
                ThingWithComps newBP = QualityBuilder.GetFirstBuildingBuildingOrFrame(curMap, targetInfo.Cell) as ThingWithComps;
                if (newBP == null)
                {
                    Log.Message("QualityBuilder: No new BP available");
                    return;
                }
                if (cmp == null)
                {
                    Log.Message("QualityBuilder: No old CompQualityBuilder to set");
                    return;
                }
                CompQualityBuilder newBPCmp = QualityBuilder.getCompQualityBuilder(newBP);
                newBPCmp.desiredMinQuality = cmp.desiredMinQuality;
                QualityBuilder.setSkilled(newBP, cmp.desiredMinQuality, cmp.isSkilled);
                return;
            }
            QualityCategory finishedBuildingQuality;
            if (!building.TryGetQuality(out finishedBuildingQuality))
            {
                Log.Message("QualityBuilder: Could not get quality from thing with defName '" + building.def.defName + "'");
                return;
            }
            CompQualityBuilder buildingCmp = QualityBuilder.getCompQualityBuilder(building);
            buildingCmp.isSkilled = cmp.isSkilled;
            buildingCmp.desiredMinQuality = cmp.desiredMinQuality;
            if (finishedBuildingQuality >= cmp.desiredMinQuality || !cmp.isSkilled)
            {
                buildingCmp.isDesiredMinQualityReached = true;
                return;
            }
            curMap.designationManager.AddDesignation(new Designation(building, DesignationDefOf.Deconstruct));
        }
    }
}
