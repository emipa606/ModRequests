using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimTrust.Trade.Ext
{
    public class Trader_BankNoteExchange : VirtualTrader
    {
        public override string TraderName => "Bank";

        public override bool UniqueBalanceMethod => true;
       
        public override IEnumerable<Thing> Goods
        {
            get
            {
                Thing thing = ThingMaker.MakeThing(ThingDefOf.Silver);
                thing.stackCount = 50000;
                yield return thing;

                thing = ThingMaker.MakeThing(BankDefOf.BankNote);
                thing.stackCount = 50;
                yield return thing;
            }
        }

        public override IEnumerable<Thing> ColonyThingsWillingToBuy(Pawn playerNegotiator)
        {
            foreach (Thing item in TradeUtility.AllLaunchableThingsForTrade(Find.CurrentMap))
            {
                if (item.def == BankDefOf.BankNote || item.def == ThingDefOf.Silver)
                {
                    yield return item;
                }
            }
        }

        public override void GiveSoldThingToPlayer(Thing toGive, int countToGive, Pawn playerNegotiator)
        {
            //excluded so BankNotes don't get delivered to player after deposit silver to TrustLedger
            /*Thing thing = toGive.SplitOff(countToGive);
            thing.PreTraded(TradeAction.PlayerBuys, playerNegotiator, this);
            TradeUtility.SpawnDropPod(DropCellFinder.TradeDropSpot(Find.CurrentMap), Find.CurrentMap, thing);*/

            // increase TrustFunds Value and initiate save
            
            if (toGive.def == ThingDefOf.Silver)
                {
                countToGive = countToGive / 1000;
                Methods.UpdateTrustFunds(countToGive); 
                }
            else if (toGive.def == BankDefOf.BankNote)
                { 
                Methods.UpdateTrustFunds(countToGive);
                }
            else
                { }

            Methods.SaveTrustFunds();
        }

        public override void GiveSoldThingToTrader(Thing toGive, int countToGive, Pawn playerNegotiator)
        {
            Thing thing = toGive.SplitOff(countToGive);
            thing.PreTraded(TradeAction.PlayerSells, playerNegotiator, this);
            Thing thing2 = TradeUtility.ThingFromStockToMergeWith(this, thing);
            if (thing2 != null && !thing2.TryAbsorbStack(thing, respectStackLimit: false))
            {
                thing.Destroy();
                // added cause of Log Warning for missing item references silver
                thing.Discard();
            }
        }

        public override void InvokeTradeUI()
        {
            Methods.cacheNotes = (from x in TradeSession.deal.AllTradeables
                                  where x.ThingDef == BankDefOf.BankNote
                                  orderby x.AnyThing.HitPoints descending
                                  select x).ToList();
            if (Methods.debug)
            {
                Utility.DebugOutputNotes();
                Utility.DebugOutputTradeables(TradeSession.deal.AllTradeables.ToList());
            }
            Utility.AskPayByBankNotes(TradeSession.deal.CurrencyTradeable, isVirtual: true);
        }

        public override string TipString(int index)
        {
            switch (index)
            {
                case 1:
                    //return "ExchangeTitle".Translate();
                    //simple TrustFunds value debug show on console title
                    return "TrustFunds: " + Methods.TrustFunds.ToString() + " BankNotes (" + Methods.TrustFunds * 1000 + " silver)";

                case 2:
                    return "ExchangeTip".Translate();

                case 3:
                    return "ExchangeSilverTip".Translate();

                case 4:
                    return "ExchangeBankNoteTip".Translate();

                default:
                    return "BUGGED!";
            }
        }

        public override void BalanceMethod(int silver, int notes, ref int silver2, ref int notes2)
        {
            silver2 = -notes2 * 1000 - (int)((float)notes2 * ExtUtil.BrokerageFactor(notes2) * 1000f);
        }

        public override bool CustomCheckViolation(Tradeable silver, Tradeable notes)
        {
            return silver.CountPostDealFor(Transactor.Trader) < 0;
        }

        public override void CustomViolationAction()
        {
            Messages.Message("NotEnoughSilverBank".Translate(), MessageTypeDefOf.RejectInput);
        }
    }
}