using Verse;

namespace BetterInfestations
{
    public class Queen : Pawn
	{
        public int nextHiveSpawnTick = -1;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad && nextHiveSpawnTick == -1)
            {
                CalculateNextHiveSpawnTick();
            }
        }
        public override void Tick()
        {
            base.Tick();
        }
        public void CalculateNextHiveSpawnTick()
        {
            if (BetterInfestationsMod.settings == null) return;

            float days = Rand.Range(BetterInfestationsMod.settings.queenHiveMinSpawnInDays, BetterInfestationsMod.settings.queenHiveMaxSpawnInDays);
            int ticks = (int)(days * 60000);
            nextHiveSpawnTick = Find.TickManager.TicksGame + ticks;
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref nextHiveSpawnTick, "nextHiveSpawnTick", -1);
        }
    }
}
