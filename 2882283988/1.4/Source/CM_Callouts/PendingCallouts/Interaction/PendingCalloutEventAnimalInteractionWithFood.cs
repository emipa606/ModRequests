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
    public class PendingCalloutEventAnimalInteractionWithFood : PendingCalloutEventAnimalInteraction
    {
        public ThingDef food = null;

        public PendingCalloutEventAnimalInteractionWithFood(Pawn _initiator, Pawn _recipient, ThingDef _food, RulePackDef _initiatorRulePack, RulePackDef _recipientRulePack)
            : base (_initiator, _recipient, _initiatorRulePack, _recipientRulePack)
        {
            food = _food;
        }

        protected override GrammarRequest PrepareGrammarRequest(RulePackDef rulePack)
        {
            GrammarRequest grammarRequest = base.PrepareGrammarRequest(rulePack);
            grammarRequest.Rules.AddRange(GrammarUtility.RulesForDef("FOOD", food));
            return grammarRequest;
        }
    }
}
