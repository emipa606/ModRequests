using System.Collections.Generic;
using JetBrains.Annotations;
using Verse;
using static PawnTrackerMain.PawnTrackerUtility;

namespace PawnTrackerMain
{
    internal static class PawnTrackerCompUtility
    {
        private static readonly Dictionary<Pawn, PawnTrackerComp> pawnTrackerComps = new();

        [CanBeNull]
        public static PawnTrackerComp PawnTrackerComp(this Pawn pawn)
        {
            if (pawn == null)
            {    
                try
                {
                    Corpse corpse = (Corpse) pawn.Corpse;
                    pawn = corpse.InnerPawn;
                    if (pawn == null)
                        return null;
                }
                catch
                {
                    return null;
                }
            }
            
            if (pawnTrackerComps.TryGetValue(pawn, out var comp))
            {    
                return comp;
            }

            comp = pawn.GetComp<PawnTrackerComp>();
            if (comp == null)
            {   
                return null;
            }
            pawnTrackerComps.Add(pawn, comp);
            if (pawn.RaceProps.Humanlike && !CHGameComp.PawnDocumented(pawn))
                GetComp().TrackedPawns.Add(pawn);
            return comp;
        }

        public static void OnPawnRemoved(Pawn pawn)
        {
            if (!GetComp().UseMod)
                return;
            pawnTrackerComps.Remove(pawn);
        }
    }
}
