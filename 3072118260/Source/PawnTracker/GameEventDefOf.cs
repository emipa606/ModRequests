using Verse;
using RimWorld;
using Verse.AI;


namespace PawnTrackerMain
{
    [DefOf]
    public static class GameEventDefOf
    {
        public static GameEventDef Raiders;
        public static GameEventDef Manhunters;

        public static GameEventDef Illness;
        public static GameEventDef MechCluster;
        public static GameEventDef Infestation;
        static GameEventDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof (GameEventDefOf));
    }
}