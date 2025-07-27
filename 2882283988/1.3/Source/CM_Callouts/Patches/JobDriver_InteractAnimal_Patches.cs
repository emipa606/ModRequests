using System;
using CM_Callouts.PendingCallouts.Interaction;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CM_Callouts.Patches
{
	// Token: 0x02000025 RID: 37
	[StaticConstructorOnStartup]
	public static class JobDriver_InteractAnimal_Patches
	{
		// Token: 0x0200003C RID: 60
		[HarmonyPatch(typeof(JobDriver_InteractAnimal))]
		[HarmonyPatch("StartFeedAnimal", MethodType.Normal)]
		public static class JobDriver_InteractAnimal_StartFeedAnimal
		{
			// Token: 0x0600007A RID: 122 RVA: 0x00004BC4 File Offset: 0x00002DC4
			[HarmonyPostfix]
			public static void Postfix(JobDriver_InteractAnimal __instance, TargetIndex tameeInd, Toil __result)
			{
				bool flag = __result == null;
				if (!flag)
				{
					__result.AddPreInitAction(delegate
					{
						Pawn actor = __instance.GetActor();
						Pawn pawn = actor.CurJob.GetTarget(tameeInd).Pawn;
						Thing thing = FoodUtility.BestFoodInInventory(actor, pawn, FoodPreferability.NeverForNutrition, FoodPreferability.RawTasty, 0f, false);
						bool flag2 = thing != null;
						if (flag2)
						{
							new PendingCalloutEventAnimalInteractionWithFood(actor, pawn, thing.def, CalloutDefOf.CM_Callouts_RulePack_Interaction_Animal_Feed_Initiated, CalloutDefOf.CM_Callouts_RulePack_Interaction_Animal_Feed_Received).AttemptCallout();
						}
					});
				}
			}
		}
	}
}
