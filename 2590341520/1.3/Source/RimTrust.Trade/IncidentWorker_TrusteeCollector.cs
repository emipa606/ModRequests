using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace RimTrust.Trade
{
    class IncidentWorker_TrusteeCollector : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (!base.CanFireNowSub(parms))
            {
                return false;
            }
            else
            {
                _ = (Map)parms.target;
                return base.CanFireNowSub(parms) && IncidentWorker_TrusteeCollector.IsTrusteeCollectorAppropriate((Map)parms.target);
            }
        }
        public static bool IsTrusteeCollectorAppropriate(Map map)
        {
            return Methods.TrustFunds >= 50 && Methods.CalculateColonyValuables() > 5;
        }
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            int ValuablesGain = Methods.CalculateColonyValuables();
            Thing thing = ThingMaker.MakeThing(ThingDefOf.Silver);
            thing.stackCount = ValuablesGain;
            string msg = ValuablesGain.ToString();
            Methods.UpdateTrustFunds(ValuablesGain);
            Methods.SaveTrustFunds();
            Methods.TrusteeTakeThings(Methods.TrusteeCollectorThings);
            Log.Message("Incident TrusteeCollector fired with: " + msg);
            TaggedString baseLetterText = this.def.letterText.Formatted(ValuablesGain.ToString("VALUABLESGAIN"));
            TaggedString baseLetterLabel = this.def.letterLabel.Formatted(ValuablesGain.ToString("VALUABLESGAIN"));
            base.SendStandardLetter(baseLetterLabel, baseLetterText, LetterDefOf.PositiveEvent, parms, thing, Array.Empty<NamedArgument>());
            return true;
        }
    }
}
