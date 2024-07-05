using System;
using System.Collections.Generic;
using RimWorldRealFoW;
using Verse;

namespace RimWorldRealFoW.Utils
{
	// Token: 0x02000009 RID: 9
	public static class FoWThingUtils
	{
		// Token: 0x0600002E RID: 46 RVA: 0x00005228 File Offset: 0x00003428
		public static IntVec3[] getPeekArray(IntVec3 intVec3)
		{

			IntVec3[] result;
			if (FoWThingUtils.peekArrayCache.ContainsKey(intVec3))
			{
				result = FoWThingUtils.peekArrayCache[intVec3];
			}
			else
			{
				IntVec3[] array = new IntVec3[]
				{
					intVec3
				};
				FoWThingUtils.peekArrayCache[intVec3] = array;
				result = array;
			}
			return result;
		}

		// Token: 0x0600002F RID: 47 RVA: 0x00005278 File Offset: 0x00003478
		public static bool fowIsVisible(this Thing _this, bool forRender = false)
		{
			bool result;
			if (_this.Spawned)
			{
				if (_this.def.isSaveable && !_this.def.saveCompressible)
				{
					CompHiddenable compHiddenable = _this.TryGetCompHiddenable();
					if (compHiddenable != null)
					{
						return !compHiddenable.hidden;
					}
				}
				result = (forRender || (_this.Map != null && _this.fowInKnownCell()));
			}
			else
			{
				result = true;
			}
			return result;
		}

		// Token: 0x06000030 RID: 48 RVA: 0x000052F4 File Offset: 0x000034F4
		private static bool fowInKnownCell(this Thing _this)
		{
			MapComponentSeenFog mapComponentSeenFog = _this.Map.getMapComponentSeenFog();
			bool result;
			if (mapComponentSeenFog != null)
			{
				bool[] knownCells = mapComponentSeenFog.knownCells;
				int mapSizeX = mapComponentSeenFog.mapSizeX;
				IntVec3 position = _this.Position;
				IntVec2 size = _this.def.size;
				if (size.x == 1 && size.z == 1)
				{
					result = mapComponentSeenFog.knownCells[position.z * mapSizeX + position.x];
				}
				else
				{
					CellRect cellRect = GenAdj.OccupiedRect(position, _this.Rotation, size);
					for (int i = cellRect.minX; i <= cellRect.maxX; i++)
					{
						for (int j = cellRect.minZ; j <= cellRect.maxZ; j++)
						{
							if (mapComponentSeenFog.knownCells[j * mapSizeX + i])
							{
								return true;
							}
						}
					}
					result = false;
				}
			}
			else
			{
				result = true;
			}
			return result;
		}

		// Token: 0x06000031 RID: 49 RVA: 0x00005404 File Offset: 0x00003604
		public static ThingComp TryGetComp(this Thing _this, CompProperties def)
		{
			ThingCategory category = _this.def.category;
;
			if (category == ThingCategory.Pawn
				|| category == ThingCategory.Building
				|| category == ThingCategory.Item
				|| category == ThingCategory.Filth
				|| category == ThingCategory.Gas
				|| _this.def.IsBlueprint)
			{
				ThingWithComps thingWithComps = _this as ThingWithComps;
				if (thingWithComps != null)
				{
					return thingWithComps.GetCompByDef(def);
				}
			}
			return null;
		}

		// Token: 0x06000032 RID: 50 RVA: 0x00005468 File Offset: 0x00003668
		public static CompHiddenable TryGetCompHiddenable(this Thing _this)
		{
			CompMainComponent compMainComponent = (CompMainComponent)_this.TryGetComp(CompMainComponent.COMP_DEF);
			CompHiddenable result;
			if (compMainComponent != null)
			{
				result = compMainComponent.compHiddenable;
			}
			else
			{
				result = null;
			}
			return result;
		}

		// Token: 0x04000029 RID: 41
		private static readonly Dictionary<IntVec3, IntVec3[]> peekArrayCache = new Dictionary<IntVec3, IntVec3[]>(15);
	}
}
