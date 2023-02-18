using System;

namespace Crystosentient.CrystalTerrain
{
	// Token: 0x02000011 RID: 17
	public class CompCrystalFloor
	{
		// Token: 0x0600005D RID: 93 RVA: 0x000023FB File Offset: 0x000005FB
		public virtual void Initialize(CompPropertiesCrystalFloor props)
		{
			this.props = props;
		}

		// Token: 0x0600005E RID: 94 RVA: 0x00003E9C File Offset: 0x0000209C
		public virtual string TransformLabel(string label)
		{
			return label;
		}

		// Token: 0x0600005F RID: 95 RVA: 0x00002405 File Offset: 0x00000605
		public virtual void CompTick()
		{
		}

		// Token: 0x06000060 RID: 96 RVA: 0x00002405 File Offset: 0x00000605
		public virtual void CompUpdate()
		{
		}

		// Token: 0x06000061 RID: 97 RVA: 0x00002405 File Offset: 0x00000605
		public virtual void PlaceSetup()
		{
		}

		// Token: 0x06000062 RID: 98 RVA: 0x00002405 File Offset: 0x00000605
		public virtual void PostRemove()
		{
		}

		// Token: 0x06000063 RID: 99 RVA: 0x00002405 File Offset: 0x00000605
		public virtual void PostExposeData()
		{
		}

		// Token: 0x06000064 RID: 100 RVA: 0x00002405 File Offset: 0x00000605
		public virtual void ReceiveCompSignal(string sig)
		{
		}

		// Token: 0x06000065 RID: 101 RVA: 0x00002405 File Offset: 0x00000605
		public virtual void PostPostLoad()
		{
		}

		// Token: 0x0400002D RID: 45
		public CrystalFloor parent;

		// Token: 0x0400002E RID: 46
		public CompPropertiesCrystalFloor props;
	}
}
