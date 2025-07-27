using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Grammar;
using Verse.AI;
using CM_Callouts.PendingCallouts;
using System;

namespace CM_Callouts
{
    public class CalloutConstantByTraitDef : Def
    {
        public List<TraitDef> traits = new List<TraitDef>();

        public string name;
        public string value;
    }
    public class CalloutConstantByPawnkindDef : Def
    {
        public List<PawnKindDef> pawnKindDefs = new List<PawnKindDef>();
        public string name;
        public string value;
    }
    public class CalloutConstantByThingDef : Def
    {
        public List<ThingDef> thingDefs = new List<ThingDef>();
        public string name;
        public string value;
    }
    public class CalloutConstantByHediffDef : Def
    {
        public List<HediffDef> hediffDefs = new List<HediffDef>();
        public string name;
        public string value;
    }
    public class CalloutConstantByHediffStage : Def
    {
        public List<HediffAndStage> hediffsAndStages = new List<HediffAndStage>();
        public string name;
        public string value;
    }
    public class HediffAndStage
    {
        public HediffDef hediffDef;
        public int stage;
    }
    public class CalloutConstantByNeedDef : Def
    {
        public NeedDef needDef;
        public float needLevel;
        public bool aboveLevel;
        public string name;
        public string value;
    }
    public class PendingCalloutEventTradeInteraction : PendingCalloutEventDoublePawn
    {
        public PendingCalloutEventTradeInteraction(Pawn _initiator, Pawn _recipient, RulePackDef _initiatorRulePack, RulePackDef _recipientRulePack) : base(CalloutCategory.Undefined, _initiator, _recipient, _initiatorRulePack, _recipientRulePack)
        {
        }
        protected override GrammarRequest PrepareGrammarRequest(RulePackDef rulePack)
        {
            GrammarRequest result = base.PrepareGrammarRequest(rulePack);
            result.Constants.Add("RECIPIENT_TraderKindDef", recipient.TraderKind.defName);
            return result;
        }
    }
    [DefOf]
    public static class CalloutsExpandedDefOf
    {
        public static RulePackDef CalloutsExpanded_RulePack_Trade_Initiated;
        public static RulePackDef CalloutsExpanded_RulePack_Trade_Received;
    }

    [HarmonyPatch(typeof(JobDriver_TradeWithPawn), "MakeNewToils")]
    public static class TradeWithPawn_CalloutsExpanded_Patch
    {
        public static IEnumerable<Toil> Postfix(IEnumerable<Toil> values, JobDriver_TradeWithPawn __instance)
        {
            List<Toil> toils = values.ToList();
            int i;
            for (i = 0; i < toils.Count; i++)
            {
                if (i == toils.Count - 1)
                {
                    Toil toil = toils[i];
                    Action action = toil.initAction;
                    toil.initAction = delegate ()
                    {
                        action();
                        Pawn pawn = __instance.GetType().GetProperty("Trader", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(__instance) as Pawn;
                    if (toil.actor == null || pawn == null)
                        {
                            return;
                        }
                        new PendingCalloutEventTradeInteraction(toil.actor, pawn,
                            CalloutsExpandedDefOf.CalloutsExpanded_RulePack_Trade_Initiated,
                            CalloutsExpandedDefOf.CalloutsExpanded_RulePack_Trade_Received).AttemptCallout();
                    };
                    yield return toils[i];
                    continue;
                }
                yield return toils[i];
            }
        }
    }
}
