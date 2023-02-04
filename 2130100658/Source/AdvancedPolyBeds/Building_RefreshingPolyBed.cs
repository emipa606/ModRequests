using System;
using RimWorld;
using Verse;
using MFSpacer;

namespace AdvancedPolyBeds
{
    public class Building_RefreshingPolyBed : Building_Bed
    {
		public override void Tick()
		{
			for (int i = 0; i < base.SleepingSlotsCount; i++)
			{
				bool flag = base.GetCurOccupant(i) != null;
				if (flag)
				{
					Pawn curOccupant = base.GetCurOccupant(i);
					this.TicksCounted[i]++;
					bool flag2 = this.TicksCounted[i] == 1250 && PawnUtility.GetPosture(curOccupant) == PawnPosture.LayingInBed;
					if (flag2)
					{
						Hediff firstHediffOfDef = curOccupant.health.hediffSet.GetFirstHediffOfDef(MFSpacer.HediffDefOf.Bed_RefreshingSleep, false);
						bool flag3 = firstHediffOfDef == null;
						if (flag3)
						{
							curOccupant.health.AddHediff(MFSpacer.HediffDefOf.Bed_RefreshingSleep, null, null, null);
						}
						else
						{
							firstHediffOfDef.Severity += 0.05f;
						}
						this.TicksCounted[i] = 0;
					}
				}
			}
			base.Tick();
		}

		private int[] TicksCounted = new int[5];
    }
}
