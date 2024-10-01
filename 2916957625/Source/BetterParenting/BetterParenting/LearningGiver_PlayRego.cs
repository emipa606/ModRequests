using RimWorld;
using Verse;
using Verse.AI;

namespace BetterParenting
{
    public class LearningGiver_PlayRego : LearningGiver
    {
        public override bool CanGiveDesire => ResearchProjectDefOf.MicroelectronicsBasics.IsFinished;

        private bool TryFindToybox(Pawn pawn, out Thing toybox)
        {
            toybox = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(ThingDefOf.Rego), PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f, (Thing t) => t is Building_Toybox building_Toybox && building_Toybox.CanUseboxNow && pawn.CanReserve(building_Toybox) && !building_Toybox.IsForbidden(pawn));
            return toybox != null;
        }

        public override bool CanDo(Pawn pawn)
        {
            if (!base.CanDo(pawn))
            {
                return false;
            }
            Thing toybox;
            return TryFindToybox(pawn, out toybox);
        }

        public override Job TryGiveJob(Pawn pawn)
        {
            if (!TryFindToybox(pawn, out var toybox))
            {
                return null;
            }
            return JobMaker.MakeJob(def.jobDef, toybox);
        }
    }
}