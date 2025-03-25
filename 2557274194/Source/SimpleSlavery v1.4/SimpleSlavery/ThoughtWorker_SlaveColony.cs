using System;
using RimWorld;
using Verse;
using System.Linq;

namespace SimpleSlavery
{
	public class ThoughtWorker_SlaveColony : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal (Pawn p)
		{
			Pawn pawn = p;
			if (pawn.story.traits.HasTrait (TraitDef.Named ("Kind")) &&
				pawn.Map.mapPawns.FreeColonistsAndPrisonersSpawned.ToList ().Find (
				   x => SlaveUtility.IsPawnColonySlave (x) && x.Faction == pawn.Faction) != null)
				return ThoughtState.ActiveDefault;
			return ThoughtState.Inactive;
		}
	}
}
