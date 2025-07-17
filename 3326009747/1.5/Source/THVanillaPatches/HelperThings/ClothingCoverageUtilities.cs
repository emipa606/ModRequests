using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace THVanillaPatches.HelperThings
{
	public static class ClothingCoverageUtilities
	{
		private static BodyPartGroupDef[] _checkedParts = { BodyPartGroupDefOf.Torso, BodyPartGroupDefOf.FullHead, BodyPartGroupDefOf.Legs };
		
		public static bool IsCoveredByClothing(this Pawn pawn)
		{
			return _checkedParts.All(bodyPartGroup => pawn.apparel.BodyPartGroupIsCovered(bodyPartGroup));
		}
		
		public static bool IsCoveredBySealedClothing(this Pawn pawn)
		{
			List<BodyPartGroupDef> partsToCheck = _checkedParts.ToList();
			
			if (pawn.apparel.WornApparel == null) return false;
			
			foreach (var apparelBodyPartGroup in pawn.apparel.WornApparel.Where(apparel => apparel.def?.techLevel >= TechLevel.Spacer).SelectMany(apparel => apparel.def?.apparel?.bodyPartGroups))
			{
				partsToCheck.Remove(apparelBodyPartGroup);
			}

			return !partsToCheck.Any();
		}
	}
}