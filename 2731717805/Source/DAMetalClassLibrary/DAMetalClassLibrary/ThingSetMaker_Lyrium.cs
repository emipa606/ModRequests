using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace dragonagemetals
{
	public class ThingSetMaker_Lyrium : ThingSetMaker
	{
		public static void Reset()
		{
			ThingSetMaker_Lyrium.nonSmoothedMineables.Clear();
			ThingSetMaker_Lyrium.nonSmoothedMineables.AddRange(from x in DefDatabase<ThingDef>.AllDefsListForReading
															   where x.mineable && x != ThingDefOf.CollapsedRocks && x != ThingDefOf.RaisedRocks && !x.IsSmoothed
			select x);
		}

		protected override void Generate(ThingSetMakerParams parms, List<Thing> outThings)
		{
			int randomInRange = (parms.countRange ?? ThingSetMaker_Lyrium.MineablesCountRange).RandomInRange;
			ThingDef def = this.FindRandomMineableDef();
			for (int i = 0; i < randomInRange; i++)
			{
				Building building = (Building)ThingMaker.MakeThing(def, null);
				building.canChangeTerrainOnDestroyed = false;
				outThings.Add(building);
			}
		}

		private ThingDef FindRandomMineableDef()
		{
			return ThingDef.Named("MineableLyrium");
		}

		protected override IEnumerable<ThingDef> AllGeneratableThingsDebugSub(ThingSetMakerParams parms)
		{
			return ThingSetMaker_Lyrium.nonSmoothedMineables;
		}

		public static List<ThingDef> nonSmoothedMineables = new List<ThingDef>();

		public static readonly IntRange MineablesCountRange = new IntRange(8, 20);

		private const float PreciousMineableMarketValue = 5f;
	}
}
