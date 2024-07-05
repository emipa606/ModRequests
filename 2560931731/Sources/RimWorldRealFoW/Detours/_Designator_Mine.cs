using System;
using HarmonyLib;
using RimWorld;
using RimWorldRealFoW;
using Verse;

namespace RimWorldRealFoW.Detours
{
	// Token: 0x0200001E RID: 30
	public static class _Designator_Mine
	{
		// Token: 0x0600007E RID: 126 RVA: 0x00008EA4 File Offset: 0x000070A4
		public static void CanDesignateCell_Postfix(IntVec3 c, ref Designator __instance, ref AcceptanceReport __result)
		{
			if (!__result.Accepted)
			{
				Map value = Traverse.Create(__instance).Property("Map", null).GetValue<Map>();
				if (value.designationManager.DesignationAt(c, DesignationDefOf.Mine) == null)
				{
					MapComponentSeenFog mapComponentSeenFog = value.getMapComponentSeenFog();
					if (mapComponentSeenFog != null && c.InBounds(value) && !mapComponentSeenFog.knownCells[value.cellIndices.CellToIndex(c)])
					{
						__result = true;
					}
				}
			}
		}
	}
}
