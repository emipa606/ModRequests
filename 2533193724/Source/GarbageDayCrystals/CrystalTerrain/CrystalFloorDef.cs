using System;
using System.Collections.Generic;
using Verse;

namespace Crystosentient.CrystalTerrain
{
	// Token: 0x0200000F RID: 15
	public class CrystalFloorDef : TerrainDef
	{
		// Token: 0x06000051 RID: 81 RVA: 0x00003C04 File Offset: 0x00001E04
		public T GetCompProperties<T>() where T : CompPropertiesCrystalFloor
		{
			for (int i = 0; i < this.terrainComps.Count; i++)
			{
				T result;
				bool flag = (result = (this.terrainComps[i] as T)) != null;
				bool flag2 = flag;
				if (flag2)
				{
					return result;
				}
			}
			return default(T);
		}

		// Token: 0x0400002A RID: 42
		public List<CompPropertiesCrystalFloor> terrainComps = new List<CompPropertiesCrystalFloor>();

		// Token: 0x0400002B RID: 43
		public Type terrainInstanceClass = typeof(CrystalFloor);
	}
}
