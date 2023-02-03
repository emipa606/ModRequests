using RimWorld;
using Verse;
using Verse.AI;

namespace BaseRobot
{
    // Token: 0x02000008 RID: 8
    public class JobGiver_RechargeEnergy : ThinkNode_JobGiver
	{
		// Token: 0x0600002C RID: 44 RVA: 0x000039D0 File Offset: 0x00001BD0
		public override float GetPriority(Pawn pawn)
		{
			Need_Battery need_Battery = pawn.needs.TryGetNeed<Need_Battery>();
			float result;
			if (need_Battery == null)
			{
				result = 0f;
			}
			else
			{
				var curLevel = need_Battery.CurLevel;
				TimeAssignmentDef timeAssignmentDef = (pawn.timetable == null) ? TimeAssignmentDefOf.Anything : pawn.timetable.CurrentAssignment;
				if (timeAssignmentDef == TimeAssignmentDefOf.Anything)
				{
					if ((curLevel < 0.5 && pawn is ArcBaseRobot) & !BaseRobot_Helper.IsInDistance(pawn.Position, (pawn as ArcBaseRobot).rechargeStation.Position, 50f))
					{
						result = 8f;
					}
					else
					{
						result = (curLevel >= 0.3) ? 0 : 8;
					}
				}
				else if (timeAssignmentDef == TimeAssignmentDefOf.Work)
				{
					result = 0f;
				}
				else if (timeAssignmentDef == TimeAssignmentDefOf.Joy)
				{
					result = (curLevel >= 0.3) ? 0 : 8;
				}
				else if (timeAssignmentDef == TimeAssignmentDefOf.Sleep)
				{
					result = (curLevel >= 0.75) ? 0 : 8;
				}
				else
				{
					result = 0f;
				}
			}
			return result;
		}

		// Token: 0x0600002D RID: 45 RVA: 0x00003B0C File Offset: 0x00001D0C
		protected override Job TryGiveJob(Pawn pawn)
		{
			var arcBaseRobot = pawn as ArcBaseRobot;
			Building_BaseRobotRechargeStation building_BaseRobotRechargeStation = BaseRobot_Helper.FindRechargeStationFor(arcBaseRobot);
			Job result;
			if (building_BaseRobotRechargeStation == null)
			{
				result = null;
			}
			else if (arcBaseRobot.rechargeStation != building_BaseRobotRechargeStation)
			{
				result = null;
			}
			else
			{
				var job = new Job(DefDatabase<JobDef>.GetNamed("AIRobot_GoRecharge", true), building_BaseRobotRechargeStation);
				result = job;
			}
			return result;
		}
	}
}
