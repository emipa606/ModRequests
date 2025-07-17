using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace THVanillaPatches.Patches
{
	public class MisanthropistTraitPatch(THPatchDef def): ToggleablePatchParent(def)
	{
		protected override List<PatchInfo> Patches => new List<PatchInfo>
		{
			new PatchInfo(AccessTools.Method(typeof(TraitSet), nameof(TraitSet.GainTrait)),
				Postfix: new HarmonyMethod(GetType(), nameof(GainMisanthropy)))
		};

		private static void GainMisanthropy(ref TraitSet __instance, Trait trait, bool suppressConflicts)
		{
			if (!__instance.HasTrait(TraitDefOf.DislikesWomen) ||
			    !__instance.HasTrait(TraitDefOf.DislikesMen)) return;
			__instance.RemoveTrait(__instance.GetTrait(TraitDefOf.DislikesWomen));
			__instance.RemoveTrait(__instance.GetTrait(TraitDefOf.DislikesMen));
			__instance.GainTrait(new Trait(THVanillaPatchesDefsOf.THVP_DislikesPeople));
		}
		
	}
}