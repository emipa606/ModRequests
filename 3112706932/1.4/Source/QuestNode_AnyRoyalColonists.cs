using RimWorld;
using RimWorld.QuestGen;
using System.Linq;
using Verse;

namespace ImperialFunctionality
{
    public class QuestNode_AnyRoyalColonists : QuestNode
    {
        public override void RunInt()
        {
        }

        public override bool TestRunInt(Slate slate)
        {
            var map = slate.Get<Map>("map");
            return map.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer).Any(x => x.royalty.HasAnyTitleIn(Faction.OfEmpire));
        }
    }
}
