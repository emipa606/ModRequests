using System;
using HarmonyLib;
using Verse;

namespace CM_Callouts
{
	// Token: 0x02000011 RID: 17
	[StaticConstructorOnStartup]
	public static class RealTime_Patches
	{
		// Token: 0x02000037 RID: 55
		[HarmonyPatch(typeof(RealTime))]
		[HarmonyPatch("Update", MethodType.Normal)]
		public static class RealTime_Update
		{
			// Token: 0x06000075 RID: 117 RVA: 0x000049AC File Offset: 0x00002BAC
			[HarmonyPostfix]
			public static void Postfix()
			{
				CalloutTracker.UpdateTextMoteQueuesRealTime();
			}
		}
	}
}
