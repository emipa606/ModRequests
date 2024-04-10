using RimWorld;
using Verse;
using Verse.AI;

namespace BetterInfestations
{
    [DefOf]
    public static class ThingDefOf
    {
        public static ThingDef BI_Queen;
        public static ThingDef BI_Hive;
        public static ThingDef BI_TunnelHiveSpawner;
    }
    [DefOf]
    public static class DutyDefOf
    {
        public static DutyDef BI_DefendAndExpandHive;
        public static DutyDef BI_DefendHiveAggressively;
        public static DutyDef BI_HiveHunters;
    }
    [DefOf]
    public static class JobDefOf
    {
        public static JobDef BI_Maintain;
        public static JobDef BI_Butcher;
        public static JobDef BI_GotoSpawnHive;
        public static JobDef BI_GotoPatrol;
        public static JobDef BI_ChewConduits;
    }
    [DefOf]
    public static class PawnKindDefOf
    {
        public static PawnKindDef BI_Queen;
    }
}