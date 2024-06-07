using Verse;
using RimWorld;

namespace ArchiteCapsuleExtractor
{
    public class WorkGiver_CarryToArchiteCapsuleExtractor : WorkGiver_CarryToBuilding
    {
        public override ThingRequest ThingRequest => ThingRequest.ForDef(ArchiteCapsuleExtractor_DefOfs.ArchiteCapsuleExtractor);

        public override bool ShouldSkip(Pawn pawn, bool forced = false) => !ModsConfig.BiotechActive;
    }
}
