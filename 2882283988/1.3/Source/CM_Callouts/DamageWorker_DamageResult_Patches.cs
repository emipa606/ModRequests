using System;
using System.Collections.Generic;
using System.Linq;
using CM_Callouts.PendingCallouts;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace CM_Callouts
{
	// Token: 0x0200000A RID: 10
	[StaticConstructorOnStartup]
	public static class DamageWorker_DamageResult_Patches
	{
		// Token: 0x02000032 RID: 50
		[HarmonyPatch(typeof(DamageWorker.DamageResult))]
		[HarmonyPatch("AssociateWithLog", MethodType.Normal)]
		public static class DamageWorker_DamageResult_AssociateWithLog
		{
			// Token: 0x0600006B RID: 107 RVA: 0x0000441C File Offset: 0x0000261C
			[HarmonyPostfix]
			public static void Postfix(DamageWorker.DamageResult __instance)
			{
				bool deflected = __instance.deflected;
				if (!deflected)
				{
					Pawn hitPawn = __instance.hitThing as Pawn;
					bool flag = hitPawn != null && !__instance.parts.NullOrEmpty<BodyPartRecord>();
					if (flag)
					{
						List<BodyPartRecord> list = __instance.parts.Distinct<BodyPartRecord>().ToList<BodyPartRecord>();
						List<bool> list2 = (from part in list
						select hitPawn.health.hediffSet.GetPartHealth(part) <= 0f).ToList<bool>();
						bool flag2 = CalloutUtility.pendingCallout != null;
						if (flag2)
						{
							CalloutUtility.pendingCallout.FillBodyPartInfo(hitPawn.RaceProps.body, list, list2);
							CalloutUtility.pendingCallout.AttemptCallout();
							CalloutUtility.pendingCallout = null;
						}
						else
						{
							bool flag3 = CalloutUtility.CanCalloutNow(hitPawn);
							if (flag3)
							{
								new PendingCalloutEventWounded(hitPawn).AttemptCallout();
							}
						}
						DamageWorker_DamageResult_Patches.DamageWorker_DamageResult_AssociateWithLog.ThrowDestroyedPartMotes(hitPawn, list, list2);
					}
				}
			}

			// Token: 0x0600006C RID: 108 RVA: 0x00004510 File Offset: 0x00002710
			private static void ThrowDestroyedPartMotes(Pawn pawn, List<BodyPartRecord> recipientPartsDamaged, List<bool> recipientPartsDestroyed)
			{
				ShowWoundLevel showWoundLevel = CalloutMod.settings.showWoundLevel;
				bool flag = showWoundLevel == ShowWoundLevel.None || !pawn.SpawnedOrAnyParentSpawned;
				if (!flag)
				{
					Vector3 drawPos = pawn.SpawnedParentOrMe.DrawPos;
					Map map = pawn.SpawnedParentOrMe.Map;
					for (int i = 0; i < recipientPartsDestroyed.Count; i++)
					{
						bool flag2 = recipientPartsDestroyed[i];
						if (flag2)
						{
							CalloutTracker.CreateWoundTextMote(drawPos, map, recipientPartsDamaged[i].def.label, Color.magenta, -1f);
						}
						else
						{
							float partHealth = pawn.health.hediffSet.GetPartHealth(recipientPartsDamaged[i]);
							float maxHealth = recipientPartsDamaged[i].def.GetMaxHealth(pawn);
							float num = partHealth / maxHealth;
							bool flag3 = num < 0.4f;
							bool flag4;
							if (flag3)
							{
								flag4 = (showWoundLevel >= ShowWoundLevel.Major);
							}
							else
							{
								bool flag5 = num < 0.7f;
								if (flag5)
								{
									flag4 = (showWoundLevel >= ShowWoundLevel.Serious);
								}
								else
								{
									flag4 = (showWoundLevel >= ShowWoundLevel.All);
								}
							}
							bool flag6 = flag4;
							if (flag6)
							{
								Color second = HealthUtility.GetPartConditionLabel(pawn, recipientPartsDamaged[i]).Second;
								CalloutTracker.CreateWoundTextMote(drawPos, map, recipientPartsDamaged[i].def.label, second, -1f);
							}
						}
					}
				}
			}
		}
	}
}
