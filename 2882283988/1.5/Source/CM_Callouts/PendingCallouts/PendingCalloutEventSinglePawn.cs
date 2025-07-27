using System;
using System.Collections.Generic;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Grammar;

namespace CM_Callouts.PendingCallouts
{
    public class PendingCalloutEventSinglePawn : PendingCalloutEvent
    {
        public Pawn initiator = null;

        public RulePackDef initiatorRulePack = null;

        public PendingCalloutEventSinglePawn(CalloutCategory _category, Pawn _initiator, RulePackDef _initiatorRulePack)
            : base(_category)
        {
            initiator = _initiator;
            initiatorRulePack = _initiatorRulePack;
        }

        public override void AttemptCallout()
        {
            if (!CalloutMod.settings.CalloutCategoryEnabled(category))
                return;

            base.AttemptCallout();

            // Some of the functions leading here might be called when loading the game
            if (Scribe.mode != LoadSaveMode.Inactive)
                return;

            if (initiator == null)
            {
                Logger.WarningFormat(this, "Initiator null");
                return;
            }

            CalloutTracker calloutTracker = Current.Game.World.GetComponent<CalloutTracker>();
            if (calloutTracker != null)
            {
                bool initiatorCallout = (Prefs.DevMode && CalloutMod.settings.forceInitiatorCallouts) || (calloutTracker.CheckCalloutChance(category, initiatorRulePack) && CalloutUtility.CanCalloutNow(initiator));

                if (initiatorCallout)
                    DoInitiatorCallout(calloutTracker);
            }
        }

        private void DoInitiatorCallout(CalloutTracker calloutTracker)
        {
            GrammarRequest grammarRequest = PrepareGrammarRequest(initiatorRulePack);
            calloutTracker.RequestCallout(initiator, initiatorRulePack, grammarRequest);
        }

        protected virtual GrammarRequest PrepareGrammarRequest(RulePackDef rulePack)
        {
            GrammarRequest grammarRequest = new GrammarRequest { Includes = { rulePack } };
            CalloutUtility.CollectPawnRules(initiator, "INITIATOR", ref grammarRequest);
            return grammarRequest;
        }
    }
}
