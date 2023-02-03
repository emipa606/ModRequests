using RimWorld;
using UnityEngine;
using Verse;

namespace BaseRobot
{
    // Token: 0x02000018 RID: 24
    public class PawnColumnWorker_RobotArea : PawnColumnWorker_AllowedArea
	{
		// Token: 0x06000076 RID: 118 RVA: 0x00005A7E File Offset: 0x00003C7E
		public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
		{
			AreaAllowedGUI.DoAllowedAreaSelectors(rect, pawn);
		}
	}
}
