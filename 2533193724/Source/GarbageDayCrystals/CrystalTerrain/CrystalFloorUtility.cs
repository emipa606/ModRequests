using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace Crystosentient.CrystalTerrain
{
	// Token: 0x0200000C RID: 12
	[StaticConstructorOnStartup]
	public static class CrystalFloorUtility
	{
		// Token: 0x06000042 RID: 66 RVA: 0x00003914 File Offset: 0x00001B14
		public static int HashCodeToMod(this object obj, int mod)
		{
			return Math.Abs(obj.GetHashCode()) % mod;
		}



		// Token: 0x06000047 RID: 71 RVA: 0x00003ADC File Offset: 0x00001CDC
		public static CrystalFloor MakeTerrainInstance(this CrystalFloorDef tDef, Map map, IntVec3 loc)
		{
			CrystalFloor crystalFloor = (CrystalFloor)Activator.CreateInstance(tDef.terrainInstanceClass);
			crystalFloor.def = tDef;
			crystalFloor.Map = map;
			crystalFloor.Position = loc;
			return crystalFloor;
		}

	}
}
