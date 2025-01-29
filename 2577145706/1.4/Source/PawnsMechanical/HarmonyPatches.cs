using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System;
using HarmonyLib;
using RimWorld;
using Verse.AI;
using Verse;

namespace VexedThings.HarmonyPatches
{

	// Makes pawns stunnable by emp attacks.
	[HarmonyPatch(typeof(StunHandler), "get_AffectedByEMP")]
	public class AffectedByEMP_HarmonyPatch
	{
		[HarmonyPostfix]
		public static void Postfix(StunHandler __instance, ref bool __result, Thing ___parent)
		{
			if (___parent is Pawn pawn)
			{
				if (pawn != null && pawn.def.HasModExtension<SyntheticPawnsExtension>() &&
				pawn.def.GetModExtension<SyntheticPawnsExtension>().canBeStunnedByEMP)
				{
					__result = true;
				}
			}
		}
	}

	// Disables cosemetic breath vapors in cold temperatures.
	[HarmonyPatch(typeof(PawnBreathMoteMaker), "ProcessPostTickVisuals")]
	internal class PawnBreathMoteMaker_BreathMoteMakerTickPatch
	{
		[HarmonyPrefix]
		public static bool Prefix(Pawn ___pawn)
		{
			SyntheticPawnsExtension pawn = ___pawn.GetSynthExtension();
			return pawn == null || !pawn.disableBreathVapors;
		}
	}

	// Disables listed actions from being performed by a pawn.
	[HarmonyPatch(typeof(Pawn_JobTracker), "StartJob")]
	internal class Pawn_JobTracker_StartJobPatch
	{
		[HarmonyPrefix]
		public static bool Prefix(Job newJob, Pawn ___pawn)
		{
			SyntheticPawnsExtension pawn = ___pawn.GetSynthExtension();
			List<JobDef> list;
			if (pawn == null)
			{
				list = null;
			}
			else
			{
				SyntheticPawnsExtension patch = pawn;
				list = (patch?.disableListedActions);
			}
			List<JobDef> list2 = list;
			return list2 == null || !list2.Contains(newJob.def);
		}
	}

    // Stops corpses with these fleshTypes from being edible.
    [HarmonyPatch(typeof(Corpse), "get_IngestibleNow")]
	public class IngestibleNow_HarmonyPatch
	{
		[HarmonyPostfix]
		public static void Postfix(Corpse __instance, ref bool __result)
		{
			bool flag = !__result;
			if (!flag)
			{
				bool flag2 = __instance.InnerPawn.RaceProps.FleshType == FleshTypeDefOfSynth.VexedMechanical;
				if (flag2)
				{
					__result = false;
				}
				bool flag3 = __instance.InnerPawn.RaceProps.FleshType == FleshTypeDefOfSynth.VexedSynthetic;
				if (flag3)
				{
					__result = false;
				}
			}
		}
	}
}