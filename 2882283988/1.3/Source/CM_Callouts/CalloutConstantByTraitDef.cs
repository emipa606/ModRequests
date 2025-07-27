using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using HarmonyLib;
using CM_Callouts;
using Verse.Grammar;
using CM_Callouts.PendingCallouts;
using Verse.AI;

namespace CM_Callouts
{
	// Token: 0x02000004 RID: 4
	public class CalloutConstantByTraitDef : Def
	{
		// Token: 0x0400001B RID: 27
		public List<TraitDef> traits = new List<TraitDef>();

		// Token: 0x0400001C RID: 28
		public string name;

		// Token: 0x0400001D RID: 29
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
                        if (toil.actor == null || __instance.Trader == null)
                        {
                            return;
                        }
                        new PendingCalloutEventTradeInteraction(toil.actor, __instance.Trader, //set the Trader property to public with an assembly editor
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
