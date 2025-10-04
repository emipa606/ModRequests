using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using static UnityEngine.Random;
using static UnityEngine.Scripting.GarbageCollector;

namespace PropagandaAndManifestos
{
    public class CompProperties_IdeoPropaganda : CompProperties
    {
        public CompProperties_IdeoPropaganda()
        {
            compClass = typeof(CompIdeoPropaganda);
        }
    }
    [DefOf]
    public static class IdeoConversionDef
    {
        public static QuestScriptDef IdeoConversion;
        static IdeoConversionDef()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(IdeoConversionDef));
        }
    }
    [DefOf]
    public static class DiversityDef
    {
        public static PreceptDef IdeoDiversity_Exalted;
        public static PreceptDef IdeoDiversity_Respected;
        public static PreceptDef IdeoDiversity_Approved;
        public static PreceptDef IdeoDiversity_Disapproved;
        public static PreceptDef IdeoDiversity_Horrible;
        public static PreceptDef IdeoDiversity_Abhorrent;
        static DiversityDef()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(DiversityDef));
        }
    }

    [DefOf]
    public static class IdeoDef
    {
        public static ThingDef IdeoFlyer;
        static IdeoDef()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(IdeoDef));
        }
    }
    public class WorldObjectCompProperties_IdeoConversion : WorldObjectCompProperties
    {
        public WorldObjectCompProperties_IdeoConversion()
        {
            compClass = typeof(IdeoConversionComp);
        }
    }
    public class QuestNode_IdeoConversion_Initiate : QuestNode
    {
        public SlateRef<Settlement> settlement;

        public SlateRef<ThingDef> requestedThingDef;

        public SlateRef<int> requestedThingCount;

        public SlateRef<int> duration;

        protected override void RunInt()
        {
            Slate slate = QuestGen.slate;
            QuestPart_InitiateIdeoConversion questPart_InitiateIdeoConversion = new QuestPart_InitiateIdeoConversion();
            questPart_InitiateIdeoConversion.settlement = settlement.GetValue(slate);
            questPart_InitiateIdeoConversion.requestedThingDef = IdeoDef.IdeoFlyer;
            questPart_InitiateIdeoConversion.requestedCount = questPart_InitiateIdeoConversion.ReqFlyer();
            questPart_InitiateIdeoConversion.requestDuration = duration.GetValue(slate);
            questPart_InitiateIdeoConversion.keepAfterQuestEnds = false;
            questPart_InitiateIdeoConversion.inSignal = slate.Get<string>("inSignal");
            QuestGen.quest.AddPart(questPart_InitiateIdeoConversion);
        }

        protected override bool TestRunInt(Slate slate)
        {
            return settlement.GetValue(slate) != null && requestedThingCount.GetValue(slate) > 0 && requestedThingDef.GetValue(slate) != null && duration.GetValue(slate) > 0;
        }
    }
    public class QuestPart_InitiateIdeoConversion : QuestPart
    {
        public string inSignal;

        public Settlement settlement;

        public ThingDef requestedThingDef;

        public int requestedCount;

        public int requestDuration;

        public bool keepAfterQuestEnds;

        public override IEnumerable<GlobalTargetInfo> QuestLookTargets
        {
            get
            {
                foreach (GlobalTargetInfo questLookTarget in base.QuestLookTargets)
                {
                    yield return questLookTarget;
                }
                if (settlement != null)
                {
                    yield return settlement;
                }
            }
        }
        public int ReqFlyer()
        {
            if (settlement.Faction.ideos.PrimaryIdeo.GetPrecept(DiversityDef.IdeoDiversity_Exalted) != null) { Log.Message("exalted"); return 25; }
            if (settlement.Faction.ideos.PrimaryIdeo.GetPrecept(DiversityDef.IdeoDiversity_Respected) != null) { Log.Message("respected"); return 50; }
            if (settlement.Faction.ideos.PrimaryIdeo.GetPrecept(DiversityDef.IdeoDiversity_Approved) != null) { Log.Message("approved"); return 75; }
            if (settlement.Faction.ideos.PrimaryIdeo.GetPrecept(DiversityDef.IdeoDiversity_Disapproved) != null){ Log.Message("disapproved"); return 125; }
            if (settlement.Faction.ideos.PrimaryIdeo.GetPrecept(DiversityDef.IdeoDiversity_Horrible) != null) { Log.Message("horrible"); return 150; }
            if (settlement.Faction.ideos.PrimaryIdeo.GetPrecept(DiversityDef.IdeoDiversity_Abhorrent) != null) { Log.Message("abhorrent"); return 175; }
            Log.Message("neutral");
            return 100;
        }
        public static int ReqFlyer(Settlement settlement)
        {
            if (settlement.Faction.ideos.PrimaryIdeo.GetPrecept(DiversityDef.IdeoDiversity_Exalted) != null) { return 24; }
            if (settlement.Faction.ideos.PrimaryIdeo.GetPrecept(DiversityDef.IdeoDiversity_Respected) != null) { return 49; }
            if (settlement.Faction.ideos.PrimaryIdeo.GetPrecept(DiversityDef.IdeoDiversity_Approved) != null) { return 74; }
            if (settlement.Faction.ideos.PrimaryIdeo.GetPrecept(DiversityDef.IdeoDiversity_Disapproved) != null) { return 124; }
            if (settlement.Faction.ideos.PrimaryIdeo.GetPrecept(DiversityDef.IdeoDiversity_Horrible) != null) { return 149; }
            if (settlement.Faction.ideos.PrimaryIdeo.GetPrecept(DiversityDef.IdeoDiversity_Abhorrent) != null) { return 174; }
            return 99;
        }
        public override IEnumerable<Faction> InvolvedFactions
        {
            get
            {
                foreach (Faction involvedFaction in base.InvolvedFactions)
                {
                    yield return involvedFaction;
                }
                if (settlement.Faction != null)
                {
                    yield return settlement.Faction;
                }
            }
        }
        public override IEnumerable<Dialog_InfoCard.Hyperlink> Hyperlinks
        {
            get
            {
                foreach (Dialog_InfoCard.Hyperlink hyperlink in base.Hyperlinks)
                {
                    yield return hyperlink;
                }
                yield return new Dialog_InfoCard.Hyperlink(requestedThingDef);
            }
        }
        public override void Notify_QuestSignalReceived(Signal signal)
        {
            base.Notify_QuestSignalReceived(signal);
            if (!((signal.tag == (inSignal))))
            {
                return;
            }
            IdeoConversionComp component = settlement.GetComponent<IdeoConversionComp>();
            if (component != null)
            {
                if (component.ActiveRequest)
                {
                    component.currentCount++;
                    return;
                }
                component.requestThingDef = requestedThingDef;
                component.requestCount = requestedCount;
                component.Settle = settlement;
                component.expiration = Find.TickManager.TicksGame + requestDuration;
            }
        }
        public override void AssignDebugData()
        {
            base.AssignDebugData();
            inSignal = "DebugSignal" + Rand.Int;
            settlement = Find.WorldObjects.Settlements.Where(delegate (Settlement x)
            {
                IdeoConversionComp component = x.GetComponent<IdeoConversionComp>();
                return component != null && !component.ActiveRequest && x.Faction != Faction.OfPlayer;
            }).RandomElementWithFallback();
            if (settlement == null)
            {
                settlement = Find.WorldObjects.Settlements.RandomElementWithFallback();
            }
            requestedThingDef = ThingDefOf.Silver;
            requestedCount = 100;
            requestDuration = 60000;
        }
        public override void Cleanup()
        {
            base.Cleanup();
            if (!keepAfterQuestEnds)
            {
                IdeoConversionComp component = settlement.GetComponent<IdeoConversionComp>();
                if (component != null && component.ActiveRequest)
                {
                    component.Disable();
                }
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref inSignal, "inSignal");
            Scribe_References.Look(ref settlement, "settlement");
            Scribe_Defs.Look(ref requestedThingDef, "requestedThingDef");
            Scribe_Values.Look(ref requestedCount, "requestedCount", 0);
            Scribe_Values.Look(ref requestDuration, "requestDuration", 0);
            Scribe_Values.Look(ref keepAfterQuestEnds, "keepAfterQuestEnds", defaultValue: false);
        }
    }
