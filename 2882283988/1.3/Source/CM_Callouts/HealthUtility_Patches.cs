using System;
using CM_Callouts.PendingCallouts;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace CM_Callouts
{
	// Token: 0x02000010 RID: 16
	[StaticConstructorOnStartup]
	public static class HealthUtility_Patches
	{
		// Token: 0x02000036 RID: 54
		[HarmonyPatch(typeof(HealthUtility))]
		[HarmonyPatch("AdjustSeverity", MethodType.Normal)]
		public static class HealthUtility_AdjustSeverity
		{
			// Token: 0x06000072 RID: 114 RVA: 0x000047B0 File Offset: 0x000029B0
			[HarmonyPrefix]
			public static void Prefix(Pawn pawn, HediffDef hdDef, float sevOffset)
			{
				bool flag = sevOffset != 0f && pawn != null && hdDef.lethalSeverity > 0f;
				if (flag)
				{
					HealthUtility_Patches.HealthUtility_AdjustSeverity.initialFoundHediff = pawn.health.hediffSet.GetFirstHediffOfDef(hdDef, false);
					bool flag2 = HealthUtility_Patches.HealthUtility_AdjustSeverity.initialFoundHediff != null;
					if (flag2)
					{
						HealthUtility_Patches.HealthUtility_AdjustSeverity.startingStageIndex = HealthUtility_Patches.HealthUtility_AdjustSeverity.initialFoundHediff.CurStageIndex;
					}
				}
				else
				{
					HealthUtility_Patches.HealthUtility_AdjustSeverity.initialFoundHediff = null;
					HealthUtility_Patches.HealthUtility_AdjustSeverity.startingStageIndex = -1;
				}
			}

			// Token: 0x06000073 RID: 115 RVA: 0x00004824 File Offset: 0x00002A24
			[HarmonyPostfix]
			public static void Postfix(Pawn pawn, HediffDef hdDef, float sevOffset)
			{
				bool flag = sevOffset == 0f || pawn == null || hdDef.lethalSeverity <= 0f;
				if (flag)
				{
					HealthUtility_Patches.HealthUtility_AdjustSeverity.initialFoundHediff = null;
					HealthUtility_Patches.HealthUtility_AdjustSeverity.startingStageIndex = -1;
				}
				else
				{
					Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(hdDef, false);
					bool flag2 = firstHediffOfDef != null;
					if (flag2)
					{
						int curStageIndex = firstHediffOfDef.CurStageIndex;
						bool flag3 = HealthUtility_Patches.HealthUtility_AdjustSeverity.initialFoundHediff == null || curStageIndex > HealthUtility_Patches.HealthUtility_AdjustSeverity.startingStageIndex;
						if (flag3)
						{
							HediffStage curStage = firstHediffOfDef.CurStage;
							ShowWoundLevel showWoundLevel = CalloutMod.settings.showWoundLevel;
							bool flag4 = pawn.SpawnedOrAnyParentSpawned && curStage != null && curStage.becomeVisible;
							if (flag4)
							{
								new PendingCalloutEventLethalHediffProgression(pawn, firstHediffOfDef).AttemptCallout();
								int num = firstHediffOfDef.def.stages.Count - curStageIndex;
								bool flag5 = showWoundLevel != ShowWoundLevel.None && (showWoundLevel == ShowWoundLevel.All || num <= (int)showWoundLevel);
								if (flag5)
								{
									Vector3 drawPos = pawn.SpawnedParentOrMe.DrawPos;
									Map map = pawn.SpawnedParentOrMe.Map;
									Color color = HealthUtility.SlightlyImpairedColor;
									bool flag6 = num == 1;
									if (flag6)
									{
										color = Color.magenta;
									}
									else
									{
										bool flag7 = num == 2;
										if (flag7)
										{
											color = HealthUtility.RedColor;
										}
										else
										{
											bool flag8 = num == 3;
											if (flag8)
											{
												color = HealthUtility.ImpairedColor;
											}
										}
									}
									CalloutTracker.CreateWoundTextMote(drawPos, map, firstHediffOfDef.Label, color, -1f);
								}
							}
						}
					}
					HealthUtility_Patches.HealthUtility_AdjustSeverity.initialFoundHediff = null;
					HealthUtility_Patches.HealthUtility_AdjustSeverity.startingStageIndex = -1;
				}
			}

			// Token: 0x04000075 RID: 117
			private static Hediff initialFoundHediff = null;

			// Token: 0x04000076 RID: 118
			private static int startingStageIndex = -1;
		}
	}
}
