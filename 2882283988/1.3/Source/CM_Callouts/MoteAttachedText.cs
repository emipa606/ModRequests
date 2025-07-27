using System;
using UnityEngine;
using Verse;

namespace CM_Callouts
{
	// Token: 0x0200000C RID: 12
	public class MoteAttachedText : MoteAttached
	{
		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000026 RID: 38 RVA: 0x0000335C File Offset: 0x0000155C
		protected float TimeBeforeStartFadeout
		{
			get
			{
				bool flag = this.overrideTimeBeforeStartFadeout < 0f;
				float solidTime;
				if (flag)
				{
					solidTime = base.SolidTime;
				}
				else
				{
					solidTime = this.overrideTimeBeforeStartFadeout;
				}
				return solidTime;
			}
		}

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x06000027 RID: 39 RVA: 0x0000338F File Offset: 0x0000158F
		protected override bool EndOfLife
		{
			get
			{
				return base.AgeSecs >= this.TimeBeforeStartFadeout + this.def.mote.fadeOutTime;
			}
		}

		// Token: 0x06000028 RID: 40 RVA: 0x000033B3 File Offset: 0x000015B3
		public override void Draw()
		{
		}

		// Token: 0x06000029 RID: 41 RVA: 0x000033B8 File Offset: 0x000015B8
		public override void DrawGUIOverlay()
		{
			float a = 1f - (base.AgeSecs - this.TimeBeforeStartFadeout) / this.def.mote.fadeOutTime;
			CalloutUtility.DrawText(new Vector2(this.exactPosition.x, this.exactPosition.z), this.text, new Color(this.textColor.r, this.textColor.g, this.textColor.b, a));
		}

		// Token: 0x0400003B RID: 59
		public string text;

		// Token: 0x0400003C RID: 60
		public Color textColor = Color.white;

		// Token: 0x0400003D RID: 61
		public float overrideTimeBeforeStartFadeout = -1f;
	}
}
