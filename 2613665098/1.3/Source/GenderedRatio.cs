using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace HediffApplier
{
    [DefOf]
    public static class myPrecepts
    {
        [MayRequireIdeology]
        public static PreceptDef AP_FemalePreceptOne;
        [MayRequireIdeology]
        public static PreceptDef AP_FemalePreceptThree;
        [MayRequireIdeology]
        public static PreceptDef AP_FemalePreceptTwo;
        [MayRequireIdeology]
        public static PreceptDef AP_FemalePreceptZero;
    }

    public class ThoughtWorker_GenderRatio : ThoughtWorker
    {
        public static bool allowprisonerslaves=false;
        public static ThoughtState CurrentThoughtState(Pawn p)
        {
            
            if (!(p.gender == Gender.Male)&&!(p.Dead) && p.Spawned)
            {
                return ThoughtState.Inactive;
            }
            else if(p.def.defName.Contains("ndroid")||!p.RaceProps.IsFlesh||p.IsPrisoner)
            {
                return ThoughtState.Inactive;
            }
            else if(!(p.GetLoveRelations(false).Count() == 0))
            {
                return ThoughtState.Inactive;
            }
            else if( ModsConfig.IdeologyActive&&!p.ideo.Ideo.HasPrecept(myPrecepts.AP_FemalePreceptOne)&& !p.ideo.Ideo.HasPrecept(myPrecepts.AP_FemalePreceptTwo)&& !p.ideo.Ideo.HasPrecept(myPrecepts.AP_FemalePreceptThree)&& !p.ideo.Ideo.HasPrecept(myPrecepts.AP_FemalePreceptZero))
            {
                return ThoughtState.Inactive;
            }
            else if (allowprisonerslaves == false&&(p.IsSlave))
            {
                return ThoughtState.Inactive;
            }

            float factor = 0.6f;
            if (p.ideo.Ideo.HasPrecept(myPrecepts.AP_FemalePreceptOne))
            {
                factor = 0.4f;
            }
            else if (p.ideo.Ideo.HasPrecept(myPrecepts.AP_FemalePreceptThree))
            {
                factor = 0.8f;
            }
            int malePawns = 0;
            int femalePawns = 0;
            List<Pawn> listPawns = new List<Pawn>();
            foreach (Pawn pawn in Find.CurrentMap.mapPawns.PawnsInFaction(p.Faction))
            {
                if (!(pawn.RaceProps.Animal) && (pawn.RaceProps.Humanlike))
                { listPawns.Add(pawn); }
            }
            foreach (Pawn pawn in Find.CurrentMap.mapPawns.AllPawnsSpawned)
            {
                if (!(listPawns.Contains(pawn)) && !(pawn.RaceProps.Animal) && (pawn.RaceProps.Humanlike) && !(pawn.Faction.HostileTo(p.Faction)))
                { listPawns.Add(pawn); }
            }
            foreach (Pawn pawn in listPawns)
            {
                if (pawn.gender == Gender.Male) malePawns++;
                else if (pawn.gender == Gender.Female) femalePawns++;
            }
            if (femalePawns == 0)
            {
                return ThoughtState.ActiveAtStage(0);
            }
            else if (femalePawns >= 1 && femalePawns <= Math.Floor(factor * malePawns))
            {
                return ThoughtState.ActiveAtStage(1);
            }
            else if (femalePawns <= malePawns)
            {
                return ThoughtState.ActiveAtStage(2);
            }
            else if (femalePawns > malePawns) return ThoughtState.ActiveAtStage(3);
            else return false;
        }

        protected override ThoughtState CurrentStateInternal(Pawn p) => ThoughtUtility.ThoughtNullified(p, this.def)||p.gender!=Gender.Male ? ThoughtState.Inactive : ThoughtWorker_GenderRatio.CurrentThoughtState(p);
    }
}
