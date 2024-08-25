using System.Linq;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace BattleQuests;

public class QuestNode_GetNearbyEnemyBase : QuestNode
{
    public SlateRef<float> maxTileDistance;

    public SlateRef<Thing> mustBeHostileToFactionOf;

    [NoTranslate] public SlateRef<string> storeAs;

    [NoTranslate] public SlateRef<string> storeFactionAs;

    [NoTranslate] public SlateRef<string> storeFactionLeaderAs;

    private Settlement RandomNearbyEnemySettlement(int originTile, Slate slate)
    {
        return Find.WorldObjects.SettlementBases.OrderBy(x => Find.WorldGrid.ApproxDistanceInTiles(originTile, x.Tile))
            .Where(delegate(Settlement settlement)
            {
                if (!settlement.Faction.HostileTo(Faction.OfPlayer) || mustBeHostileToFactionOf.GetValue(slate) != null
                    && !mustBeHostileToFactionOf.GetValue(slate).Faction.HostileTo(settlement.Faction))
                {
                    return false;
                }

                return Find.WorldGrid.ApproxDistanceInTiles(originTile, settlement.Tile) <
                    maxTileDistance.GetValue(slate) && Find.WorldReachability.CanReach(originTile, settlement.Tile);
            }).RandomElementWithFallback();
    }

    protected override void RunInt()
    {
        var slate = QuestGen.slate;
        var map = QuestGen.slate.Get<Map>("map");
        var settlement = RandomNearbyEnemySettlement(map.Tile, slate);
        if (settlement != null)
        {
            var comp = Current.Game.GetComponent<QuestManager>();
            comp.threatLevels[settlement] = slate.Get<float>("challengeRating", 1);
            comp.allyFactions[settlement] = slate.Get<Pawn>("asker").Faction;
        }

        QuestGen.slate.Set(storeAs.GetValue(slate), settlement);
        if (!string.IsNullOrEmpty(storeFactionAs.GetValue(slate)))
        {
            QuestGen.slate.Set(storeFactionAs.GetValue(slate), settlement?.Faction);
        }

        if (!storeFactionLeaderAs.GetValue(slate).NullOrEmpty())
        {
            QuestGen.slate.Set(storeFactionLeaderAs.GetValue(slate), settlement?.Faction.leader);
        }
    }

    protected override bool TestRunInt(Slate slate)
    {
        var map = slate.Get<Map>("map");
        var settlement = RandomNearbyEnemySettlement(map.Tile, slate);
        if (settlement == null)
        {
            return false;
        }

        slate.Set(storeAs.GetValue(slate), settlement);
        if (!string.IsNullOrEmpty(storeFactionAs.GetValue(slate)))
        {
            slate.Set(storeFactionAs.GetValue(slate), settlement.Faction);
        }

        if (!string.IsNullOrEmpty(storeFactionLeaderAs.GetValue(slate)))
        {
            slate.Set(storeFactionLeaderAs.GetValue(slate), settlement.Faction.leader);
        }

        return true;
    }
}