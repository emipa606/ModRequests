using Verse;
using Verse.AI;

namespace BetterInfestations
{
    public class ThinkNode_ConditionalHiveCanReproduce : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            return (pawn.mindState.duty.focus.Thing as Hive)?.GetComp<CompSpawnerHives>().canSpawnHives ?? false;
        }
    }
    public class ThinkNode_PerTick : ThinkNode_Priority
    {
        private float ticks = -1f;
        private float savedTick = -1f;

        public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
        {
            if (Find.TickManager.TicksGame < savedTick)
            {
                return ThinkResult.NoJob;
            }
            savedTick = Find.TickManager.TicksGame + ticks;
            return base.TryIssueJobPackage(pawn, jobParams);
        }
        public override ThinkNode DeepCopy(bool resolve = true)
        {
            ThinkNode_PerTick obj = (ThinkNode_PerTick)base.DeepCopy(resolve);
            obj.ticks = ticks;
            obj.savedTick = savedTick;
            return obj;
        }
    }
}