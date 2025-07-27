using System;
using CM_Callouts.PendingCallouts.Interaction;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CM_Callouts.Patches
{
	// Token: 0x02000028 RID: 40
	[StaticConstructorOnStartup]
	public static class Toils_Ingest_Patches
	{
		// Token: 0x0200003F RID: 63
		[HarmonyPatch(typeof(Toils_Ingest))]
		[HarmonyPatch("ChewIngestible", MethodType.Normal)]
		public static class Toils_Ingest_ChewIngestible
		{
			// Token: 0x0600007D RID: 125 RVA: 0x00004C6C File Offset: 0x00002E6C
			[HarmonyPostfix]
			public static void Postfix(Pawn chewer, TargetIndex ingestibleInd, Toil __result)
			{
				bool flag = __result == null;
				if (!flag)
				{
					bool flag2 = chewer == null || !chewer.AnimalOrWildMan();
					if (!flag2)
					{
						__result.AddPreInitAction(delegate
						{
							Pawn actor = __result.GetActor();
							bool flag3 = actor == chewer;
							if (!flag3)
							{
								Thing thing = actor.CurJob.GetTarget(ingestibleInd).Thing;
								bool flag4 = !thing.IngestibleNow;
								if (!flag4)
								{
									bool flag5 = thing != null;
									if (flag5)
									{
										new PendingCalloutEventAnimalInteractionWithFood(actor, chewer, thing.def, CalloutDefOf.CM_Callouts_RulePack_Interaction_Animal_Feed_Initiated, CalloutDefOf.CM_Callouts_RulePack_Interaction_Animal_Feed_Received).AttemptCallout();
									}
								}
							}
						});
					}
				}
			}
		}
	}
}
