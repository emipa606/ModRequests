using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace KB_Go_To_Sleep
{
    [StaticConstructorOnStartup]
    public static class FloatMenuMakerMap_Patches
    {
        [HarmonyPatch(typeof(FloatMenuMakerMap))]
        [HarmonyPatch("AddHumanlikeOrders", MethodType.Normal)]
        public static class FloatMenuMakerMap_AddHumanlikeOrders
        {
            [HarmonyPostfix]
            public static void Postfix(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
            {
                if (pawn.needs == null || pawn.needs.rest == null)
                    return;

                foreach (LocalTargetInfo bed in GenUI.TargetsAt(clickPos, ForSleeping(pawn), thingsOnly: true))
                {
                    if (pawn.needs.rest.CurLevel > FloatMenuMakerMap_AddHumanlikeOrders.FallAsleepMaxLevel(pawn))
                    {
                        opts.Add(new FloatMenuOption("KB_Go_To_Sleep_Cannot_Sleep".Translate() + ": " + "KB_Go_To_Sleep_Not_Tired".Translate().CapitalizeFirst(), null));
                    }
                    else if (!pawn.CanReach(bed, PathEndMode.OnCell, Danger.Deadly))
                    {
                        opts.Add(new FloatMenuOption("KB_Go_To_Sleep_Cannot_Sleep".Translate() + ": " + "NoPath".Translate().CapitalizeFirst(), null));
                    }
                    else
                    {
                        opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("KB_Go_To_Sleep_GoToSleep".Translate(), delegate
                        {
                            Job job = JobMaker.MakeJob(JobDefOf.LayDown, bed.Thing);

                            pawn.jobs.TryTakeOrderedJob(job);
                        }, MenuOptionPriority.High), pawn, bed.Thing));
                    }
                }
            }

            private static float WakeThreshold(Pawn p)
            {
                Lord lord = p.GetLord();
                if (lord != null && lord.CurLordToil != null && lord.CurLordToil.CustomWakeThreshold.HasValue)
                {
                    return lord.CurLordToil.CustomWakeThreshold.Value;
                }
                return p.ageTracker.CurLifeStage?.naturalWakeThresholdOverride ?? 1f;
            }


            private static float FallAsleepMaxLevel(Pawn p)
            {
                return Mathf.Min(p.ageTracker.CurLifeStage?.fallAsleepMaxThresholdOverride ?? 0.75f, WakeThreshold(p) - 0.01f);
            }


            private static TargetingParameters ForSleeping(Pawn sleeper)
            {
                return new TargetingParameters
                {
                    canTargetPawns = false,
                    canTargetBuildings = true,
                    mapObjectTargetsMustBeAutoAttackable = false,
                    validator = delegate (TargetInfo targ)
                    {
                        if (!targ.HasThing)
                        {
                            return false;
                        }
                        Building_Bed bed = targ.Thing as Building_Bed;
                        if (bed == null)
                        {
                            return false;
                        }
                        return (!bed.ForPrisoners && !bed.Medical);
                        //return (bed.AnyUnownedSleepingSlot || bed.CompAssignableToPawn.AssignedPawns.Contains(sleeper));
                    }
                };
            }
        }
    }
}