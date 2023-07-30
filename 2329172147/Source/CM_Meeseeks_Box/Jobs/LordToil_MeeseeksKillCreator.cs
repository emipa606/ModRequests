using System.Linq;

using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace CM_Meeseeks_Box
{
    public class LordToil_MeeseeksKillCreator : LordToil
    {
        public override void Init()
        {
            base.Init();
        }

        public override void UpdateAllDuties()
        {
            foreach(Pawn ownedPawn in lord.ownedPawns)
            {
                if (ownedPawn.mindState.duty == null || ownedPawn.mindState.duty.def != MeeseeksDefOf.CM_Meeseeks_Box_Duty_Kill_Creator)
                {
                    ownedPawn.mindState.duty = new PawnDuty(MeeseeksDefOf.CM_Meeseeks_Box_Duty_Kill_Creator);
                    ownedPawn.mindState.duty.locomotion = LocomotionUrgency.Jog;
                    Logger.MessageFormat(this, "Adding Kill Creator duty to {0}", ownedPawn);
                    if (ownedPawn.jobs.curJob != null)
                    {
                        Logger.MessageFormat(this, "Ending job {0} on {1}", ownedPawn.jobs.curJob, ownedPawn);
                        ownedPawn.jobs.EndCurrentJob(JobCondition.InterruptForced, false);
                    }
                }
            }
        }
    }

}
