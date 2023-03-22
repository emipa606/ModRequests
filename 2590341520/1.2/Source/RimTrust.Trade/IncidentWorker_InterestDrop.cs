using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace RimTrust.Trade
{
    public class IncidentWorker_InterestDrop : IncidentWorker
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
                return base.CanFireNowSub(parms) && IncidentWorker_InterestDrop.IsInterestDropAppropriate((Map)parms.target);
            }
        }
        public static bool IsInterestDropAppropriate(Map map)
        {
           return Methods.TrustFunds != 0; 
        }
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            //Map map = (Map)parms.target;
            Thing thing = ThingMaker.MakeThing(ThingDefOf.Silver);
            Thing thing2 = ThingMaker.MakeThing(BankDefOf.BankNote);
            int Interest = (int)(Methods.TrustFunds * 1000 * 0.0025);
            thing.stackCount = Interest;

            int InterestSilver = Interest;
            int InterestBankNotes = 0;
            if (Interest >= 1000)
            {
                InterestBankNotes = Math.DivRem(Interest, 1000, out InterestSilver);
                thing2.stackCount = InterestBankNotes;
                thing.stackCount = InterestSilver;
            }

            IntVec3 dropspot = DropCellFinder.TradeDropSpot(Find.CurrentMap);
            if (dropspot == IntVec3.Invalid || Methods.TrustFunds == 0)
            {
                return false;
            }
            else
            {
                if (thing2.stackCount > 0)
                {
                    TradeUtility.SpawnDropPod(dropspot, Find.CurrentMap, thing2);
                }
                TradeUtility.SpawnDropPod(dropspot, Find.CurrentMap, thing);
                Messages.Message("Incident_InterestDrop_SuccessMessage".Translate(Interest.ToString()), MessageTypeDefOf.PositiveEvent);
                return true;
            }
        } 
    }
}

