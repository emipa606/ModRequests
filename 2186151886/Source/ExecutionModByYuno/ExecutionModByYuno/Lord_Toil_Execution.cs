using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI.Group;
using Verse.AI;
using RimWorld;
using Verse;

namespace ExecutionModByYuno
{
  public   class Lord_Toil_Execution : LordToil
    {

     
        Building_SpotExecution spotE;




        public override void Init()
        {
            base.Init();
        }


        public override ThinkTreeDutyHook VoluntaryJoinDutyHookFor(Pawn p)
        {
            return DutyDefExecution.WatchExecution.hook;
        }


        public Lord_Toil_Execution(IntVec3 spot, Building_SpotExecution executionSpot)
        {
            this.spotE = executionSpot;
        }

        private CellRect CalculateSpectateRect()
        {
            return CellRect.CenteredOn(this.spotE.Position, spotE.WatchRadius-1);
        }


        public override void UpdateAllDuties()
        {
            for (int index = 0; index < this.lord.ownedPawns.Count; index++)
            {
                Pawn ownedPawn = this.lord.ownedPawns[index];
                ownedPawn.mindState.duty = new PawnDuty(DutyDefExecution.WatchExecution)
                {
                    spectateRect = this.CalculateSpectateRect(),
                    focus = this.spotE
                };
            }
        }
    }
}
