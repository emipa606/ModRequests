using Verse;

namespace ImperialFunctionality
{
    public abstract class CompImperialSpawner : ThingComp
    {
        protected int ticksUntilSpawn;
        protected abstract bool CanOperate { get; }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            if (!respawningAfterLoad)
            {
                ResetCountdown();
            }
        }

        public override void CompTick()
        {
            TickInterval(1);
        }

        public override void CompTickRare()
        {
            TickInterval(250);
        }

        protected virtual void TickInterval(int interval)
        {
            if (CanOperate)
            {
                ticksUntilSpawn -= interval;
                CheckShouldSpawn();
            }
        }

        protected void CheckShouldSpawn()
        {
            if (ticksUntilSpawn <= 0)
            {
                ResetCountdown();
                TryDoSpawn();
            }
        }

        protected abstract void TryDoSpawn();

        public bool TryFindRandomCellNear(IntVec3 root, Map map, float radius, out IntVec3 result)
        {
            foreach (var cell in GenRadial.RadialCellsAround(root, radius, true))
            {
                if (cell.GetFirstItem(map) is null)
                {
                    result = cell;
                    return true;
                }
            }
            result = IntVec3.Invalid;
            return false;
        }

        protected void ResetCountdown()
        {
            ticksUntilSpawn = TicksUntilSpawn;
        }

        public abstract int TicksUntilSpawn { get; }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref ticksUntilSpawn, "ImperialFunctionality_ticksUntilSpawn", 0);
        }
    }
}
