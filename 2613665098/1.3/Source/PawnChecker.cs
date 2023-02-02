using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace HediffApplier
{
    public class PawnChecker
    {
        public static bool MeetReq(Pawn pawn)
        {
            return pawn != null && !pawn.Dead && pawn.Spawned && pawn.gender == Gender.Female && pawn.RaceProps.Humanlike;
        }

        public static bool HasHediff(Pawn pawn)
        {
            return pawn != null && pawn.health.hediffSet.GetFirstHediffOfDef(CustomDefOf.FemaleModifierDef) != null;
        }
        public static bool MeetReqMale(Pawn pawn)
        {
            return pawn != null && !pawn.Dead && pawn.Spawned && pawn.gender == Gender.Male && pawn.RaceProps.Humanlike;
        }
    }
}
