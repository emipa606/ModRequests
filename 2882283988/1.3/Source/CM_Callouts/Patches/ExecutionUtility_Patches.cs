using System;
using CM_Callouts.PendingCallouts.Interaction;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CM_Callouts.Patches
{
	// Token: 0x02000024 RID: 36
	[StaticConstructorOnStartup]
	public static class ExecutionUtility_Patches
	{
		// Token: 0x0200003B RID: 59
		[HarmonyPatch(typeof(ExecutionUtility))]
		[HarmonyPatch("DoExecutionByCut", MethodType.Normal)]
		public static class ExecutionUtility_DoExecutionByCut
		{
			// Token: 0x06000079 RID: 121 RVA: 0x00004B90 File Offset: 0x00002D90
			[HarmonyPrefix]
			public static void Prefix(Pawn executioner, Pawn victim)
			{
				bool flag = !victim.AnimalOrWildMan();
				if (!flag)
				{
					new PendingCalloutEventAnimalInteraction(executioner, victim, CalloutDefOf.CM_Callouts_RulePack_Interaction_Animal_Slaughter_Initiated, CalloutDefOf.CM_Callouts_RulePack_Interaction_Animal_Slaughter_Received).AttemptCallout();
				}
			}
		}
	}
}
