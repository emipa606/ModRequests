using System;
using Verse;

namespace CM_Callouts
{
	// Token: 0x02000016 RID: 22
	public class TextMoteQueueTickBased : TextMoteQueue
	{
		// Token: 0x0600002F RID: 47 RVA: 0x000034D7 File Offset: 0x000016D7
		public TextMoteQueueTickBased(IntVec3 aPosition, Map aMap) : base(aPosition, aMap)
		{
		}

		// Token: 0x06000030 RID: 48 RVA: 0x000034EC File Offset: 0x000016EC
		public override void AddMote(MoteText newMote)
		{
			newMote.def = CalloutDefOf.CM_Callouts_Thing_Mote_Text_Ticked;
			int ticksGame = Find.TickManager.TicksGame;
			bool flag = this.queuedMotes.Count == 0 && ticksGame > this.lastReleaseTick + 30 && base.Valid();
			if (flag)
			{
				this.lastReleaseTick = ticksGame;
				GenSpawn.Spawn(newMote, this.position, this.map, WipeMode.Vanish);
			}
			else
			{
				this.queuedMotes.Add(newMote);
			}
		}

		// Token: 0x06000031 RID: 49 RVA: 0x00003564 File Offset: 0x00001764
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
				int ticksGame = Find.TickManager.TicksGame;
				bool flag2 = ticksGame > this.lastReleaseTick + 30;
				if (flag2)
				{
					this.lastReleaseTick = ticksGame;
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

		// Token: 0x04000041 RID: 65
		public const int delayTicks = 30;

		// Token: 0x04000042 RID: 66
		public int lastReleaseTick = -1;
	}
}
