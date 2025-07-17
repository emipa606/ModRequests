using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace THVanillaPatches.Patches
{
	public class TransferPrisonersPatch(THPatchDef def): ToggleablePatchParent(def)
	{
		protected override List<PatchInfo> Patches => new List<PatchInfo>
		{
			new PatchInfo(AccessTools.Method(typeof(FloatMenuMakerMap), "AddHumanlikeOrders"),
				Postfix: new HarmonyMethod(GetType(), nameof(TransferPrisonerMenuOption)))
		};
		
		private static TargetingParameters Target =>
			new()
			{
				canTargetItems = false,
				canTargetBuildings = false,
				canTargetPawns = true,
				onlyTargetColonistsOrPrisonersOrSlavesAllowMinorMentalBreaks = true
			};

		public static TargetingParameters TargetBed(Pawn prisoner, Pawn carrier) => new TargetingParameters()
		{
			canTargetPawns = false,
			canTargetItems = false,
			canTargetBuildings = true,
			validator = target =>
				target.HasThing &&
				RestUtility.IsValidBedFor(target.Thing, prisoner, carrier, false, false, true, GuestStatus.Prisoner)
		};
		
		private static void TransferPrisonerMenuOption(Vector3 clickPos, Pawn pawn, ref List<FloatMenuOption> opts)
		{
            if (pawn.jobs == null) return;
            
            FloatMenuOption option = null;


            foreach (LocalTargetInfo possiblePrisoner in GenUI.TargetsAt(clickPos, Target))
            {
	            //if (!possiblePrisoner.HasThing) continue;
            
	            if (!possiblePrisoner.Pawn.IsPrisonerOfColony) continue;

	            if (pawn.WorkTypeIsDisabled(WorkTypeDefOf.Warden))
	            {
		            option = new FloatMenuOption("THVP.CannotTransferWarden".Translate(), null);
		            continue;
	            }
	            if (!pawn.CanReach(possiblePrisoner, PathEndMode.ClosestTouch, Danger.Deadly))
	            {
		            option = new FloatMenuOption("THVP.CannotTransfer".Translate("NoPath".Translate()), null);
		            continue;
	            }
            
            
            
	            // Add menu option to transfer prisoner.
	            option = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("THVP.TransferPrisoner".Translate(possiblePrisoner.Pawn.Name.ToStringShort), delegate
	            {
		            Find.Targeter.BeginTargeting(TargetBed(possiblePrisoner.Pawn, pawn), targetBed => {
			            pawn.jobs.TryTakeOrderedJob(new Job(THVanillaPatchesDefsOf.THVP_TransferPrisoner, targetBed, possiblePrisoner));
		            });
	            }, MenuOptionPriority.High), pawn, possiblePrisoner);
            }
            
            
            if (option != null) opts.Add(option);
		}
	}
}