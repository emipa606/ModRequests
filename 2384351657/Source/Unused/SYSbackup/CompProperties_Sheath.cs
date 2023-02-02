using System;
using Verse;

namespace SYS
{
	// Token: 0x02000006 RID: 6
	public class CompProperties_Sheath : CompProperties
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000006 RID: 6 RVA: 0x00002A08 File Offset: 0x00000C08
		public static Graphic NullData
		{
			get
			{
				var flag = CompProperties_Sheath.nullData == null;
				if (flag)
				{
                    CompProperties_Sheath.nullData = new GraphicData
                    {
                        graphicClass = typeof(Graphic_Single),
                        texPath = "Things/NullDraw"
                    };
                }
				return CompProperties_Sheath.nullData.Graphic;
			}
		}

		// Token: 0x06000007 RID: 7 RVA: 0x00002A5F File Offset: 0x00000C5F
		public CompProperties_Sheath()
		{
			this.compClass = typeof(CompSheath);
		}

		// Token: 0x0400000A RID: 10
		private static GraphicData nullData;

		// Token: 0x0400000B RID: 11
		public GraphicData sheathOnlyGraphicData = null;

		// Token: 0x0400000C RID: 12
		public GraphicData fullGraphicData = null;

		// Token: 0x0400000D RID: 13
		public DrawPosition drawPosition = DrawPosition.None;

		// Token: 0x0400000E RID: 14
		public Offset northOffset;

		// Token: 0x0400000F RID: 15
		public Offset eastOffset;

		// Token: 0x04000010 RID: 16
		public Offset southOffset;

		// Token: 0x04000011 RID: 17
		public Offset westOffset;
	}
}
