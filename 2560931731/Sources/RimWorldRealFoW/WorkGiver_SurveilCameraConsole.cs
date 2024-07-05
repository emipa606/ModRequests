using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimWorldRealFoW
{
	public class WorkGiver_SurveilCameraConsole : WorkGiver_Scanner
	{

		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.InteractionCell;
			}
		}

		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			return pawn.Map.listerBuildings.AllBuildingsColonistOfDef(FoWDef.CameraConsole).Cast<Thing>();
		}

		public override ThingRequest PotentialWorkThingRequest
		{
			get
			{
				return ThingRequest.ForDef(FoWDef.CameraConsole);
			}
		}

		public override Danger MaxPathDanger(Pawn pawn)
		{
			return Danger.Deadly;
		}

		public override bool ShouldSkip(Pawn pawn, bool forced = false)
		{
			return !pawn.Map.listerBuildings.AllBuildingsColonistOfDef(FoWDef.CameraConsole).OfType<Building_CameraConsole>().Any((Building_CameraConsole x) => x.WorkingNow);
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (!(t as Building_CameraConsole).NeedWatcher())
			{
				return false;
			}
			LocalTargetInfo target = t;
			return pawn.CanReserve(target, 1, -1, null, forced);
		}

		// Token: 0x060001F4 RID: 500 RVA: 0x0000CE74 File Offset: 0x0000B074
		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			return new Job(FoWDef.SurveilCameraConsole, t, 1500, true);
		}
	}
}
