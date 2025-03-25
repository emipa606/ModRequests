using RimWorld;
using Verse;

namespace SimpleSlaveryCollars
{
    public class RecordWorker_TimeAsSlave : RecordWorker
    {
        public override bool ShouldMeasureTimeNow(Pawn pawn) => pawn.IsSlave;
    }
}
