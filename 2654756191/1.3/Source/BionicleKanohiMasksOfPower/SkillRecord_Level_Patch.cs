using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace BionicleKanohiMasksOfPower
{
    [HarmonyPatch(typeof(SkillRecord), "Level", MethodType.Getter)]
	public static class SkillRecord_Level_Patch
	{
		public static void Postfix(SkillRecord __instance, ref int __result)
		{
			if ((__instance.def == SkillDefOf.Melee || __instance.def == SkillDefOf.Shooting) && __instance.Pawn.Wears(BionicleDefOf.BKMOP_Akaku, out var apparel) && apparel.IsMasterworkOrLegendary())//skill increase of shooting and melee with akaku
            {
				__result = __result + 5;
			}
			else if ((__instance.def == SkillDefOf.Social || __instance.def == SkillDefOf.Intellectual || __instance.def == SkillDefOf.Crafting) //skill increase of social, intellectual, and crafting with rau
				&& __instance.Pawn.Wears(BionicleDefOf.BKMOP_Rau, out var apparel2) && apparel2.IsMasterworkOrLegendary())
            {
				__result = __result + 5;
			}
		}
	}
}