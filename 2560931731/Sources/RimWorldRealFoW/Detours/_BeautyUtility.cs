using System;
using RimWorld;
using RimWorldRealFoW;
using Verse;

namespace RimWorldRealFoW.Detours
{
	// Token: 0x0200001B RID: 27
	public static class _BeautyUtility
	{
		// Token: 0x0600007B RID: 123 RVA: 0x00008CE4 File Offset: 0x00006EE4
		public static void FillBeautyRelevantCells_Postfix(Map map)
		{
			MapComponentSeenFog mapCmq = map.getMapComponentSeenFog();
			if (mapCmq != null)
			{
				BeautyUtility.beautyRelevantCells.RemoveAll((IntVec3 c) => !mapCmq.knownCells[map.cellIndices.CellToIndex(c)]);
			}
		}
	}
}
