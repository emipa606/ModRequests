using System;
using System.Linq.Expressions;
using System.Reflection;
using CM_Callouts.PendingCallouts.Interaction;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CM_Callouts.Patches.InteractionWorkers
{
	// Token: 0x0200002B RID: 43
	[StaticConstructorOnStartup]
	public static class InteractionWorker_RecruitAttempt_Patches
	{
		// Token: 0x02000042 RID: 66
		public static class InteractionWorker_RecruitAttempt_DoRecruit
		{
			// Token: 0x06000080 RID: 128 RVA: 0x00004D80 File Offset: 0x00002F80
			[HarmonyPostfix]
			public static void Postfix(Pawn recruiter, Pawn recruitee)
			{
				bool flag = !recruitee.AnimalOrWildMan();
				if (!flag)
				{
					new PendingCalloutEventAnimalInteraction(recruiter, recruitee, CalloutDefOf.CM_Callouts_RulePack_Interaction_Animal_Tame_Success_Initiated, CalloutDefOf.CM_Callouts_RulePack_Interaction_Animal_Tame_Success_Received).AttemptCallout();
				}
			}

			// Token: 0x06000081 RID: 129 RVA: 0x00004DB4 File Offset: 0x00002FB4
			public static void Patch(Harmony harmony)
            {
                MethodInfo doRecruit = AccessTools.Method(typeof(InteractionWorker_RecruitAttempt), "DoRecruit", new Type[] { typeof(Pawn), typeof(Pawn), typeof(float), typeof(string).MakeByRefType(), typeof(string).MakeByRefType(), typeof(bool), typeof(bool) });
                if (doRecruit != null)
                {
                    HarmonyMethod postFix = new HarmonyMethod(SymbolExtensions.GetMethodInfo(() => Postfix(null, null)));
                    harmony.Patch(doRecruit, null, postFix);
                }
            }
		}
	}
}
