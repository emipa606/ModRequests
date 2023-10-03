using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using RimWorld;
using Verse.AI.Group;

namespace ExecutionModByYuno
{
    public class Lord_Job_Execution : LordJob_VoluntarilyJoinable
    {

        public IntVec3 spot;


        public Building_SpotExecution SpotE;

        
        public Lord_Job_Execution(IntVec3 spot, Building_SpotExecution ExecutionSpot)
        {
            this.spot = spot;
            SpotE = ExecutionSpot;
        }

        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new StateGraph();
            Lord_Toil_Execution lordToilExecution = new Lord_Toil_Execution(spot, SpotE);
            stateGraph.AddToil(lordToilExecution);
            LordToil_End lordToilEnd = new LordToil_End();
            stateGraph.AddToil(lordToilEnd);
            Transition transition = new Transition(lordToilExecution, lordToilEnd, false, true);
            transition.AddTrigger(new Trigger_TickCondition(() => this.SpotE.currentState == Building_SpotExecution.State.rest, 1));
            transition.AddTrigger(new Trigger_TickCondition(() => this.SpotE.currentState == Building_SpotExecution.State.preparation || this.SpotE.currentState == Building_SpotExecution.State.inMotion , 9000));
            stateGraph.AddTransition(transition, false);
            return stateGraph;
        }

        public override float VoluntaryJoinPriorityFor(Pawn p)
        {
            bool flag = this.IsInvited(p);
            float result;
            if (flag)
            {
                result = VoluntarilyJoinableLordJobJoinPriorities.SocialGathering;
            }
            else
            {
                result = 0f;
            }
            return result;
        }


        public virtual string GetReport()
        {
            return "Execution PlaceHolder";
        }

        public override bool AllowStartNewGatherings
        {
            get
            {
                return false;
            }
        }

        public bool IsInvited(Pawn p)
        {
            bool flag = this.lord.faction != null;
            return flag && p.Faction == this.lord.faction;
        }

    }

}
