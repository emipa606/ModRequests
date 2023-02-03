using RimWorld;
using Verse;
using Verse.AI;

namespace Diabetes
{
	public class WorkGiver_TendHypoglycaemia : WorkGiver_TendBloodSugar
	{
		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Pawn pawn2 = (Pawn) t;
			if (!base.HasJobOnThing(pawn, t, forced))
			{
				return false;
			}
			Hediff hediff = pawn2.health.hediffSet.GetFirstHediffOfDef(TypeGetter.HediffDef(EHediffDef.Hypoglycaemia));
			if (hediff == null)
			{
				return false;
			}
			if (!hediff.CurStage.becomeVisible)
			{
				return false;
			}
			Thing thing;
			if (!FoodUtility.TryFindBestFoodSourceFor(pawn, pawn2, true, out thing, out _))
			{
				if (thing == null)
				{
					JobFailReason.Is("NoFood".Translate());
				}
				return false;
			}
			return true;
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Pawn pawn2 = (Pawn) t;
			Thing food;
			ThingDef foodDef;
			if (FoodUtility.TryFindBestFoodSourceFor(pawn, pawn2, true, out food, out foodDef))
			{
				return null;
			}
			if (food == null || foodDef == null || food.def != foodDef)
			{
				return null;
			}
			Hediff hediff = pawn2.health.hediffSet.GetFirstHediffOfDef(TypeGetter.HediffDef(EHediffDef.Hyperglycaemia));
			if (hediff == null)
			{
				return null;
			}
			int wantedCount = FoodUtility.StackCountForNutrition(hediff.Severity, foodDef.ingestible.CachedNutrition);
			Job job = JobMaker.MakeJob(JobDefOf.FeedPatient);
			job.targetA = food;
			job.targetB = pawn2;
			job.count = FoodUtility.GetMaxAmountToPickup(food, pawn2, wantedCount);
			return job;
		}
	}
}
