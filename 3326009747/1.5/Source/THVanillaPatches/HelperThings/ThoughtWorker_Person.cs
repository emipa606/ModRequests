using RimWorld;
using Verse;

namespace THVanillaPatches.HelperThings
{
	public class ThoughtWorker_Person: ThoughtWorker
	{
		protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn other)
		{
			if (!p.RaceProps.Humanlike)
				return false;
			if (!p.story.traits.HasTrait(THVanillaPatchesDefsOf.THVP_DislikesPeople))
				return false;
			if (!RelationsUtility.PawnsKnowEachOther(p, other))
				return false;
			return other.def == p.def;
		}
	}
}