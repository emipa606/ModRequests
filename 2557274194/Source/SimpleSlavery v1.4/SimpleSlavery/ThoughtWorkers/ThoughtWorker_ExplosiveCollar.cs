using RimWorld;
using Verse;

namespace SimpleSlaveryCollars
{
    public class ThoughtWorker_ExplosiveCollar : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn pawn)
        {
            if (SlaveUtility.HasSlaveCollar(pawn) && SlaveUtility.GetSlaveCollar(pawn).def.thingClass == typeof(SlaveCollar_Explosive))
            {
                if ((SlaveUtility.GetSlaveCollar(pawn) as SlaveCollar_Explosive).armed) return ThoughtState.ActiveAtStage(1);
                return pawn.IsSlaveOfColony ? ThoughtState.ActiveAtStage(0) : ThoughtState.ActiveAtStage(2);
            }
            return ThoughtState.Inactive;
        }
    }
}