using System.Collections.Generic;
using Analyst;
using RimWorld;
using Verse;
using Verse.AI;

public class WorkGiver_MechOperateScanner : WorkGiver_Scanner
{
	public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(ScannerDef);

	public ThingDef ScannerDef => ((WorkGiver)this).def.scannerDef;

	public override PathEndMode PathEndMode => (PathEndMode)4;

	public override Danger MaxPathDanger(Pawn pawn)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		return (Danger)3;
	}

	public override bool ShouldSkip(Pawn pawn, bool forced = false)
	{
		if (!pawn.IsColonyMech)
		{
			return true;
		}
		if (((Thing)pawn).def != AnalystThingDefOf.Mech_Analyst)
		{
			return true;
		}
		List<Thing> list = ((Thing)pawn).Map.listerThings.ThingsOfDef(ScannerDef);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].Faction == ((Thing)pawn).Faction)
			{
				CompScanner val = ThingCompUtility.TryGetComp<CompScanner>(list[i]);
				if (val != null && val.CanUseNow)
				{
					return false;
				}
			}
		}
		return true;
	}

	public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		if (t.Faction != ((Thing)pawn).Faction)
		{
			return false;
		}
		Building val = (Building)(object)((t is Building) ? t : null);
		if (val == null)
		{
			return false;
		}
		if (ForbidUtility.IsForbidden((Thing)(object)val, pawn))
		{
			return false;
		}
		if (!ReservationUtility.CanReserve(pawn, LocalTargetInfo.op_Implicit((Thing)(object)val), 1, -1, (ReservationLayerDef)null, forced))
		{
			return false;
		}
		if (!ThingCompUtility.TryGetComp<CompScanner>((Thing)(object)val).CanUseNow)
		{
			return false;
		}
		if (FireUtility.IsBurning((Thing)(object)val))
		{
			return false;
		}
		return true;
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return JobMaker.MakeJob(AnalystJobDefOf.MechOperateScanner, LocalTargetInfo.op_Implicit(t), 1500, true);
	}
}