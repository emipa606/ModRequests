using System;
using RimWorld;
using RimWorldRealFoW;
using UnityEngine;
using Verse;
using RimWorldRealFoW.Utils;

namespace RimWorldRealFoW.Detours
{
	// Token: 0x0200002A RID: 42
	internal static class _Verb
	{
		// Token: 0x0600008D RID: 141 RVA: 0x000093E0 File Offset: 0x000075E0
		private static void CanHitCellFromCellIgnoringRange_Postfix(this Verb __instance, ref bool __result, IntVec3 sourceSq, IntVec3 targetLoc, bool includeCorners = false)
		{
			bool flag = __result && __instance.verbProps.requireLineOfSight;
			if (flag)
			{
				__result = ((__instance.caster.Faction != null && _Verb.SeenByFaction(__instance.caster, targetLoc)) || _Verb.fovLineOfSight(sourceSq, targetLoc, __instance.caster));
			}
		}

		// Token: 0x0600008E RID: 142 RVA: 0x00009434 File Offset: 0x00007634
		private static bool SeenByFaction(Thing thing, IntVec3 targetLoc)
		{
			MapComponentSeenFog mapComponentSeenFog = thing.Map.getMapComponentSeenFog();
			bool flag = mapComponentSeenFog != null;
			return !flag || mapComponentSeenFog.isShown(thing.Faction, targetLoc);
		}

		// Token: 0x0600008F RID: 143 RVA: 0x0000946C File Offset: 0x0000766C
		private static bool fovLineOfSight(IntVec3 sourceSq, IntVec3 targetLoc, Thing thing)
		{
			CompMannable compMannable = thing.TryGetComp<CompMannable>();
			bool flag = compMannable != null;
			if (flag)
			{
				thing = compMannable.ManningPawn;
				sourceSq += thing.Position - thing.InteractionCell;
			}
			bool flag2 = !(thing is Pawn);
			bool result;
			if (flag2)
			{
				result = true;
			}
			else
			{
				MapComponentSeenFog mapComponentSeenFog = thing.Map.getMapComponentSeenFog();
				CompMainComponent compMainComponent = (CompMainComponent)thing.TryGetComp(CompMainComponent.COMP_DEF);
				CompFieldOfViewWatcher compFieldOfViewWatcher = compMainComponent.compFieldOfViewWatcher;
				int num = Mathf.RoundToInt(compFieldOfViewWatcher.CalcPawnSightRange(sourceSq, true, !thing.Position.AdjacentToCardinal(sourceSq)));
				bool flag3 = !sourceSq.InHorDistOf(targetLoc, (float)num);
				if (flag3)
				{
					result = false;
				}
				else
				{
					IntVec3 intVec = targetLoc - sourceSq;
					bool flag4 = intVec.x >= 0;
					byte specificOctant;
					if (flag4)
					{
						bool flag5 = intVec.z >= 0;
						if (flag5)
						{
							bool flag6 = intVec.x >= intVec.z;
							if (flag6)
							{
								specificOctant = 0;
							}
							else
							{
								specificOctant = 1;
							}
						}
						else
						{
							bool flag7 = intVec.x >= -intVec.z;
							if (flag7)
							{
								specificOctant = 7;
							}
							else
							{
								specificOctant = 6;
							}
						}
					}
					else
					{
						bool flag8 = intVec.z >= 0;
						if (flag8)
						{
							bool flag9 = -intVec.x >= intVec.z;
							if (flag9)
							{
								specificOctant = 3;
							}
							else
							{
								specificOctant = 2;
							}
						}
						else
						{
							bool flag10 = -intVec.x >= -intVec.z;
							if (flag10)
							{
								specificOctant = 4;
							}
							else
							{
								specificOctant = 5;
							}
						}
					}
					Map map = thing.Map;
					bool[] array = new bool[1];
					ShadowCaster.computeFieldOfViewWithShadowCasting(sourceSq.x, sourceSq.z, num, mapComponentSeenFog.viewBlockerCells, map.Size.x, map.Size.z, false, null, null, null, array, 0, 0, 0, null, 0, 0, 0, 0, 0, specificOctant, targetLoc.x, targetLoc.z);
					result = array[0];
				}
			}
			return result;
		}
	}
}
