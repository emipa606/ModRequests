using System;
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace THVanillaPatches.Patches
{
	public class ImmortalityPatch(THPatchDef def) : ToggleablePatchParent(def)
	{
		protected override List<PatchInfo> Patches => new List<PatchInfo>
		{
			new PatchInfo(
				AccessTools.Method(typeof(ThoughtWorker_AgeReversalDemanded),"ShouldHaveThought"),
				Prefix: new HarmonyMethod(GetType(), nameof(PreShouldWantAgeReversal)))
		};
		
		public static bool PreShouldWantAgeReversal(ref ThoughtState __result, Pawn p)
		{
			if (!p.genes.HasGene(THVanillaPatchesDefsOf.DiseaseFree)) return true;
			__result = ThoughtState.Inactive;
			return false;
		}
		
	}
}