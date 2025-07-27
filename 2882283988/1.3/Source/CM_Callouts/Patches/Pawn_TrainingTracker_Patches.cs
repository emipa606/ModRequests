using System;
using CM_Callouts.PendingCallouts.Interaction;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CM_Callouts.Patches
{
	// Token: 0x02000026 RID: 38
	[StaticConstructorOnStartup]
	public static class Pawn_TrainingTracker_Patches
	{
		// Token: 0x0200003D RID: 61
		[HarmonyPatch(typeof(Pawn_TrainingTracker))]
		[HarmonyPatch("Train", MethodType.Normal)]
		public static class Pawn_TrainingTracker_Train
		{
			// Token: 0x0600007B RID: 123 RVA: 0x00004C04 File Offset: 0x00002E04
			[HarmonyPostfix]
			public static void Postfix(Pawn_TrainingTracker __instance, TrainableDef td, Pawn trainer, bool complete, Pawn ___pawn)
			{
				bool flag = td == TrainableDefOf.Tameness && complete;
				if (!flag)
				{
					new PendingCalloutEventAnimalInteraction(trainer, ___pawn, CalloutDefOf.CM_Callouts_RulePack_Interaction_Animal_Train_Success_Initiated, CalloutDefOf.CM_Callouts_RulePack_Interaction_Animal_Train_Success_Received).AttemptCallout();
				}
			}
		}
	}
}