[StaticConstructorOnStartup]

public class IdeoConversionComp : WorldObjectComp
{
    public ThingDef requestThingDef;

    public int requestCount;

    public int currentCount;

    public Settlement Settle;

    public int expiration = -1;

    public string outSignalFulfilled;

    private static readonly Texture2D TradeCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/FulfillTradeRequest");

    public bool ActiveRequest => expiration > Find.TickManager.TicksGame;

    public override string CompInspectStringExtra()
    {
        if (ActiveRequest)
        {
            return "CaravanRequestInfo".Translate(TradeRequestUtility.RequestedThingLabel(requestThingDef, requestCount).CapitalizeFirst(), TradeRequestUtility.RequestedThingLabel(requestThingDef, requestCount-currentCount) + "IdeoFlyers Remaining", (requestThingDef.GetStatValueAbstract(StatDefOf.MarketValue) * (float)requestCount).ToStringMoney());
        }
        return null;
    }
    public override IEnumerable<Gizmo> GetCaravanGizmos(Caravan caravan)
    {
        if (ActiveRequest && CaravanVisitUtility.SettlementVisitedNow(caravan) == parent)
        {
            yield return FulfillRequestCommand(caravan);
        }
    }
    public void Disable()
    {
        expiration = -1;
    }
    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Defs.Look(ref requestThingDef, "requestThingDef");
        Scribe_Values.Look(ref requestCount, "requestCount", 0);
        Scribe_Values.Look(ref expiration, "expiration", 0);
        BackCompatibility.PostExposeData(this);
    }
    private Command FulfillRequestCommand(Caravan caravan)
    {
        Command_Action command_Action = new Command_Action();
        command_Action.defaultLabel = "CommandFulfillTradeOffer".Translate();
        command_Action.defaultDesc = "CommandFulfillTradeOfferDesc".Translate();
        command_Action.icon = TradeCommandTex;
        command_Action.action = delegate
        {
            if (!ActiveRequest)
            {
                Log.Error("Attempted to fulfill an unavailable request");
            }
            else
            {
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("CommandFulfillTradeOfferConfirm".Translate(GenLabel.ThingLabel(requestThingDef, null, currentCount-requestCount)), delegate
                {
                    Fulfill(caravan);
                }));
            }
        };
        return command_Action;
    }
    private void Fulfill(Caravan caravan)
    {
        int remaining = requestCount;
        List<Thing> list = CaravanInventoryUtility.TakeThings(caravan, delegate (Thing thing)
        {
            if (requestThingDef != thing.def)
            {
                return 0;
            }
            if (!PlayerCanGive(thing))
            {
                return 0;
            }
            int num = Mathf.Min(remaining, thing.stackCount);
            remaining -= num;
            return num;
        });
        for (int i = 0; i < list.Count; i++)
        {
            list[i].Destroy();
        }
        if (parent.Faction != null)
        {
            Faction.OfPlayer.TryAffectGoodwillWith(parent.Faction, 12, canSendMessage: true, canSendHostilityLetter: true, HistoryEventDefOf.QuestGoodwillReward);
        }
        QuestUtility.SendQuestTargetSignals(parent.questTags, "TradeRequestFulfilled", parent.Named("SUBJECT"), caravan.Named("CARAVAN"));
        Disable();
    }

    public void fulfillquest()
        {
            QuestUtility.SendQuestTargetSignals(parent.questTags, "TradeRequestFulfilled", parent.Named("SUBJECT"));
            Disable();
        }
    private bool PlayerCanGive(Thing thing)
    {
        if (thing.GetRotStage() != 0)
        {
            return false;
        }
        CompQuality compQuality = thing.TryGetComp<CompQuality>();
        if (compQuality != null && (int)compQuality.Quality < 2)
        {
            return false;
        }
        return true;
    }
}
    public class QuestNode_Sequence_Ideo : QuestNode
    {
        public List<QuestNode> nodes = new List<QuestNode>();

        protected override void RunInt()
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].Run();
            }
        }

        protected override bool TestRunInt(Slate slate)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                if (!nodes[i].TestRun(slate))
                {
                    return false;
                }
            }
            return true;
        }
    }

    public class CompIdeoPropaganda : ThingComp
{

