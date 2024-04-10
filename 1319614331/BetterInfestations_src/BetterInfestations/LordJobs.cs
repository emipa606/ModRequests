using RimWorld;
using Verse;
using Verse.AI.Group;

namespace BetterInfestations
{
    public class LordJob_DefendAndExpandHive : LordJob
    {
        public override bool CanBlockHostileVisitors => false;
        public override bool AddFleeToil => false;
        public LordJob_DefendAndExpandHive()
        {
        }
        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new StateGraph();
            LordToil_DefendAndExpandHive lordToil_DefendAndExpandHive = new LordToil_DefendAndExpandHive();
            lordToil_DefendAndExpandHive.distToHiveToAttack = 30f;
            stateGraph.StartingToil = lordToil_DefendAndExpandHive;
            LordToil_DefendHiveAggressively lordToil_DefendHiveAggressively = new LordToil_DefendHiveAggressively();
            lordToil_DefendHiveAggressively.distToHiveToAttack = 80f;
            stateGraph.AddToil(lordToil_DefendHiveAggressively);
            LordToil_AssaultColony lordToil_AssaultColony = new LordToil_AssaultColony();
            stateGraph.AddToil(lordToil_AssaultColony);
            Transition transition = new Transition(lordToil_DefendAndExpandHive, lordToil_DefendHiveAggressively);
            transition.AddTrigger(new Trigger_PawnHarmed(0.5f, requireInstigatorWithFaction: true));
            transition.AddTrigger(new Trigger_PawnLostViolently(allowRoofCollapse: true));
            transition.AddTrigger(new Trigger_Memo(Hive.MemoAttackedByEnemy));
            transition.AddTrigger(new Trigger_Memo(HediffGiver_Heat.MemoPawnBurnedByAir));
            transition.AddPostAction(new TransitionAction_EndAllJobs());
            stateGraph.AddTransition(transition);
            Transition transition2 = new Transition(lordToil_DefendAndExpandHive, lordToil_AssaultColony);
            transition2.AddSource(lordToil_DefendHiveAggressively);
            transition2.AddTrigger(new Trigger_Memo(Hive.MemoDeSpawned));
            transition2.AddTrigger(new Trigger_Memo(Hive.MemoDestroyedNonRoofCollapse));
            transition2.AddPostAction(new TransitionAction_EndAllJobs());
            stateGraph.AddTransition(transition2);
            Transition transition3 = new Transition(lordToil_DefendHiveAggressively, lordToil_DefendAndExpandHive);
            transition3.AddTrigger(new Trigger_TicksPassedWithoutHarmOrMemos(1200, Hive.MemoAttackedByEnemy, HediffGiver_Heat.MemoPawnBurnedByAir));
            transition3.AddPostAction(new TransitionAction_EndAllJobs());
            stateGraph.AddTransition(transition3);
            return stateGraph;
        }
    }
    public class LordJob_HiveHunters : LordJob
    {
        public override bool CanBlockHostileVisitors => false;
        public override bool AddFleeToil => false;
        public LordJob_HiveHunters()
        {
        }
        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new StateGraph();
            LordToil_HiveHunters lordToil_HiveHunters = new LordToil_HiveHunters();
            stateGraph.StartingToil = lordToil_HiveHunters;
            LordToil_DefendHiveAggressively lordToil_DefendHiveAggressively = new LordToil_DefendHiveAggressively();
            lordToil_DefendHiveAggressively.distToHiveToAttack = 80f;
            stateGraph.AddToil(lordToil_DefendHiveAggressively);
            LordToil_AssaultColony lordToil_AssaultColony = new LordToil_AssaultColony();
            stateGraph.AddToil(lordToil_AssaultColony);
            Transition transition = new Transition(lordToil_HiveHunters, lordToil_DefendHiveAggressively);
            transition.AddTrigger(new Trigger_Memo(Hive.MemoAttackedByEnemy));
            transition.AddTrigger(new Trigger_Memo(HediffGiver_Heat.MemoPawnBurnedByAir));
            transition.AddPostAction(new TransitionAction_EndAllJobs());
            stateGraph.AddTransition(transition);
            Transition transition2 = new Transition(lordToil_HiveHunters, lordToil_AssaultColony);
            transition2.AddTrigger(new Trigger_Memo(Hive.MemoDeSpawned));
            transition2.AddTrigger(new Trigger_Memo(Hive.MemoDestroyedNonRoofCollapse));
            transition2.AddPostAction(new TransitionAction_EndAllJobs());
            stateGraph.AddTransition(transition2);
            Transition transition3 = new Transition(lordToil_DefendHiveAggressively, lordToil_HiveHunters);
            transition3.AddTrigger(new Trigger_TicksPassedWithoutHarmOrMemos(1200, Hive.MemoAttackedByEnemy, HediffGiver_Heat.MemoPawnBurnedByAir));
            transition3.AddPostAction(new TransitionAction_EndAllJobs());
            stateGraph.AddTransition(transition3);
            return stateGraph;
        }
    }
    public class LordJob_AssaultColony : LordJob
    {
        public override bool CanBlockHostileVisitors => false;
        public override bool AddFleeToil => false;
        public LordJob_AssaultColony()
        {
        }
        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new StateGraph();
            LordToil_AssaultColony lordToil_AssaultColony = new LordToil_AssaultColony();
            stateGraph.StartingToil = lordToil_AssaultColony;
            return stateGraph;
        }
    }
}