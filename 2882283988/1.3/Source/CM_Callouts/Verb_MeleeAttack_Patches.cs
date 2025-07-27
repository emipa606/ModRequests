using System;
using CM_Callouts.PendingCallouts.Combat;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CM_Callouts
{
	// Token: 0x02000012 RID: 18
	[StaticConstructorOnStartup]
	public static class Verb_MeleeAttack_Patches
	{
		// Token: 0x02000038 RID: 56
		[HarmonyPatch(typeof(Verb_MeleeAttack))]
		[HarmonyPatch("CreateCombatLog", MethodType.Normal)]
		public static class Verb_MeleeAttack_CreateCombatLog
		{
			// Token: 0x06000076 RID: 118 RVA: 0x000049B8 File Offset: 0x00002BB8
			[HarmonyPostfix]
			public static void Postfix(Verb_MeleeAttack __instance, Func<ManeuverDef, RulePackDef> rulePackGetter)
			{
				CalloutUtility.pendingCallout = null;
				bool flag = __instance.maneuver == null || __instance.tool == null;
				if (!flag)
				{
					bool flag2 = __instance.CurrentTarget.Thing is Pawn && CalloutUtility.CanCalloutNow(__instance.CasterPawn) && CalloutUtility.CanCalloutAtTarget(__instance.CurrentTarget.Thing);
					if (flag2)
					{
						bool flag3 = rulePackGetter(__instance.maneuver) == __instance.maneuver.combatLogRulesDodge;
						if (!flag3)
						{
							bool flag4 = rulePackGetter(__instance.maneuver) == __instance.maneuver.combatLogRulesHit;
							if (flag4)
							{
								CalloutUtility.pendingCallout = new PendingCalloutEventMeleeImpact(__instance.CasterPawn, __instance.CurrentTarget.Thing as Pawn);
							}
							else
							{
								bool flag5 = rulePackGetter(__instance.maneuver) == __instance.maneuver.combatLogRulesMiss;
								if (flag5)
								{
								}
							}
						}
					}
				}
			}
		}
	}
}
