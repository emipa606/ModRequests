using System;
using UnityEngine;
using Verse;

namespace CM_Callouts
{
	// Token: 0x02000017 RID: 23
	public class TextMoteQueueRealTime : TextMoteQueue
	{
		// Token: 0x06000032 RID: 50 RVA: 0x000035F5 File Offset: 0x000017F5
		public TextMoteQueueRealTime(IntVec3 aPosition, Map aMap) : base(aPosition, aMap)
		{
		}

		// Token: 0x06000033 RID: 51 RVA: 0x00003618 File Offset: 0x00001818
		public override void AddMote(MoteText newMote)
		{
			bool flag = this.queuedMotes.Count == 0 && this.timeAccumulated > this.lastReleaseTime + 0.5f && base.Valid();
			if (flag)
			{
				this.lastReleaseTime = this.timeAccumulated;
				GenSpawn.Spawn(newMote, this.position, this.map, WipeMode.Vanish);
			}
			else
			{
				this.queuedMotes.Add(newMote);
			}
		}

		// Token: 0x06000034 RID: 52 RVA: 0x00003688 File Offset: 0x00001888
		public override bool Update()
		{
			bool flag = !base.Valid();
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				this.timeAccumulated += Time.deltaTime;
				bool flag2 = this.timeAccumulated > this.lastReleaseTime + 0.5f;
				if (flag2)
				{
					this.lastReleaseTime = this.timeAccumulated;
					bool flag3 = this.queuedMotes.Count > 0;
					if (flag3)
					{
						GenSpawn.Spawn(this.queuedMotes[0], this.position, this.map, WipeMode.Vanish);
						this.queuedMotes.RemoveAt(0);
						result = true;
					}
					else
					{
						result = false;
					}
				}
				else
				{
					result = true;
				}
			}
			return result;
		}

		// Token: 0x04000043 RID: 67
		public const float delayTime = 0.5f;

		// Token: 0x04000044 RID: 68
		public float lastReleaseTime = 0f;

		// Token: 0x04000045 RID: 69
		public float timeAccumulated = 0f;
	}
}
