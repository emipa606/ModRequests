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
    public class PendingCalloutEventLethalHediffProgression : PendingCalloutEventSinglePawn
    {
        public Hediff hediff = null;

        public PendingCalloutEventLethalHediffProgression(Pawn _initiator, Hediff _hediff)
            : base(CalloutCategory.Undefined, _initiator, CalloutDefOf.CM_Callouts_RulePack_Lethal_Hediff_Progression)
        {
            hediff = _hediff;
        }

        protected override GrammarRequest PrepareGrammarRequest(RulePackDef rulePack)
        {
            GrammarRequest grammarRequest = base.PrepareGrammarRequest(rulePack);
            CalloutUtility.CollectHediffRules(hediff, "CULPRITHEDIFF", ref grammarRequest);
            return grammarRequest;
        }
    }
}