    private CompProperties_IdeoPropaganda Props => (CompProperties_IdeoPropaganda)props;
    private void TryIdeoConversion(int amount, int tile)
    {
        Settlement settlement = Find.WorldObjects.SettlementAt(tile);

        if (settlement != null && CanMakeQuest(settlement))
        {
            Slate slate = new Slate();
            slate.Set("settlement", settlement);
            slate.Set("requestedThing", IdeoDef.IdeoFlyer);
            slate.Set("requestedThingCount", QuestPart_InitiateIdeoConversion.ReqFlyer(settlement));
            QuestUtility.GenerateQuestAndMakeAvailable(IdeoConversionDef.IdeoConversion, slate);
        }
        else
        {
                IdeoConversionComp component = settlement.GetComponent<IdeoConversionComp>();
                if (component.Settle == settlement)
                {
                    foreach (Quest a in Find.QuestManager.QuestsListForReading)
                    {
                        if (a.root.defName == "IdeoConversion") {
                        if (a.GetFirstPartOfType<QuestPart_InitiateIdeoConversion>().settlement != null && a.GetFirstPartOfType<QuestPart_InitiateIdeoConversion>().settlement == settlement)
                            {
                                a.description="A nearby settlement, "+settlement.ToString()+", has started to be converted to your ideoligion use this opportunity to shift the primary ideoligion of their faction.\n They need: "+component.requestThingDef.ToString()+"\n If you want to convert them, drop pod in "+(component.requestCount-component.currentCount)+" more flyers.";
                            }
                        }
                    }
                    component.currentCount++;
                    Log.Message("point added");
                    if (component.currentCount >= component.requestCount)
                    {
                        Log.Message("complete");
                        component.fulfillquest();
                        settlement.Faction.ideos.SetPrimary(Faction.OfPlayer.ideos.PrimaryIdeo);
                            component.currentCount = -999;
                        
                        }        
                }
        }
    }
    public bool CanMakeQuest(Settlement settlement)
    {
            IdeoConversionComp component = settlement.GetComponent<IdeoConversionComp>();
            if(component.Settle==settlement){ return false; }
            return true;
    }
    public override void Notify_AbandonedAtTile(int tile)
    {
        TryIdeoConversion(parent.stackCount, tile);
    }
}

}
