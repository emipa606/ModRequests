using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace zed_0xff.CPS;

public class JobDriver_TakeToTSS : JobDriver
{
	private const TargetIndex TakeeIndex = TargetIndex.A;

	private const TargetIndex BedIndex = TargetIndex.B;

	protected Pawn Takee => (Pawn)job.GetTarget(TargetIndex.A).Thing;

	protected Building_TSS DropTSS => (Building_TSS)job.GetTarget(TargetIndex.B).Thing;

	private bool TakeeRescued
	{
		get
		{
			if (Takee.RaceProps.Humanlike && job.def != VDefOf.ArrestToTSS && !Takee.IsPrisonerOfColony)
			{
				if (Takee.ageTracker.CurLifeStage.alwaysDowned)
				{
					return HealthAIUtility.ShouldSeekMedicalRest(Takee);
				}
				return true;
			}
			return false;
		}
	}

	public override string GetReport()
	{
		if (job.def == JobDefOf.Rescue && !TakeeRescued)
		{
			return "TakingToBed".Translate(Takee);
		}
		return base.GetReport();
	}

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		if (DropTSS.CanAcceptPawn(Takee, forcePrisoner: true)) {
            DropTSS.SelectPawn2(Takee, makeJob: false);
            return true;
		}
		return false;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDestroyedOrNull(TargetIndex.A);
		this.FailOnDestroyedOrNull(TargetIndex.B);
		this.FailOnAggroMentalStateAndHostile(TargetIndex.A);
		this.FailOn(delegate {
			if (job.def.makeTargetPrisoner) {
				if (!DropTSS.ForPrisoners) {
					return true;
				}
			} else if (DropTSS.ForPrisoners != Takee.IsPrisoner) {
				return true;
			}
            return !DropTSS.CanAcceptPawn(Takee, forcePrisoner: true) || !DropTSS.SelectedPawns.Contains(Takee);
            // for proper canceling of a pending job --------------------^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
		});
        yield return Toils_General.Do(delegate
                {
                DropTSS.SelectedPawns.Add(Takee);
                });
		Toil goToTakee = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.A).FailOnDespawnedNullOrForbidden(TargetIndex.B)
			.FailOn(() => job.def == VDefOf.ArrestToTSS && !Takee.CanBeArrestedBy(pawn))
			.FailOn(() => !pawn.CanReach(DropTSS, PathEndMode.InteractionCell, Danger.Deadly))
			.FailOn(() => (job.def == JobDefOf.Rescue || job.def == JobDefOf.Capture) && !Takee.Downed)
			.FailOnSomeonePhysicallyInteracting(TargetIndex.A);
		Toil checkArrestResistance = ToilMaker.MakeToil("MakeNewToils");
		checkArrestResistance.initAction = delegate
		{
			if (job.def.makeTargetPrisoner)
			{
				Pawn pawn = (Pawn)job.targetA.Thing;
				pawn.GetLord()?.Notify_PawnAttemptArrested(pawn);
				GenClamor.DoClamor(pawn, 10f, ClamorDefOf.Harm);
				if (!pawn.IsPrisoner && !pawn.IsSlave)
				{
					QuestUtility.SendQuestTargetSignals(pawn.questTags, "Arrested", pawn.Named("SUBJECT"));
					if (pawn.Faction != null)
					{
						QuestUtility.SendQuestTargetSignals(pawn.Faction.questTags, "FactionMemberArrested", pawn.Faction.Named("FACTION"));
					}
				}
				if (job.def == VDefOf.ArrestToTSS && !pawn.CheckAcceptArrest(base.pawn))
				{
					base.pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
				}
			}
		};
		yield return Toils_Jump.JumpIf(checkArrestResistance, () => pawn.IsCarryingPawn(Takee));
		yield return goToTakee;
		yield return checkArrestResistance;
		Toil startCarrying = Toils_Haul.StartCarryThing(TargetIndex.A);
//		startCarrying.FailOnBedNoLongerUsable(TargetIndex.B, TargetIndex.A);
		startCarrying.AddPreInitAction(CheckMakeTakeeGuest);
		startCarrying.AddFinishAction(delegate
		{
			if (pawn.Faction == Takee.Faction)
			{
				CheckMakeTakeePrisoner();
			}
		});
		Toil goToBed = Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.InteractionCell).FailOn(() => !pawn.IsCarryingPawn(Takee));
//		goToBed.FailOnBedNoLongerUsable(TargetIndex.B, TargetIndex.A);
		yield return Toils_Jump.JumpIf(goToBed, () => pawn.IsCarryingPawn(Takee));
		yield return startCarrying;
		yield return goToBed;
		Toil toil = ToilMaker.MakeToil("MakeNewToils");
		toil.initAction = delegate
		{
			CheckMakeTakeePrisoner();
			if (Takee.playerSettings == null)
			{
				Takee.playerSettings = new Pawn_PlayerSettings(Takee);
			}
		};
		yield return toil;
        yield return Toils_General.WaitWith(TargetIndex.B, 60, useProgressBar: true);
        yield return Toils_General.Do(delegate
                {
                DropTSS.TryAcceptPawn(Takee);
                });
	}

	private void CheckMakeTakeePrisoner()
	{
		if (job.def.makeTargetPrisoner)
		{
			if (Takee.guest.Released)
			{
				Takee.guest.Released = false;
				Takee.guest.interactionMode = PrisonerInteractionModeDefOf.NoInteraction;
				GenGuest.RemoveHealthyPrisonerReleasedThoughts(Takee);
			}
			if (!Takee.IsPrisonerOfColony)
			{
				Takee.guest.CapturedBy(Faction.OfPlayer, pawn);
			}
		}
	}

	private void CheckMakeTakeeGuest()
	{
		if (!job.def.makeTargetPrisoner && Takee.Faction != Faction.OfPlayer && Takee.HostFaction != Faction.OfPlayer && Takee.guest != null && !Takee.IsWildMan())
		{
			Takee.guest.SetGuestStatus(Faction.OfPlayer);
			QuestUtility.SendQuestTargetSignals(Takee.questTags, "Rescued", Takee.Named("SUBJECT"));
		}
	}
}
