using System.Collections.Generic;
using System.Security.AccessControl;
using HarmonyLib;
using RimWorld;
using Verse;

namespace THVanillaPatches.Patches
{
	public class GenderlessRoyaltyPatch(THPatchDef def): ToggleablePatchParent(def)
	{
		protected override List<PatchInfo> Patches => new List<PatchInfo>()
		{
			new PatchInfo(AccessTools.Method(typeof(ApparelProperties), nameof(ApparelProperties.CorrectGenderForWearing)), Prefix: new HarmonyMethod(GetType(), nameof(AlwaysCorrectGender)))
		};

		//FUCK YOUR GENDER NORMS >:(
		private static bool AlwaysCorrectGender(ref bool __result, Gender wearerGender)
		{
			__result = true;
			return false;
		}
	}
}