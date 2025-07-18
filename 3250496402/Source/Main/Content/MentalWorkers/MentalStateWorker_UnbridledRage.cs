using Verse;
using Verse.AI;

namespace Kraltech_Industries;

public class MentalStateWorker_UnbridledRage : MentalStateWorker
{
    public override bool StateCanOccur(Pawn pawn)
    {
        return base.StateCanOccur(pawn) && UnbridledRageMentalStateUtility.FindPawnToKill(pawn) != null;
    }
}