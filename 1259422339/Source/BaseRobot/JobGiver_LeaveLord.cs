using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace BaseRobot
{
    // Token: 0x02000007 RID: 7
    public class JobGiver_LeaveLord : ThinkNode_JobGiver
	{
		// Token: 0x06000029 RID: 41 RVA: 0x00003984 File Offset: 0x00001B84
		public override float GetPriority(Pawn pawn)
		{
			return (pawn.GetLord() == null) ? 0 : 9;
		}

		// Token: 0x0600002A RID: 42 RVA: 0x0000399C File Offset: 0x00001B9C
		protected override Job TryGiveJob(Pawn pawn)
		{
			Lord lord = pawn.GetLord();
			if (lord != null)
			{
				lord.Notify_PawnLost(pawn, PawnLostCondition.LeftVoluntarily, null);
			}
			return null;
		}
	}
}
