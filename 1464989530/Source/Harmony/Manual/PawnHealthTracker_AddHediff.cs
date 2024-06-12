using Verse;

namespace NightVision.Harmony.Manual
{

    public static class PawnHealthTracker_AddHediff
    {
        public static void AddHediff_Postfix(
            Hediff hediff,
            BodyPartRecord part,
            Pawn ___pawn
        )
        {
            if (___pawn != null
                && ___pawn.Spawned
                && /*part != null && hediff is Hediff_MissingPart &&*/ /*___pawn.RaceProps.Humanlike &&*/
                ___pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision comp)
            {
                comp.CheckAndAddHediff(hediff, part);
            }
        }
    }
}