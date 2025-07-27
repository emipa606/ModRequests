using System;
using HarmonyLib;
using Verse;
using Verse.AI;

namespace CM_Callouts.Patches
{
	// Token: 0x02000027 RID: 39
	[StaticConstructorOnStartup]
	public static class ThinkNode_ChancePerHour_Nuzzle_Patches
	{
		// Token: 0x0200003E RID: 62
		[HarmonyPatch(typeof(ThinkNode_ChancePerHour_Nuzzle))]
		[HarmonyPatch("MtbHours", MethodType.Normal)]
		public static class ThinkNode_ChancePerHour_Nuzzle_MtbHours
		{
			// Token: 0x0600007C RID: 124 RVA: 0x00004C3C File Offset: 0x00002E3C
			[HarmonyPostfix]
			public static void Postfix(ref float __result)
			{
				bool flag = Prefs.DevMode && CalloutMod.settings.hyperNuzzling;
				if (flag)
				{
					__result = 0.01f;
				}
			}
		}
	}
}
