using RimWorld;
using Verse;

namespace Kraltech_Industries;

public class Building_SATrophy : Building, IObservedThoughtGiver
{
    public Thought_Memory GiveObservedThought(Pawn observer)
    {
        Thought_Memory result;
        if ((observer.Ideo != null && observer.Ideo.IdeoApprovesOfSlavery()) || !observer.IsSlave)
        {
            result = null;
        }
        else
        {
            var thought_MemoryObservation = (Thought_MemoryObservation)ThoughtMaker.MakeThought(ThoughtDefOf.ObservedSupremeApocritonTrophy);
            thought_MemoryObservation.Target = this;
            result = thought_MemoryObservation;
        }

        return result;
    }

    public HistoryEventDef GiveObservedHistoryEvent(Pawn observer)
    {
        return null;
    }
}