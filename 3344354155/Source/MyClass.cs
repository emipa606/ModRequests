using HarmonyLib; // Needed for Harmony
using RimWorld; // Access RimWorld's classes
using Verse; // Access Verse namespace (important for modding RimWorld)
using System.Reflection; // For reflection, though AccessTools may handle most of this
using System;

namespace fixSpear
{
	public class fixSpear
	{
		[StaticConstructorOnStartup]
		public static class ArmorPenetrationPatches
		{
			static ArmorPenetrationPatches()
			{
				var harmony = new Harmony("I.fixSpear");
				harmony.PatchAll();
			}

			[HarmonyPatch(typeof(VerbProperties))]
			[HarmonyPatch("AdjustedArmorPenetration", new[] { typeof(Tool), typeof(Pawn) ,typeof(Thing), typeof(HediffComp_VerbGiver)})]
			public static class Patch_AdjustedArmorPenetration_WithParams
			{
				public static void Postfix(VerbProperties __instance, ref float __result, Tool tool, Pawn attacker, Thing equipment, HediffComp_VerbGiver hediffCompSource)
				{
					float num = tool?.armorPenetration ?? -1.0f;

					//if (num >= 0f && equipment != null && attacker != null) {
					//	__result *= __instance.GetDamageFactorFor(tool, attacker, hediffCompSource);
					//}
					if (num >= 0f && attacker != null) {
						__result *= __instance.GetDamageFactorFor(tool, attacker, hediffCompSource);
					}
				}
			}
		}
	}
}

