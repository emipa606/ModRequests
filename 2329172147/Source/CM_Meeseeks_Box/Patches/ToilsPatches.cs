using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace CM_Meeseeks_Box
{
    [StaticConstructorOnStartup]
    public static class ToilsPatches
    {
        [HarmonyPatch(typeof(Toils_JobTransforms))]
        [HarmonyPatch("ClearDespawnedNullOrForbiddenQueuedTargets", MethodType.Normal)]
        public class Toils_JobTransforms_ClearDespawnedNullOrForbiddenQueuedTargets
        {
            [HarmonyPostfix]
            public static void Postfix(ref Toil __result, TargetIndex ind, Func<Thing, bool> validator = null)
            {
                if (__result != null)
                {
                    Toil toil = new Toil();
                    toil.initAction = delegate
                    {
                        Pawn actor = toil.actor;

                        // If this is a Meeseeks:  screw the validator, vanilla only uses that to block undesignated items in a way that circumvents job.ignoreDesignations
                        if (actor.GetComp<CompMeeseeksMemory>() != null)
                        {
                            actor.jobs.curJob.GetTargetQueue(ind).RemoveAll((LocalTargetInfo ta) => !ta.HasThing || !ta.Thing.Spawned);
                        }
                        else
                        {
                            actor.jobs.curJob.GetTargetQueue(ind).RemoveAll((LocalTargetInfo ta) => !ta.HasThing || !ta.Thing.Spawned || ta.Thing.IsForbidden(actor) || (validator != null && !validator(ta.Thing)));
                        }
                    };
                    __result = toil;
                }
            }
        }
    }
}
