using System;
using Verse;

namespace Crystosentient.CrystalTerrain
{
	// Token: 0x02000013 RID: 19
	public class CompPropertiesCrystalFloor_Glower : CompPropertiesCrystalFloor
	{
		// Token: 0x06000068 RID: 104 RVA: 0x00003EB0 File Offset: 0x000020B0
		public CompPropertiesCrystalFloor_Glower()
		{
			this.compClass = typeof(CompCrystalFloor_Glower);
		}

		// Token: 0x04000030 RID: 48
		public float overlightRadius;

		// Token: 0x04000031 RID: 49
		public float glowRadius = 14f;

		// Token: 0x04000032 RID: 50
		public ColorInt glowColor = new ColorInt(255, 255, 255, 0) * 0.2f;

		// Token: 0x04000033 RID: 51
		public bool powered = true;
	}
}
