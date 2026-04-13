using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace BionicleKanohiMasksOfPower
{
    [HarmonyPatch(typeof(PawnCapacityWorker_Manipulation), "CalculateCapacityLevel")]
	public static class PawnCapacityWorker_Manipulation_CalculateCapacityLevel_Patch
	{
		private static void Postfix(ref float __result, HediffSet diffSet, List<PawnCapacityUtility.CapacityImpactor> impactors = null)
		{
			if (diffSet.pawn.Wears(BionicleDefOf.BKMOP_Kakama, out var apparel) && apparel.IsMasterworkOrLegendary())//doubles manipulation speed with kakama
            {
				__result *= 2f;
			}
		}
	}

	[HarmonyPatch(typeof(ITab_Pawn_Gear), "CanControl", MethodType.Getter)]
	public static class ITab_Pawn_Gear_CanControl_Patch
	{
		private static void Postfix(ref bool __result, ITab_Pawn_Gear __instance)
		{
			if (__instance.SelPawnForGear?.health?.hediffSet?.HasHediff(BionicleDefOf.BKMOP_PawnDuplicate) ?? false)
            {
				__result = false;
			}
		}
	}

	[HarmonyPatch(typeof(Pawn_ApparelTracker), "TryDrop",
	new Type[] { typeof(Apparel), typeof(Apparel), typeof(IntVec3), typeof(bool) },
	new ArgumentType[] { ArgumentType.Normal, ArgumentType.Out, ArgumentType.Normal, ArgumentType.Normal })]
	public static class Patch_TryDrop//prevent duplicate from dropping weapon
	{
		private static bool Prefix(Pawn_ApparelTracker __instance, Apparel ap)
		{
			if (__instance.pawn?.health?.hediffSet?.HasHediff(BionicleDefOf.BKMOP_PawnDuplicate) ?? false)
			{
				return false;
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(Pawn_EquipmentTracker), "TryDropEquipment")]
	public static class TryDropEquipment_Patch//prevent duplicate from droppping equipment
	{
		private static bool Prefix(Pawn_EquipmentTracker __instance, ThingWithComps eq, ThingWithComps resultingEq, IntVec3 pos, bool forbid = true)
		{
			if (__instance.pawn?.health?.hediffSet?.HasHediff(BionicleDefOf.BKMOP_PawnDuplicate) ?? false)
			{
				return false;
			}
			return true;
		}
	}
	[HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders")]
	public static class FloatMenuMakerMap_AddHumanlikeOrders_Patch
	{
		private static bool Prefix(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
		{
			if (pawn?.health?.hediffSet?.HasHediff(BionicleDefOf.BKMOP_PawnDuplicate) ?? false)
			{
				return false;
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(EquipmentUtility), "CanEquip", 
		new Type[] { typeof(Thing), typeof(Pawn), typeof(string), typeof(bool) },
		new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Normal })]
	public static class EquipmentUtility_CanEquip_Patch//prevent duplicate from picking up item
	{
		private static void Postfix(ref bool __result, Thing thing, Pawn pawn, ref string cantReason, bool checkBonded = true)
		{
			if (__result && (pawn.health?.hediffSet?.HasHediff(BionicleDefOf.BKMOP_PawnDuplicate) ?? false))
			{
				cantReason = "Bionicle.IsHologram".Translate();
				__result = false;
			}
		}
	}
}