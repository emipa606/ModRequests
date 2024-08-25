using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace BattleQuests;

public class QuestNode_SpawnAllyPawns : QuestNode
{
    [NoTranslate] public SlateRef<string> inSignal;

    protected override bool TestRunInt(Slate slate)
    {
        return true;
    }

    protected override void RunInt()
    {
        var slate = QuestGen.slate;
        var questPart = new QuestPart_SpawnAllyPawns
        {
            settlement = slate.Get<Settlement>("site"),
            allyFaction = slate.Get<Pawn>("asker").Faction,
            points = slate.Get<float>("points"),
            threatLevel = slate.Get<float>("challengeRating", 1),
            inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ??
                       QuestGen.slate.Get<string>("inSignal")
        };
        QuestGen.quest.AddPart(questPart);
    }
}