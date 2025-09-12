using System;
using System.Runtime.CompilerServices;
using RimWorld;
using Verse;

namespace Necro
{
	
	public class IncidentWorker_NeconoidBiomass : IncidentWorker
	{
		
		
		protected virtual int CountToSpawn
		{
			get
			{
				return 6;
			}
		}

		
		protected override bool CanFireNowSub(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			return true;
		}

		
		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			int countToSpawn = this.CountToSpawn;
			IntVec3 cell = IntVec3.Invalid;
			Predicate<IntVec3> validator = delegate(IntVec3 c)
			{
				if (!c.Fogged(map))
				{
					foreach (IntVec3 c2 in GenAdj.CellsOccupiedBy(c, Rot4.North, ThingDefOf.Necro_InfestedBiomass.size))
					{
						if (!c2.Standable(map))
						{
							return false;
						}
						if (map.roofGrid.Roofed(c2))
						{
							return false;
						}
					}
					return map.reachability.CanReachColony(c);
				}
				return false;
			};
			IntVec3 intVec;
			bool result;
			if (!CellFinderLoose.TryFindRandomNotEdgeCellWith(14, validator, map, out intVec))
			{
				result = false;
			}
			else
			{
				GenSpawn.Spawn(ThingDefOf.Necro_InfestedBiomass, intVec, map, WipeMode.Vanish).SetFaction(Find.FactionManager.FirstFactionOfDef(FactionDefOf.Necronoid), null);
				cell = intVec;
				Find.LetterStack.ReceiveLetter(this.def.letterLabel, this.def.letterText, this.def.letterDef, new TargetInfo(cell, map, false), null, null, null, null, 0, true);
				result = true;
			}
			return result;
		}

		
		public IncidentWorker_NeconoidBiomass()
		{
		}

		
		private const int IncidentMinimumPoints = 500;
	}
}
