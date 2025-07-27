using System;
using System.Collections.Generic;
using Verse;

namespace CM_Callouts.PendingCallouts
{
	// Token: 0x02000019 RID: 25
	public abstract class PendingCalloutEvent
	{
		// Token: 0x06000035 RID: 53 RVA: 0x0000372E File Offset: 0x0000192E
		public PendingCalloutEvent(CalloutCategory _category)
		{
			this.category = _category;
		}

		// Token: 0x06000036 RID: 54 RVA: 0x00003763 File Offset: 0x00001963
		public void FillBodyPartInfo(BodyDef _body, List<BodyPartRecord> _bodyPartsDamaged, List<bool> _bodyPartsDestroyed)
		{
			this.body = _body;
			this.bodyPartsDamaged = _bodyPartsDamaged;
			this.bodyPartsDestroyed = _bodyPartsDestroyed;
		}

		// Token: 0x06000037 RID: 55 RVA: 0x0000377B File Offset: 0x0000197B
		public virtual void AttemptCallout()
		{
			Logger.MessageFormat(this, "Attempting callout.", Array.Empty<object>());
		}

		// Token: 0x0400004A RID: 74
		public CalloutCategory category = CalloutCategory.Undefined;

		// Token: 0x0400004B RID: 75
		public BodyDef body = null;

		// Token: 0x0400004C RID: 76
		public List<BodyPartRecord> bodyPartsDamaged = new List<BodyPartRecord>();

		// Token: 0x0400004D RID: 77
		public List<bool> bodyPartsDestroyed = new List<bool>();
	}
}
