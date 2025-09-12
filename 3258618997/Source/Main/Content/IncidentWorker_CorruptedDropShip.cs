using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace Necro
{
	
	public class IncidentWorker_CorruptedDropShip : IncidentWorker
	{
		
		protected override bool CanFireNowSub(IncidentParms parms)
		{
			return ((Map)parms.target).listerThings.ThingsOfDef(this.def.mechClusterBuilding).Count <= 0;
		}

		
		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			List<TargetInfo> list = new List<TargetInfo>();
			ThingDef mechClusterBuilding = this.def.mechClusterBuilding;
			IntVec3 intVec;
			CellFinderLoose.TryFindSkyfallerCell(ThingDefOf.CorruptedDropShipLanding, map, out intVec, 14, default(IntVec3), -1, false, true, true, true, true, false, null);
			Thing thing = ThingMaker.MakeThing(ThingDefOf.CorruptedDropShip, null);
			bool result;
			if (intVec == IntVec3.Invalid)
			{
				result = false;
			}
			else
			{
				float points = Mathf.Max(parms.points * 0.9f, 400f);
				List<Pawn> list2 = PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
				{
					groupKind = PawnGroupKindDefOf.Combat,
					tile = map.Tile,
					faction = Find.FactionManager.FirstFactionOfDef(FactionDefOf.Necronoid),
					points = points
				}, true).ToList<Pawn>();
				thing.SetFaction(Find.FactionManager.FirstFactionOfDef(FactionDefOf.Necronoid), null);
				LordMaker.MakeNewLord(Find.FactionManager.FirstFactionOfDef(FactionDefOf.Necronoid), new LordJob_MechanoidsDefend(new List<Thing>
				{
					thing
				}, Find.FactionManager.FirstFactionOfDef(FactionDefOf.Necronoid), 28f, intVec, false, false), map, list2);
				DropPodUtility.DropThingsNear(intVec, map, list2.Cast<Thing>(), 110, false, false, true, true, true, null);
				list.AddRange(from p in list2
				select new TargetInfo(p));
				GenSpawn.Spawn(SkyfallerMaker.MakeSkyfaller(ThingDefOf.CorruptedDropShipLanding, thing), intVec, map, WipeMode.Vanish);
				list.Add(new TargetInfo(intVec, map, false));
				base.SendStandardLetter(parms, list, Array.Empty<NamedArgument>());
				result = true;
			}
			return result;
		}

		
		private static IntVec3 FindDropPodLocation(Map map, Predicate<IntVec3> validator)
		{
			for (int i = 0; i < 200; i++)
			{
				IntVec3 intVec = RCellFinder.FindSiegePositionFrom(DropCellFinder.FindRaidDropCenterDistant(map, true), map, true, true, null, true);
				if (validator(intVec))
				{
					return intVec;
				}
			}
			return IntVec3.Invalid;
		}

		
		public IncidentWorker_CorruptedDropShip()
		{
		}

		
		private const float ShipPointsFactor = 0.9f;

		
		private const int IncidentMinimumPoints = 400;

		
		private const float DefendRadius = 75f;
        
	}
}
