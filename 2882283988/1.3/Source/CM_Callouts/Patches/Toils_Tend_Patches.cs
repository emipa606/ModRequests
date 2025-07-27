using System;
using CM_Callouts.PendingCallouts.Interaction;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CM_Callouts.Patches
{
	// Token: 0x02000029 RID: 41
	[StaticConstructorOnStartup]
	public static class Toils_Tend_Patches
	{
		// Token: 0x02000040 RID: 64
		[HarmonyPatch(typeof(Toils_Tend))]
		[HarmonyPatch("FinalizeTend", MethodType.Normal)]
		public static class Toils_Tend_FinalizeTend
		{
			// Token: 0x0600007E RID: 126 RVA: 0x00004CDC File Offset: 0x00002EDC
			[HarmonyPostfix]
			public static void Postfix(Pawn patient, Toil __result)
			{
				bool flag = __result == null;
				if (!flag)
				{
					bool flag2 = patient == null || !patient.AnimalOrWildMan();
					if (!flag2)
					{
						__result.AddPreInitAction(delegate
						{
							Pawn actor = __result.GetActor();
							new PendingCalloutEventAnimalInteraction(actor, patient, CalloutDefOf.CM_Callouts_RulePack_Interaction_Animal_Tend_Initiated, CalloutDefOf.CM_Callouts_RulePack_Interaction_Animal_Tend_Received).AttemptCallout();
						});
					}
				}
			}
		}
	}
}
