using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimTrust.Core.Interactive
{
	public class WorkGiver_FillNutrientTube : WorkGiver_Scanner
	{
		public override ThingRequest PotentialWorkThingRequest
		{
			get
			{
				return ThingRequest.ForDef(CoreDefOf.NutrientTube);
			}
		}

		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.Touch;
			}
		}

		public static void ResetStaticData()
		{
			WorkGiver_FillNutrientTube.TemperatureTrans = "BadTemperature".Translate().ToLower();
			WorkGiver_FillNutrientTube.NoWortTrans = "NoNutrients".Translate();
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Building_NutrientTube building_NutrientTube = t as Building_NutrientTube;
			if (building_NutrientTube == null || building_NutrientTube.Produced || building_NutrientTube.SpaceLeftForNutrient <= 0)
			{
				return false;
			}
			float ambientTemperature = building_NutrientTube.AmbientTemperature;
			CompProperties_TemperatureRuinable compProperties = building_NutrientTube.def.GetCompProperties<CompProperties_TemperatureRuinable>();
			if (ambientTemperature < compProperties.minSafeTemperature + 2f || ambientTemperature > compProperties.maxSafeTemperature - 2f)
			{
				JobFailReason.Is(WorkGiver_FillNutrientTube.TemperatureTrans, null);
				return false;
			}
			if (t.IsForbidden(pawn) || !pawn.CanReserve(t, 1, -1, null, forced))
			{
				return false;
			}
			if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null)
			{
				return false;
			}
			if (this.FindNutrient(pawn, building_NutrientTube) == null)
			{
				JobFailReason.Is(WorkGiver_FillNutrientTube.NoWortTrans, null);
				return false;
			}
			return !t.IsBurning();
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Building_NutrientTube tube = (Building_NutrientTube)t;
			Thing t2 = this.FindNutrient(pawn, tube);
			return JobMaker.MakeJob(CoreDefOf.FillNutrientTube, t, t2);
		}

		private Thing FindNutrient(Pawn pawn, Building_NutrientTube tube)
		{
			Predicate<Thing> validator = (Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x, 1, -1, null, false);

			ThingDef Nutrients = ThingDefOf.RawPotatoes;

			if (GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(ThingDefOf.RawPotatoes), PathEndMode.ClosestTouch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator, null, 0, -1, false, RegionType.Set_Passable, false) != null)
			{
				Nutrients = ThingDefOf.RawPotatoes;
			}
			else if (GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(CoreDefOf.RawRice), PathEndMode.ClosestTouch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator, null, 0, -1, false, RegionType.Set_Passable, false) != null)
			{
				Nutrients = CoreDefOf.RawRice;
			}
			else
			{
				Nutrients = CoreDefOf.RawCorn;
			}

			return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(Nutrients), PathEndMode.ClosestTouch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator, null, 0, -1, false, RegionType.Set_Passable, false);
			
		}

		private static string TemperatureTrans;

		private static string NoWortTrans;
	}
}

