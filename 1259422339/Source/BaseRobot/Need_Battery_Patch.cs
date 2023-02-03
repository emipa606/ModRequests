using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace BaseRobot
{
    // Token: 0x0200001A RID: 26
    [HarmonyPatch(typeof(Pawn_NeedsTracker))]
	[HarmonyPatch("ShouldHaveNeed")]
	[HarmonyPatch(new Type[]
	{
		typeof(NeedDef)
	})]
	internal class Need_Battery_Patch
	{
		// Token: 0x0600007A RID: 122 RVA: 0x00005ABC File Offset: 0x00003CBC
		private static void Postfix(Pawn_NeedsTracker __instance, NeedDef nd, ref bool __result)
		{
			var pawn = (Pawn)AccessTools.Field(typeof(Pawn_NeedsTracker), "pawn").GetValue(__instance);
			if (nd.needClass == typeof(Need_Battery))
			{
				if (pawn.def.thingClass == typeof(ArcBaseRobot))
				{
					__result = true;
				}
				else
				{
					__result = false;
				}
			}
			else if (pawn.def.thingClass == typeof(ArcBaseRobot))
			{
				__result = false;
			}
		}
	}
}
