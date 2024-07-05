using System;
using RimWorldRealFoW;
using Verse;

namespace RimWorldRealFoW.Detours
{
	// Token: 0x02000020 RID: 32
	public static class _EnvironmentStatsDrawer
	{
		// Token: 0x06000081 RID: 129 RVA: 0x00008FE8 File Offset: 0x000071E8
		public static void ShouldShowWindowNow_Postfix(ref bool __result)
		{
			if (__result)
			{
				Map currentMap = Find.CurrentMap;
				MapComponentSeenFog mapComponentSeenFog = currentMap.getMapComponentSeenFog();
				__result = (mapComponentSeenFog == null || mapComponentSeenFog.knownCells[currentMap.cellIndices.CellToIndex(UI.MouseCell())]);
			}
		}
	}
}
