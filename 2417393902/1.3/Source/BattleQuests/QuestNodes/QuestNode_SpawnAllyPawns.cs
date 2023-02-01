using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Grammar;

namespace BattleQuests
{
	public class QuestNode_SpawnAllyPawns : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> inSignal;
		public override bool TestRunInt(Slate slate)
		{
			return true;
		}
		public override void RunInt()
		{
			Slate slate = QuestGen.slate;
			QuestPart_SpawnAllyPawns questPart = new QuestPart_SpawnAllyPawns();
			questPart.settlement = slate.Get<Settlement>("site");
			questPart.allyFaction = slate.Get<Pawn>("asker").Faction;
			questPart.points = slate.Get<float>("points");
			questPart.threatLevel = slate.Get<float>("challengeRating", 1);
			questPart.inSignal = (QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal"));
			QuestGen.quest.AddPart(questPart);
		}
	}
}