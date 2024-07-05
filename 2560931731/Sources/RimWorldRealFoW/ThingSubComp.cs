using System;
using Verse;

namespace RimWorldRealFoW
{
	// Token: 0x02000010 RID: 16
	public abstract class ThingSubComp
	{
		// Token: 0x0600004B RID: 75 RVA: 0x000060B8 File Offset: 0x000042B8
		public virtual void CompTick()
		{
		}

		// Token: 0x0600004C RID: 76 RVA: 0x000060B8 File Offset: 0x000042B8
		public virtual void CompTickRare()
		{
		}

		// Token: 0x0600004D RID: 77 RVA: 0x000060B8 File Offset: 0x000042B8
		public virtual void PostDeSpawn(Map map)
		{
		}

		// Token: 0x0600004E RID: 78 RVA: 0x000060B8 File Offset: 0x000042B8
		public virtual void PostExposeData()
		{
		}

		// Token: 0x0600004F RID: 79 RVA: 0x000060B8 File Offset: 0x000042B8
		public virtual void PostSpawnSetup(bool respawningAfterLoad)
		{
		}

		// Token: 0x06000050 RID: 80 RVA: 0x000060B8 File Offset: 0x000042B8
		public virtual void ReceiveCompSignal(string signal)
		{
		}

		// Token: 0x04000043 RID: 67
		public ThingWithComps parent;

		// Token: 0x04000044 RID: 68
		public CompMainComponent mainComponent;
	}
}
