using System;
using CM_Callouts.PendingCallouts.Combat;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CM_Callouts
{
	// Token: 0x02000014 RID: 20
	[StaticConstructorOnStartup]
	public static class Verb_Patches
	{
		// Token: 0x0200003A RID: 58
		[HarmonyPatch(typeof(Verb))]
		[HarmonyPatch("TryStartCastOn", new Type[]
		{
			typeof(LocalTargetInfo),
			typeof(LocalTargetInfo),
			typeof(bool),
			typeof(bool),
			typeof(bool)
		})]
		public static class Verb_TryStartCastOn
		{
			// Token: 0x06000078 RID: 120 RVA: 0x00004B18 File Offset: 0x00002D18
			[HarmonyPostfix]
			public static void Postfix(Verb __instance)
			{
				bool flag = !(__instance is Verb_MeleeAttack) || __instance.CasterPawn == null;
				if (!flag)
				{
					bool flag2 = CalloutUtility.CanCalloutNow(__instance.CasterPawn) && CalloutUtility.CanCalloutAtTarget(__instance.CurrentTarget.Thing);
					if (flag2)
					{
						new PendingCalloutEventMeleeAttempt(__instance.CasterPawn, __instance.CurrentTarget.Thing as Pawn, __instance).AttemptCallout();
					}
				}
			}
		}
	}
}
