using System;
using HarmonyLib;
using RimWorldRealFoW;
using Verse;

namespace RimWorldRealFoW.Detours
{
	// Token: 0x02000016 RID: 22
	public static class _RoofGrid
	{
		// Token: 0x06000075 RID: 117 RVA: 0x00008B0C File Offset: 0x00006D0C
		public static void GetCellBool_Postfix(int index, ref TerrainGrid __instance, ref bool __result)
		{
			if (__result)
			{
				Map value = Traverse.Create(__instance).Field("map").GetValue<Map>();
				MapComponentSeenFog mapComponentSeenFog = value.getMapComponentSeenFog();
				if (mapComponentSeenFog != null)
				{
					__result = mapComponentSeenFog.knownCells[index];
				}
			}
		}
	}
}
