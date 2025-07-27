using System;
using System.Collections.Generic;
using Verse;

namespace CM_Callouts
{
	// Token: 0x02000015 RID: 21
	public abstract class TextMoteQueue
	{
		// Token: 0x0600002B RID: 43 RVA: 0x00003458 File Offset: 0x00001658
		public TextMoteQueue(IntVec3 aPosition, Map aMap)
		{
			this.position = aPosition;
			this.map = aMap;
		}

		// Token: 0x0600002C RID: 44 RVA: 0x00003490 File Offset: 0x00001690
		public bool Valid()
		{
			return this.position != IntVec3.Invalid && this.map != null && !this.map.info.parent.Destroyed;
		}

		// Token: 0x0600002D RID: 45
		public abstract void AddMote(MoteText newMote);

		// Token: 0x0600002E RID: 46
		public abstract bool Update();

		// Token: 0x0400003E RID: 62
		public IntVec3 position = IntVec3.Invalid;

		// Token: 0x0400003F RID: 63
		public Map map = null;

		// Token: 0x04000040 RID: 64
		public List<MoteText> queuedMotes = new List<MoteText>();
	}
}
