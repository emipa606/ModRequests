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
    public class PendingCalloutEventRangedImpact : PendingCalloutEventDoublePawn
    {
        public Pawn originalTarget = null;
        public ThingDef weaponDef = null;
        public ThingDef projectileDef = null;
        public ThingDef coverDef = null;

        public PendingCalloutEventRangedImpact(Pawn _initiator, Pawn _recipient, Pawn _originalTarget, ThingDef _weaponDef, ThingDef _projectileDef, ThingDef _coverDef)
            : base(CalloutCategory.Combat, _initiator, _recipient, CalloutDefOf.CM_Callouts_RulePack_Ranged_Attack_Landed_OriginalTarget, CalloutDefOf.CM_Callouts_RulePack_Ranged_Attack_Received_OriginalTarget)
        {
            originalTarget = _originalTarget;

            weaponDef = _weaponDef;
            projectileDef = _projectileDef;
            coverDef = _coverDef;
        }

        protected override GrammarRequest PrepareGrammarRequest(RulePackDef rulePack)
        {
            GrammarRequest grammarRequest = base.PrepareGrammarRequest(rulePack);

            if (recipient != null)
            {
                grammarRequest.Rules.AddRange(PlayLogEntryUtility.RulesForDamagedParts("PART", body, bodyPartsDamaged, bodyPartsDestroyed, grammarRequest.Constants));

                if (coverDef != null)
                    grammarRequest.Rules.AddRange(GrammarUtility.RulesForDef("RECIPIENT_COVER", coverDef));
            }

            return grammarRequest;
        }
    }
}
