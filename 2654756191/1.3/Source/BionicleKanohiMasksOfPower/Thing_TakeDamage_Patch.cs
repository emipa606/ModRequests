using HarmonyLib;
using Verse;

namespace BionicleKanohiMasksOfPower
{
    [HarmonyPatch(typeof(Thing), "TakeDamage")]
	public static class Thing_TakeDamage_Patch//kaukau sos2 patch
	{
		public static bool Prefix(Thing __instance, DamageInfo dinfo)
		{
			if (__instance is Pawn pawn && pawn.Wears(BionicleDefOf.BKMOP_Kaukau, out var apparel) && apparel.IsMasterworkOrLegendary() && dinfo.Def?.defName == "VacuumDamage")
            {
				return false;
			}
			return true;
		}
	}
}