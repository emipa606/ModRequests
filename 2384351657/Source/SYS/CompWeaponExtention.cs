using System;
using Verse;

namespace SYS
{
	// Token: 0x0200000A RID: 10
	public class CompWeaponExtention : ThingComp
	{
		// Token: 0x0600000F RID: 15 RVA: 0x00002BDC File Offset: 0x00000DDC
		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			this.Props = (CompProperties_WeaponExtention)this.props;
			var flag = this.Props.littleDown;
			if (flag)
			{
				this.littleDown = true;
			}
		}

		// Token: 0x06000010 RID: 16 RVA: 0x00002C1B File Offset: 0x00000E1B
		public override void PostExposeData()
		{
			base.PostExposeData();
			this.Props = (CompProperties_WeaponExtention)this.props;
		}

		// Token: 0x06000011 RID: 17 RVA: 0x00002C38 File Offset: 0x00000E38
		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
			this.Props = (CompProperties_WeaponExtention)props;
			var flag = this.Props.littleDown;
			if (flag)
			{
				this.littleDown = true;
			}
		}

		// Token: 0x0400001E RID: 30
		public bool littleDown = false;

		// Token: 0x0400001F RID: 31
		public CompProperties_WeaponExtention Props;
	}
}
