using System;
using CM_Callouts.PendingCallouts;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CM_Callouts
{
	// Token: 0x0200000F RID: 15
	[StaticConstructorOnStartup]
	public static class Pawn_DraftController_Patches
	{
		// Token: 0x02000035 RID: 53
		[HarmonyPatch(typeof(Pawn_DraftController))]
		[HarmonyPatch("Drafted", MethodType.Setter)]
		public static class Pawn_DraftController_Drafted
		{
			// Token: 0x0600006F RID: 111 RVA: 0x00004748 File Offset: 0x00002948
			[HarmonyPrefix]
			public static void Prefix(Pawn_DraftController __instance, bool value, bool ___draftedInt)
			{
				bool flag = value && value != ___draftedInt;
				if (flag)
				{
					Pawn_DraftController_Patches.Pawn_DraftController_Drafted.wereDrafted = true;
				}
			}

			// Token: 0x06000070 RID: 112 RVA: 0x00004770 File Offset: 0x00002970
			[HarmonyPostfix]
			public static void Postfix(Pawn_DraftController __instance, bool value, bool ___draftedInt)
			{
				bool flag = ___draftedInt && Pawn_DraftController_Patches.Pawn_DraftController_Drafted.wereDrafted;
				if (flag)
				{
					new PendingCalloutEventSinglePawn(CalloutCategory.Undefined, __instance.pawn, CalloutDefOf.CM_Callouts_RulePack_Drafted).AttemptCallout();
				}
			}

			// Token: 0x04000074 RID: 116
			private static bool wereDrafted = false;
		}
	}
}
