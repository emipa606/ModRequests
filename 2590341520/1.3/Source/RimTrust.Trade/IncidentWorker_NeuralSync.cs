using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace RimTrust.Trade
{
    class IncidentWorker_NeuralSync : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (!base.CanFireNowSub(parms))
            {
                return false;
            }
           _ = (Map)parms.target;

            return base.CanFireNowSub(parms) && IncidentWorker_NeuralSync.IsNeuralSyncAppropriate((Map)parms.target);
        }
        public static bool IsNeuralSyncAppropriate(Map map)
        {
           return Methods.LegacySkills.Sum() != 0 && Methods.ColonyHasNeuralImplant(map) != false; 
        }
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (Methods.LegacySkills.Count.Equals(0))
            {
                LoadTrustFunds.LoadLegacySkills();
            }

            Map map = (Map)parms.target;
            foreach (Pawn pawn in map.mapPawns.FreeColonists)
                {
                //Log.Message("In first foreach loop NeuralSync TryExecuteWorker, map" + map.Index);
                var NeuralImplantOnPawn = pawn.health?.hediffSet?.GetFirstHediffOfDef(HediffDef_Neural.NeuralImplant);
                if (NeuralImplantOnPawn != null)
                    {
                    //Log.Message("In second if NeuralSync TryExecuteWorker for: " + pawn.Name.ToString());
                    Methods.UpdatePawnSkillFromLegacy(pawn);
                    Messages.Message("Incident_NeuralSync_SuccessMessage".Translate(), MessageTypeDefOf.PositiveEvent);
                    //Log.Message("Pawn skill updating done, before return true: " + pawn.Name.ToString());
                    //return true;
                    }
                }
            return false;
        }
    }
}


