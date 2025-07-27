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
    public class PendingCalloutEventMeleeImpact : PendingCalloutEventDoublePawn
    {
        public PendingCalloutEventMeleeImpact(Pawn _initiator, Pawn _recipient)
            : base(CalloutCategory.Combat, _initiator, _recipient, CalloutDefOf.CM_Callouts_RulePack_Melee_Attack_Landed, CalloutDefOf.CM_Callouts_RulePack_Melee_Attack_Received)
        {
        }

        protected override GrammarRequest PrepareGrammarRequest(RulePackDef rulePack)
        {
            GrammarRequest grammarRequest = base.PrepareGrammarRequest(rulePack);
            grammarRequest.Rules.AddRange(PlayLogEntryUtility.RulesForDamagedParts("PART", body, bodyPartsDamaged, bodyPartsDestroyed, grammarRequest.Constants));
            return grammarRequest;
        }
    }
}
