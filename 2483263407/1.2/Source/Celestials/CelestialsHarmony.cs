using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using RimWorld;
using Verse;

using HarmonyLib;
namespace Celestials
{
    [StaticConstructorOnStartup]
    public static class CelestialsHarmony
    {
        static CelestialsHarmony()
        {
            Harmony CelestialsHarmony = new Harmony("com.neronix17.celestials.mod");

            CelestialsHarmony.PatchAll();
        }
    }


	[HarmonyPatch(typeof(Corpse), "ShouldVanish", MethodType.Getter)]
	public static class Patch_Corpse_ShouldVanish
	{
		[HarmonyPrefix]
		public static bool Prefix(Corpse __instance, bool __result)
		{
            if(__instance == null || __instance.InnerPawn == null)
            {
                __result = true;
                return false;
            }
			return true;
		}
    }
}
