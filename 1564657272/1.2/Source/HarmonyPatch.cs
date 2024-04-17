﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;
using UnityEngine;
using HarmonyLib;
using RimWorld.Planet;

namespace BioReactor
{
    [StaticConstructorOnStartup]
    public static class BioReactorPatches
    {
        private static readonly Type patchType = typeof(BioReactorPatches);

        
        static BioReactorPatches()
        {
            Harmony harmonyInstance = new Harmony("com.BioReactor.rimworld.mod");
            harmonyInstance.Patch(
                original: AccessTools.Method(typeof(FloatMenuMakerMap), "AddHumanlikeOrders"),
                prefix: new HarmonyMethod(patchType, "Prefix_AddHumanlikeOrders", null));
            harmonyInstance.Patch(
                original: AccessTools.Method(typeof(ThingOwnerUtility), "ContentsSuspended"),
                prefix: new HarmonyMethod(patchType, "Prefix_ContentsSuspended", null));
        }

        public static bool Prefix_AddHumanlikeOrders(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
        {
            if (pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
            {
                foreach (LocalTargetInfo localTargetInfo3 in GenUI.TargetsAt(clickPos, TargetingParameters.ForRescue(pawn), true))
                {
                    LocalTargetInfo localTargetInfo4 = localTargetInfo3;
                    Pawn victim = (Pawn)localTargetInfo4.Thing;
                    if (victim.Downed && pawn.CanReserveAndReach(victim, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, true) && Building_BioReactor.FindBioReactorFor(victim, pawn, true) != null)
                    {
                        string text4 = "CarryToBioReactor".Translate(localTargetInfo4.Thing.LabelCap, localTargetInfo4.Thing);
                        JobDef jDef = Bio_JobDefOf.CarryToBioReactor;
                        Action action3 = delegate ()
                        {
                            Building_BioReactor building_BioReactor = Building_BioReactor.FindBioReactorFor(victim, pawn, false);
                            if (building_BioReactor == null)
                            {
                                building_BioReactor = Building_BioReactor.FindBioReactorFor(victim, pawn, true);
                            }
                            if (building_BioReactor == null)
                            {
                                Messages.Message("CannotCarryToBioReactor".Translate() + ": " + "NoBioReactor".Translate(), victim, MessageTypeDefOf.RejectInput, false);
                                return;
                            }
                            Job job = new Job(jDef, victim, building_BioReactor);
                            job.count = 1;
                            pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                        };
                        string label = text4;
                        Action action2 = action3;
                        Pawn revalidateClickTarget = victim;
                        opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, action2, MenuOptionPriority.Default, null, revalidateClickTarget, 0f, null, null), pawn, victim, "ReservedBy"));
                    }
                }
            }
            return true;
        }
        public static bool Prefix_ContentsSuspended(ref bool __result, IThingHolder holder)
        {
            while (holder != null)
            {
                if (holder is Building_BioReactor || holder is Building_CryptosleepCasket || holder is ImportantPawnComp)
                {
                    __result = true;
                    return false;
                }
                holder = holder.ParentHolder;
            }
            __result = false;
            return false;
        }
    }
}