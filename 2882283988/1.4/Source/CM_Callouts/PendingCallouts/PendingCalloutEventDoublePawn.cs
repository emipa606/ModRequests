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
    public class PendingCalloutEventDoublePawn : PendingCalloutEvent
    {
        public Pawn initiator = null;
        public Pawn recipient = null;

        public RulePackDef initiatorRulePack = null;
        public RulePackDef recipientRulePack = null;

        public PendingCalloutEventDoublePawn(CalloutCategory _category, Pawn _initiator, Pawn _recipient, RulePackDef _initiatorRulePack, RulePackDef _recipientRulePack)
            : base(_category)
        {
            initiator = _initiator;
            recipient = _recipient;
            initiatorRulePack = _initiatorRulePack;
            recipientRulePack = _recipientRulePack;
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

            if (recipient == null)
            {
                Logger.WarningFormat(this, "Recipient null");
                return;
            }

            CalloutTracker calloutTracker = Current.Game.World.GetComponent<CalloutTracker>();
            if (calloutTracker != null)
            {
                bool hasInitiatorCallout = initiatorRulePack != null;
                bool hasRecipientCallout = recipientRulePack != null;

                bool initiatorCalloutForced = (hasInitiatorCallout && Prefs.DevMode && CalloutMod.settings.forceInitiatorCallouts);
                bool recipientCalloutForced = (hasRecipientCallout && Prefs.DevMode && CalloutMod.settings.forceRecipientCallouts);

                bool initiatorCallout = hasInitiatorCallout && (initiatorCalloutForced || ((!hasRecipientCallout || Rand.Bool) && calloutTracker.CheckCalloutChance(category, initiatorRulePack) && CalloutUtility.CanCalloutNow(initiator) && !recipientCalloutForced));
                bool recipientCallout = hasRecipientCallout && (recipientCalloutForced || ((!hasInitiatorCallout || Rand.Bool) && calloutTracker.CheckCalloutChance(category, recipientRulePack) && CalloutUtility.CanCalloutNow(initiator)));

                if (initiatorCallout)
                    DoInitiatorCallout(calloutTracker);
                else if (recipientCallout)
                    DoRecipientCallout(calloutTracker);
            }
        }

        private void DoInitiatorCallout(CalloutTracker calloutTracker)
        {
            GrammarRequest grammarRequest = PrepareGrammarRequest(initiatorRulePack);
            calloutTracker.RequestCallout(initiator, initiatorRulePack, grammarRequest);
        }

        private void DoRecipientCallout(CalloutTracker calloutTracker)
        {
            GrammarRequest grammarRequest = PrepareGrammarRequest(recipientRulePack);
            calloutTracker.RequestCallout(recipient, recipientRulePack, grammarRequest);
        }

        protected virtual GrammarRequest PrepareGrammarRequest(RulePackDef rulePack)
        {
            GrammarRequest grammarRequest = new GrammarRequest { Includes = { rulePack } };

            CalloutUtility.CollectPawnRules(initiator, "INITIATOR", ref grammarRequest);

            if (recipient != null)
                CalloutUtility.CollectPawnRules(recipient, "RECIPIENT", ref grammarRequest);

            return grammarRequest;
        }
    }
}
