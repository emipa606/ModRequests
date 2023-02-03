using RimWorld;
using UnityEngine;
using Verse;

namespace BaseRobot
{
    // Token: 0x02000015 RID: 21
    public class PawnColumnWorker_Rename : PawnColumnWorker
	{
        // Token: 0x17000005 RID: 5
        // (get) Token: 0x06000062 RID: 98 RVA: 0x0000545C File Offset: 0x0000365C
        protected override GameFont DefaultHeaderFont => GameFont.Tiny;

        // Token: 0x06000063 RID: 99 RVA: 0x0000545F File Offset: 0x0000365F
        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
		{
			if (Widgets.ButtonText(rect, "Rename", true, false, true))
			{
				Find.WindowStack.Add(new Dialog_ChangeLabel(pawn));
			}
		}

		// Token: 0x06000064 RID: 100 RVA: 0x00005484 File Offset: 0x00003684
		public override int GetMinWidth(PawnTable table)
		{
			return Mathf.Max(base.GetMinWidth(table), 100);
		}

		// Token: 0x06000065 RID: 101 RVA: 0x00005494 File Offset: 0x00003694
		public override int GetOptimalWidth(PawnTable table)
		{
			return Mathf.Clamp(170, GetMinWidth(table), GetMaxWidth(table));
		}
	}
}
