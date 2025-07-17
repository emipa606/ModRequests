using RimWorld;
using Verse;

namespace THVanillaPatches.HelperThings
{
	public class Thought_SoakingWet: Thought_Memory
	{
		public override bool ShouldDiscard => base.ShouldDiscard || IsCovered();

		private bool IsCovered()
		{
			if (!THVanillaPatchesDefsOf.THVP_ApparelCoveragePatch.IsEnabled) return false;
			return pawn.IsCoveredBySealedClothing();
		}
	}
}