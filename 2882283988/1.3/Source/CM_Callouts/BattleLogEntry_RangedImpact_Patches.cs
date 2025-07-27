using System;
using CM_Callouts.PendingCallouts.Combat;
using HarmonyLib;
using Verse;

namespace CM_Callouts
{
	// Token: 0x02000009 RID: 9
	[StaticConstructorOnStartup]
	public static class BattleLogEntry_RangedImpact_Patches
	{
		// Token: 0x02000031 RID: 49
		[HarmonyPatch(typeof(BattleLogEntry_RangedImpact))]
		[HarmonyPatch(MethodType.Constructor)]
		[HarmonyPatch(new Type[]
		{
			typeof(Thing),
			typeof(Thing),
			typeof(Thing),
			typeof(ThingDef),
			typeof(ThingDef),
			typeof(ThingDef)
		})]
		public static class BattleLogEntry_RangedImpact_Constructor
		{
			// Token: 0x0600006A RID: 106 RVA: 0x000043A8 File Offset: 0x000025A8
			[HarmonyPostfix]
			public static void Postfix(BattleLogEntry_RangedImpact __instance, Thing initiator, Thing recipient, Thing originalTarget, ThingDef weaponDef, ThingDef projectileDef, ThingDef coverDef)
			{
				CalloutUtility.pendingCallout = null;
				bool flag = !(initiator is Pawn);
				if (!flag)
				{
					bool flag2 = recipient != originalTarget;
					if (!flag2)
					{
						bool flag3 = recipient is Pawn && CalloutUtility.CanCalloutNow(initiator) && CalloutUtility.CanCalloutAtTarget(recipient);
						if (flag3)
						{
							CalloutUtility.pendingCallout = new PendingCalloutEventRangedImpact(initiator as Pawn, recipient as Pawn, originalTarget as Pawn, weaponDef, projectileDef, coverDef);
						}
					}
				}
			}
		}
	}
}
