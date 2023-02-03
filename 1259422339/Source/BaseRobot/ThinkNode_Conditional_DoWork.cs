using Verse;
using Verse.AI;

namespace BaseRobot
{
    // Token: 0x0200000E RID: 14
    public class ThinkNode_Conditional_DoWork : ThinkNode_Conditional
	{
		// Token: 0x0600003B RID: 59 RVA: 0x000044B8 File Offset: 0x000026B8
		public override ThinkNode DeepCopy(bool resolve = true)
		{
			var thinkNode_Conditional_DoWork = (ThinkNode_Conditional_DoWork)base.DeepCopy(resolve);
			thinkNode_Conditional_DoWork.workType = workType;
			return thinkNode_Conditional_DoWork;
		}

		// Token: 0x0600003C RID: 60 RVA: 0x000044DF File Offset: 0x000026DF
		public override float GetPriority(Pawn pawn)
		{
			return priority;
		}

		// Token: 0x0600003D RID: 61 RVA: 0x000044E8 File Offset: 0x000026E8
		protected override bool Satisfied(Pawn pawn)
		{
			var flag = workType == null;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				var arcBaseRobot = pawn as ArcBaseRobot;
				var flag2 = arcBaseRobot == null;
				if (flag2)
				{
					result = false;
				}
				else
				{
                    result = arcBaseRobot.def is ThingDef_BaseRobot thingDef_BaseRobot && arcBaseRobot.CanDoWorkType(workType);
                }
			}
			return result;
		}

		// Token: 0x04000030 RID: 48
		private WorkTypeDef workType;
	}
}
