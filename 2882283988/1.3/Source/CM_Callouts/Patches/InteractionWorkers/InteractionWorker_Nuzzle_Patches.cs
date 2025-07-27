using System;
using CM_Callouts.PendingCallouts.Interaction;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CM_Callouts.Patches.InteractionWorkers
{
	// Token: 0x0200002A RID: 42
	[StaticConstructorOnStartup]
	public static class InteractionWorker_Nuzzle_Patches
	{
		// Token: 0x02000041 RID: 65
		[HarmonyPatch(typeof(InteractionWorker_Nuzzle))]
		[HarmonyPatch("Interacted", MethodType.Normal)]
		public static class InteractionWorker_Nuzzle_Interacted
		{
			// Token: 0x0600007F RID: 127 RVA: 0x00004D44 File Offset: 0x00002F44
			[HarmonyPostfix]
			public static void Postfix(InteractionWorker_Nuzzle __instance, Pawn initiator, Pawn recipient)
			{
				bool flag = recipient.needs.mood == null;
				if (!flag)
				{
					new PendingCalloutEventAnimalInteraction(recipient, initiator, CalloutDefOf.CM_Callouts_RulePack_Interaction_Animal_Nuzzle_Initiated, CalloutDefOf.CM_Callouts_RulePack_Interaction_Animal_Nuzzle_Received).AttemptCallout();
				}
			}
		}
	}
}
