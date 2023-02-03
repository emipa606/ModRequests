using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Diabetes
{
	public class JobGiver_RegulateBloodSugar : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			if (!DiabetesUtility.HasIrregularBloodSugar(pawn))
			{
				return null;
			}
			Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(TypeGetter.HediffDef(EHediffDef.Hypoglycaemia));
			Thing thing;
			float value;
			JobDef jobDef;
			if (hediff == null)
			{
				hediff = pawn.health.hediffSet.GetFirstHediffOfDef(TypeGetter.HediffDef(EHediffDef.Hyperglycaemia));
				thing = DiabetesUtility.FindNextInsulin(pawn);
				value = DiabetesUtility.InsulinValue;
				jobDef = TypeGetter.JobDef(EJobDef.LowerBloodSugar);
			}
			else
			{
				ThingDef thingDef;
				FoodUtility.TryFindBestFoodSourceFor(pawn, pawn, true, out thing, out thingDef);
				if (thing.def != thingDef)
				{
					return null;
				}
				value = thingDef.ingestible.CachedNutrition * Nutrition_Patch.randomFactor.Average;
				jobDef = JobDefOf.Ingest;
			}
			if (!hediff.CurStage.becomeVisible)
			{
				return null;
			}
			if (thing == null)
			{
				return null;
			}
			Job job = JobMaker.MakeJob(jobDef);
			job.targetA = thing;
			job.count = DiabetesUtility.NumToIngest(thing, hediff, value);
			return job;
		}

		public override float GetPriority(Pawn pawn)
		{
			if (!DiabetesUtility.HasIrregularBloodSugar(pawn))
			{
				return 0f;
			}
			Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(TypeGetter.HediffDef(EHediffDef.Hypoglycaemia));
			if (hediff == null)
			{
				hediff = pawn.health.hediffSet.GetFirstHediffOfDef(TypeGetter.HediffDef(EHediffDef.Hyperglycaemia));
			}
			if (!hediff.CurStage.becomeVisible)
			{
				return 0f;
			}
			return Mathf.Lerp(1.0f, 10f, hediff.Severity / 0.8f);
		}
	}
}
