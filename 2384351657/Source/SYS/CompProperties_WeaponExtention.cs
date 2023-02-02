using System;
using Verse;

namespace SYS
{
	// Token: 0x02000009 RID: 9
	public class CompProperties_WeaponExtention : CompProperties
	{
		// Token: 0x0600000E RID: 14 RVA: 0x00002BBA File Offset: 0x00000DBA
		public CompProperties_WeaponExtention()
		{
			this.compClass = typeof(CompWeaponExtention);
		}

		// Token: 0x04000019 RID: 25
		public Offset northOffset;

		// Token: 0x0400001A RID: 26
		public Offset eastOffset;

		// Token: 0x0400001B RID: 27
		public Offset southOffset;

		// Token: 0x0400001C RID: 28
		public Offset westOffset;

		// Token: 0x0400001D RID: 29
		public bool littleDown = false;
	}
}
