using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using Verse.AI;
using Verse;
using HarmonyLib;

namespace QualityBuilder
{
    [HarmonyPatch(typeof(JobDriver_Deconstruct), "MakeNewToils")]
    public class _JobDriver_Deconstruct
    {
        public static IEnumerable<Toil> Postfix(IEnumerable<Toil> __result, JobDriver_Deconstruct __instance)
        {
            List<Toil> toils = new List<Toil>(__result);
            LocalTargetInfo target = __instance.job.targetA;
            Thing thing = target.Thing;
            CompQualityBuilder cmp = QualityBuilder.getCompQualityBuilder(thing);
            IntVec3 center = target.Cell;
            Rot4 rotation = thing.Rotation;
            ThingDef stuffDef = thing.Stuff;
            ThingDef buildingDef = thing.def;
            for (int i = 0; i < toils.Count; i++)
            {
                Toil cur = toils[i];
                if (i + 1 == toils.Count) // last
                {
                    cur.AddFinishAction(new Action(delegate { afterFinishToil(cur, cmp, center, rotation, stuffDef, buildingDef); }));
                }
                yield return cur;
            }

        }

        private static  void afterFinishToil(Toil cur, CompQualityBuilder cmp, IntVec3 center, Rot4 rotation, ThingDef stuffDef, ThingDef buildingDef)
        {
            if (cmp == null)
                return;
            if (!cur.actor.Faction.IsPlayer || cmp.isDesiredMinQualityReached || !cmp.isSkilled)
                return;
            Blueprint newBP = GenConstruct.PlaceBlueprintForBuild(buildingDef, center, cur.actor.Map, rotation, Faction.OfPlayer, stuffDef);
            CompQualityBuilder newBPCmp = QualityBuilder.getCompQualityBuilder(newBP);
            if (newBPCmp == null)
            {
                Log.Error("New BP has no compQualityBuilder");
                return;
            }
            newBPCmp.desiredMinQuality = cmp.desiredMinQuality;
            QualityBuilder.setSkilled(newBP, cmp.desiredMinQuality, cmp.isSkilled);
        }
    }
}
