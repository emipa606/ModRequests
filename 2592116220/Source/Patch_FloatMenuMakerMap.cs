using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace BuriedAlive
{
    [HarmonyPatch(typeof(FloatMenuMakerMap))]
    [HarmonyPatch("AddHumanlikeOrders")]
    public static class Patch_FloatMenuMakerMap_AddHumanlikeOrders
	{
        public static void Postfix(ref Vector3 clickPos, ref Pawn pawn, ref List<FloatMenuOption> opts) 
        {
			foreach (LocalTargetInfo localTargetInfo in GenUI.TargetsAt(clickPos, TargetingParameters.ForRescue(pawn), true, null))
			{
				Pawn victim = (Pawn)localTargetInfo.Thing;
				if (victim.Downed && pawn.CanReserveAndReach(victim, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, true) && GraveTool.FindGraveFor(victim, pawn, true) != null)
				{
					string text = "BuriedAlive_BuryAlive".Translate(localTargetInfo.Thing.LabelCap, localTargetInfo.Thing);
					JobDef jDef = JobDefOf.BuryAlive;
					Pawn burier = pawn;
					Action action = delegate ()
					{
						Building_Grave grave = GraveTool.FindGraveFor(victim, burier, false);
						if (grave == null)
						{
							grave = GraveTool.FindGraveFor(victim, burier, true);
						}
						if (grave == null)
						{
							Messages.Message("BuriedAlive_CannotBuryAlive".Translate() + ": " + "BuriedAlive_NoGrave".Translate(), victim, MessageTypeDefOf.RejectInput, false);
							return;
						}
						Job job = JobMaker.MakeJob(jDef, victim, grave);
						job.count = 1;
						burier.jobs.TryTakeOrderedJob(job, new JobTag?(JobTag.Misc), false);
					};
					opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text, action, MenuOptionPriority.Default, null, victim, 0f, null, null, true, 0), pawn, victim, "ReservedBy"));
				}
			}
		}
    }
}
