using Verse;
using Verse.AI;

namespace Diabetes
{
	public class WorkGiver_TendHyperglycaemia : WorkGiver_TendBloodSugar
	{
		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Pawn pawn2 = (Pawn) t;
			if (!base.HasJobOnThing(pawn, t, forced))
			{
				return false;
			}
			if (pawn2.health.hediffSet.HasHediff(TypeGetter.HediffDef(EHediffDef.InsulinHigh)))
			{
				return false;
			}
			Hediff hediff = pawn2.health.hediffSet.GetFirstHediffOfDef(TypeGetter.HediffDef(EHediffDef.Hyperglycaemia));
			if (hediff == null)
			{
				return false;
			}
			if (!hediff.CurStage.becomeVisible)
			{
				return false;
			}
			Thing thing = DiabetesUtility.FindNextInsulin(pawn);
			if (thing == null)
			{
				JobFailReason.Is("NoInsulinFloat".Translate());
				return false;
			}
			return true;
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Pawn pawn2 = (Pawn) t;
			Thing insulin = DiabetesUtility.FindNextInsulin(pawn);
			Hediff hediff = pawn2.health.hediffSet.GetFirstHediffOfDef(TypeGetter.HediffDef(EHediffDef.Hyperglycaemia));
			if (hediff == null)
			{
				return null;
			}
			if (insulin == null)
			{
				return null;
			}

			Job job = JobMaker.MakeJob(TypeGetter.JobDef(EJobDef.AdministerDrug));
			job.targetA = insulin;
			job.targetB = pawn2;
			job.count = DiabetesUtility.NumToIngest(insulin, hediff, DiabetesUtility.InsulinValue);
			return job;
		}
	}
}
