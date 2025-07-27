using System;
using System.Collections.Generic;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Grammar;

namespace CM_Callouts.PendingCallouts.Combat
{
    public class PendingCalloutEventRangedAttempt : PendingCalloutEventDoublePawn
    {
        public Verb_LaunchProjectile verb = null;

        public PendingCalloutEventRangedAttempt(Pawn _initiator, Pawn _recipient, Verb_LaunchProjectile _verb)
            : base(CalloutCategory.Combat, _initiator, _recipient, CalloutDefOf.CM_Callouts_RulePack_Ranged_Attack, null)
        {
            verb = _verb;
        }

        protected override GrammarRequest PrepareGrammarRequest(RulePackDef rulePack)
        {
            GrammarRequest grammarRequest = base.PrepareGrammarRequest(rulePack);

            if (recipient != null)
            {
                CalloutUtility.CollectCoverRules(recipient, initiator, "INITIATOR_COVER", verb, ref grammarRequest);
                CalloutUtility.CollectCoverRules(initiator, recipient, "RECIPIENT_COVER", verb, ref grammarRequest);
            }

            return grammarRequest;
        }
    }
}
