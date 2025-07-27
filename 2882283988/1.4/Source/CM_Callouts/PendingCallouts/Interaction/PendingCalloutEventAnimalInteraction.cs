using System;
using System.Collections.Generic;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Grammar;

namespace CM_Callouts.PendingCallouts.Interaction
{
    public class PendingCalloutEventAnimalInteraction : PendingCalloutEventDoublePawn
    {
        public PendingCalloutEventAnimalInteraction(Pawn _initiator, Pawn _recipient, RulePackDef _initiatorRulePack, RulePackDef _recipientRulePack)
            : base(CalloutCategory.Animal, _initiator, _recipient, _initiatorRulePack, _recipientRulePack)
        {

        }

        protected override GrammarRequest PrepareGrammarRequest(RulePackDef rulePack)
        {
            GrammarRequest grammarRequest = base.PrepareGrammarRequest(rulePack);

            if (recipient.relations != null && recipient.relations.DirectRelationExists(PawnRelationDefOf.Bond, initiator))
                grammarRequest.Constants.Add("BONDED", "true");

            return grammarRequest;
        }
    }
}
