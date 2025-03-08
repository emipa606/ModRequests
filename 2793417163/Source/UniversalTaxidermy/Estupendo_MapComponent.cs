using System.Collections.Generic;
using Verse;

namespace EstupendoBase
{
	public class Estupendo_MapComponent : MapComponent
	{
		public Dictionary<Thing, CellRect>	PlaqueClaims = new Dictionary<Thing, CellRect>();
		private List<Thing>					ClaimThings;
		private List<CellRect>				ClaimRects;

		public Estupendo_MapComponent(Map map) : base(map)
		{
			HarmonyLib.Harmony.DEBUG = true;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref PlaqueClaims, "Claims", LookMode.Reference, LookMode.Value, ref ClaimThings, ref ClaimRects);
		}

		public bool CanClaimDisplayCells(CellRect rect, Thing claimer)
		{
			foreach( KeyValuePair<Thing, CellRect> pair in PlaqueClaims )
			{
				if( (pair.Key != claimer) && pair.Value.Overlaps(rect) ) return false;
			}
			return true;
		}

		public bool ClaimDisplayCells(CellRect rect, Thing claimer)
		{
			if( !CanClaimDisplayCells(rect, claimer) ) { return false; }
			PlaqueClaims.SetOrAdd(claimer, rect);
			return true;
		}

		public void ReleaseDisplayCells(Thing claimer)
		{
			if( PlaqueClaims.ContainsKey(claimer) ) PlaqueClaims.Remove(claimer);
		}

		public Thing GetCellClaimant(IntVec3 cell)
		{
			foreach( Thing claimant in PlaqueClaims.Keys )
			{
				if( PlaqueClaims.TryGetValue(claimant).Contains(cell) )
				{
					return claimant;
				}
			}
			return null;
		}
	}
}
