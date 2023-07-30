using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace CM_Meeseeks_Box
{
    public class LordJob_MeeseeksKillCreator : LordJob
    {
        private bool useAvoidGridSmart;

        private Pawn target;

        public Pawn Target => target;

        public LordJob_MeeseeksKillCreator()
        {

        }

        public LordJob_MeeseeksKillCreator(Pawn creator)
        {
            this.useAvoidGridSmart = true;
            this.target = creator;
        }

        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new StateGraph();
            LordToil lordToil = null;

            lordToil = new LordToil_MeeseeksKillCreator();
            if (useAvoidGridSmart)
            {
                lordToil.useAvoidGrid = true;
            }
            stateGraph.AddToil(lordToil);
            
            return stateGraph;
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref useAvoidGridSmart, "useAvoidGridSmart", defaultValue: true);
            Scribe_References.Look(ref target, "target");
        }
    }
}
