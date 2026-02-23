using HarmonyLib;
using RimWorld;
using Verse;

namespace YoungAndOldHair
{
	[HarmonyPatch(typeof(PawnStyleItemChooser), "AgeAppropriateHairStyle")]
	public static class PawnStyleItemChooser_AgeAppropriateHairStyle_Patch
	{
		public static bool Prefix(Pawn pawn, HairDef hair)
		{
			if (pawn.HairIsAllowed(hair) is false)
			{
				return false;
			}
			return true;
		}

		public static bool HairIsAllowed(this Pawn pawn, HairDef hair)
		{
			var extension = hair.GetModExtension<HairExtension>();
			if (extension != null && extension.allowedYears.Includes(pawn.ageTracker.AgeBiologicalYearsFloat) is false)
			{
				return false;
			}
			return true;
		}
	}
}
