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
    public class PendingCalloutEventWounded : PendingCalloutEventSinglePawn
    {
        public PendingCalloutEventWounded(Pawn _initiator)
            : base(CalloutCategory.Combat, _initiator, CalloutDefOf.CM_Callouts_RulePack_Wounded)
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
