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
    public class PendingCalloutEventMeleeAttempt : PendingCalloutEventDoublePawn
    {
        public Verb verb = null;

        public PendingCalloutEventMeleeAttempt(Pawn _initiator, Pawn _recipient, Verb _verb)
            : base(CalloutCategory.Combat, _initiator, _recipient, CalloutDefOf.CM_Callouts_RulePack_Melee_Attack, null)
        {
            verb = _verb;
        }

        protected override GrammarRequest PrepareGrammarRequest(RulePackDef rulePack)
        {
            GrammarRequest grammarRequest = base.PrepareGrammarRequest(rulePack);

            if (verb != null && verb.maneuver != null && verb.maneuver.combatLogRulesHit != null)
                grammarRequest.Includes.Add(verb.maneuver.combatLogRulesHit);

            return grammarRequest;
        }
    }
}
