using HarmonyLib;
using Quarry;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using VFE.Mechanoids;
using VFEMech;

namespace QuarryPatch
{
	[DefOf]
	public static class QPDefOf
    {
		public static WorkTypeDef QuarryMining;
	}
	[StaticConstructorOnStartup]
	internal static class HarmonyInit
	{
		static HarmonyInit()
		{
			new Harmony("QuarryPatch.Mod").PatchAll();
		}
	}

	[HarmonyPatch(typeof(Machine), "SpawnSetup")]
	public static class SpawnSetup_Patch
	{
		public static void Prefix(Machine __instance)
		{
			if (__instance.workSettings != null)
            {
				if (__instance.TryGetComp<CompMachine>()?.myBuilding?.def?.defName == "VFE_Mechanoids_AutominerBase")
				{
					if (!__instance.workSettings.WorkIsActive(QPDefOf.QuarryMining))
                    {
						__instance.workSettings.SetPriority(QPDefOf.QuarryMining, 1);
                    }
				}
			}
		}
	}
}
