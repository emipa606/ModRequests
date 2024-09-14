using System;
using System.Collections.Generic;
using Analyst;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

public class JobDriver_MechResearch : JobDriver
{
	private const int JobEndInterval = 4000;

	private ResearchProjectDef Project => Find.ResearchManager.currentProj;

	private Building_ResearchBench ResearchBench => (Building_ResearchBench)((JobDriver)this).TargetThingA;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		if (ReservationUtility.Reserve(base.pawn, LocalTargetInfo.op_Implicit((Thing)(object)ResearchBench), base.job, 1, -1, (ReservationLayerDef)null, errorOnFailed))
		{
			if (((Thing)ResearchBench).def.hasInteractionCell)
			{
				return ReservationUtility.ReserveSittableOrSpot(base.pawn, ((Thing)ResearchBench).InteractionCell, base.job, errorOnFailed);
			}
			return true;
		}
		return false;
	}

	public override IEnumerable<Toil> MakeNewToils()
	{
		ToilFailConditions.FailOnDespawnedNullOrForbidden<JobDriver_MechResearch>(this, (TargetIndex)1);
		yield return Toils_Goto.GotoThing((TargetIndex)1, (PathEndMode)4);
		Toil research = ToilMaker.MakeToil("MakeNewToils");
		research.tickAction = delegate
		{
			Pawn actor = research.actor;
			float statValue = StatExtension.GetStatValue((Thing)(object)actor, StatDefOf.ResearchSpeed, true, -1);
			statValue *= StatExtension.GetStatValue(((JobDriver)this).TargetThingA, StatDefOf.ResearchSpeedFactor, true, -1);
			Find.ResearchManager.ResearchPerformed(statValue, actor);
		};
		ToilFailConditions.FailOn<Toil>(research, (Func<bool>)(() => Project == null));
		ToilFailConditions.FailOn<Toil>(research, (Func<bool>)(() => !Project.CanBeResearchedAt(ResearchBench, false)));
		ToilFailConditions.FailOnCannotTouch<Toil>(research, (TargetIndex)1, (PathEndMode)4);
		ToilEffects.WithEffect(research, EffecterDefOf.Research, (TargetIndex)1, (Color?)null);
		ToilEffects.WithProgressBar(research, (TargetIndex)1, (Func<float>)delegate
		{
			ResearchProjectDef project = Project;
			return (project != null) ? project.ProgressPercent : 0f;
		}, false, -0.5f, false);
		research.defaultCompleteMode = (ToilCompleteMode)3;
		research.defaultDuration = 4000;
		yield return research;
		yield return Toils_General.Wait(2, (TargetIndex)0);
	}
}