using System;
using Verse;

namespace SYS
{
	// Token: 0x02000007 RID: 7
	public class CompSheath : ThingComp
	{
		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000008 RID: 8 RVA: 0x00002A90 File Offset: 0x00000C90
		public virtual Graphic FullGraphic
		{
			get
			{
				var flag = this.fullGraphicInt == null;
				if (flag)
				{
					var flag2 = this.Props.fullGraphicData == null;
					if (flag2)
					{
						return this.parent.Graphic;
					}
					this.fullGraphicInt = this.Props.fullGraphicData.GraphicColoredFor(this.parent);
				}
				return this.fullGraphicInt;
			}
		}

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x06000009 RID: 9 RVA: 0x00002AF4 File Offset: 0x00000CF4
		public virtual Graphic SheathOnlyGraphic
		{
			get
			{
				var flag = this.sheathOnlyGraphicInt == null;
				if (flag)
				{
					var flag2 = this.Props.sheathOnlyGraphicData == null;
					if (flag2)
					{
						this.sheathOnlyGraphicInt = CompProperties_Sheath.NullData;
						return this.sheathOnlyGraphicInt;
					}
					this.sheathOnlyGraphicInt = this.Props.sheathOnlyGraphicData.GraphicColoredFor(this.parent);
				}
				return this.sheathOnlyGraphicInt;
			}
		}

		// Token: 0x0600000A RID: 10 RVA: 0x00002B5E File Offset: 0x00000D5E
		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
			this.Props = (CompProperties_Sheath)this.props;
		}

		// Token: 0x0600000B RID: 11 RVA: 0x00002B7A File Offset: 0x00000D7A
		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			this.Props = (CompProperties_Sheath)this.props;
		}

		// Token: 0x0600000C RID: 12 RVA: 0x00002B96 File Offset: 0x00000D96
		public override void PostExposeData()
		{
			base.PostExposeData();
			this.Props = (CompProperties_Sheath)this.props;
		}

		// Token: 0x04000012 RID: 18
		private Graphic fullGraphicInt;

		// Token: 0x04000013 RID: 19
		private Graphic sheathOnlyGraphicInt;

		// Token: 0x04000014 RID: 20
		public CompProperties_Sheath Props;
	}
}
