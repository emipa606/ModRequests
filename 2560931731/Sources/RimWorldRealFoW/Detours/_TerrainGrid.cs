using System;
using HarmonyLib;
using RimWorldRealFoW;
using Verse;

namespace RimWorldRealFoW.Detours
{
	// Token: 0x02000017 RID: 23
	public static class _TerrainGrid
	{
		// Token: 0x06000076 RID: 118 RVA: 0x00008B54 File Offset: 0x00006D54
		public static void CellBoolDrawerGetBoolInt_Postfix(int index, ref TerrainGrid __instance, ref bool __result)
		{
			bool flag = __result;
			if (flag)
			{
				Map value = Traverse.Create(__instance).Field("map").GetValue<Map>();
				MapComponentSeenFog mapComponentSeenFog = value.getMapComponentSeenFog();
				bool flag2 = mapComponentSeenFog != null;
				if (flag2)
				{
					__result = mapComponentSeenFog.knownCells[index];
				}
			}
		}
	}
}
