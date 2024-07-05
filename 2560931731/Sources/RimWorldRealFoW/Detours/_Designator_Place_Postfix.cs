using System;
using HarmonyLib;
using RimWorldRealFoW;
using Verse;

namespace RimWorldRealFoW.Detours
{
	// Token: 0x0200001D RID: 29
	public static class _Designator_Place_Postfix
	{
		// Token: 0x0600007D RID: 125 RVA: 0x00008DAC File Offset: 0x00006FAC
		public static void CanDesignateCell_Postfix(ref IntVec3 c, ref Designator __instance, ref AcceptanceReport __result)
		{
			bool accepted = __result.Accepted;
			if (accepted)
			{
				Traverse traverse = Traverse.Create(__instance);
				CellRect cellRect = GenAdj.OccupiedRect(c, traverse.Field("placingRot").GetValue<Rot4>(), traverse.Property("PlacingDef", null).GetValue<BuildableDef>().Size);
				Map value = traverse.Property("Map", null).GetValue<Map>();
				MapComponentSeenFog mapComponentSeenFog = value.getMapComponentSeenFog();
				if (mapComponentSeenFog != null)
				{
					foreach (IntVec3 c2 in cellRect)
					{
						if (!mapComponentSeenFog.knownCells[value.cellIndices.CellToIndex(c2)])
						{
							__result = "CannotPlaceInUndiscovered".Translate();
							break;
						}
					}
				}
			}
		}
	}
}
