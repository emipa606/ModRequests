using System;
using CM_Callouts.PendingCallouts.Combat;
using HarmonyLib;
using Verse;

namespace CM_Callouts
{
	// Token: 0x02000013 RID: 19
	[StaticConstructorOnStartup]
	public static class Verb_LaunchProjectilePatches
	{
		// Token: 0x02000039 RID: 57
		[HarmonyPatch(typeof(Verb_LaunchProjectile))]
		[HarmonyPatch("WarmupComplete", MethodType.Normal)]
		public static class Verb_LaunchProjectile_WarmupComplete
		{
			// Token: 0x06000077 RID: 119 RVA: 0x00004AB0 File Offset: 0x00002CB0
			[HarmonyPostfix]
			public static void Postfix(Verb_LaunchProjectile __instance)
			{
				bool flag = __instance.CasterPawn == null;
				if (!flag)
				{
					bool flag2 = CalloutUtility.CanCalloutNow(__instance.CasterPawn) && CalloutUtility.CanCalloutAtTarget(__instance.CurrentTarget.Thing);
					if (flag2)
					{
						new PendingCalloutEventRangedAttempt(__instance.CasterPawn, __instance.CurrentTarget.Pawn, __instance).AttemptCallout();
					}
				}
			}
		}
	}
}
