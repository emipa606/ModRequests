using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace NightVision
{
    [UsedImplicitly]
    public class ThoughtWorker_TooBright : ThoughtWorker
    {
        public static void SetLastDarkTick(
                        Pawn pawn
                    )
        {
            if (pawn?.TryGetComp<Comp_NightVision>() is Comp_NightVision comp)
            {
                comp.LastDarkTick = Find.TickManager.TicksGame;
            }
        }

        protected override ThoughtState CurrentStateInternal(
                        Pawn p
                    )
            => p.Awake() && p.TryGetComp<Comp_NightVision>() is Comp_NightVision comp && comp.PsychBright;
    }
}