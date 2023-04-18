using System.Collections.Generic;
using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace SimpleWarrants
{
	public class QuestNode_WarrantFailed : QuestNode
	{
        [NoTranslate]
		public SlateRef<string> inSignal;

        protected override bool TestRunInt(Slate slate)
		{
			return true;
		}

        protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			QuestPart_WarrantFailed questPart = new QuestPart_WarrantFailed();
			questPart.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? slate.Get<string>("inSignal");
			questPart.warrant = slate.Get<Warrant>("warrant");
			questPart.warrant.relatedQuest = QuestGen.quest;
			QuestGen.quest.AddPart(questPart);
		}
    }

	public class QuestPart_WarrantFailed : QuestPart
	{
        public override IEnumerable<Faction> InvolvedFactions
		{
			get
			{
				yield return warrant.issuer;
			}
		}

        public string inSignal;
        public Warrant warrant;

        public override void Notify_QuestSignalReceived(Signal signal)
		{
			base.Notify_QuestSignalReceived(signal);
			if (!(signal.tag == inSignal))
			{
				return;
			}
			warrant.issuer.TryAffectGoodwillWith(Faction.OfPlayer, -30);
			WarrantsManager.Instance.acceptedWarrants.Remove(warrant);
		}

        public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref inSignal, "inSignal");
			Scribe_References.Look(ref warrant, "warrant");
		}
    }

}