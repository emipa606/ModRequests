using Analyst;
using RimWorld;
using Verse;
using Verse.AI;

public class WorkGiver_MechResearcher : WorkGiver_Scanner
{
	public override ThingRequest PotentialWorkThingRequest
	{
		get
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			if (Find.ResearchManager.currentProj == null)
			{
				return ThingRequest.ForGroup((ThingRequestGroup)1);
			}
			return ThingRequest.ForGroup((ThingRequestGroup)42);
		}
	}

	public override bool Prioritized => true;

	public override bool ShouldSkip(Pawn pawn, bool forced = false)
	{
		if (Find.ResearchManager.currentProj == null || !pawn.IsColonyMech)
		{
			return true;
		}
		if (((Thing)pawn).def != AnalystThingDefOf.Mech_Analyst)
		{
			return true;
		}
		return false;
	}

	public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		ResearchProjectDef currentProj = Find.ResearchManager.currentProj;
		if (currentProj == null)
		{
			return false;
		}
		Building_ResearchBench val = (Building_ResearchBench)(object)((t is Building_ResearchBench) ? t : null);
		if (val == null)
		{
			return false;
		}
		if (!currentProj.CanBeResearchedAt(val, false))
		{
			return false;
		}
		if (!ReservationUtility.CanReserve(pawn, LocalTargetInfo.op_Implicit(t), 1, -1, (ReservationLayerDef)null, forced) || (t.def.hasInteractionCell && !ReservationUtility.CanReserveSittableOrSpot(pawn, t.InteractionCell, forced)))
		{
			return false;
		}
		if (!IdeoUtility.Notify_PawnAboutToDo_Job(new HistoryEvent(HistoryEventDefOf.Researching, NamedArgumentUtility.Named((object)pawn, HistoryEventArgsNames.Doer))))
		{
			return false;
		}
		return true;
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return JobMaker.MakeJob(AnalystJobDefOf.MechResearch, LocalTargetInfo.op_Implicit(t));
	}

	public override float GetPriority(Pawn pawn, TargetInfo t)
	{
		return StatExtension.GetStatValue(((TargetInfo)(ref t)).Thing, StatDefOf.ResearchSpeedFactor, true, -1);
	}
}