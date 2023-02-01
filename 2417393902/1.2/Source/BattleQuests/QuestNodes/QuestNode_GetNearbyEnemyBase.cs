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
	public class QuestNode_GetNearbyEnemyBase : QuestNode
	{
		public SlateRef<float> maxTileDistance;

		[NoTranslate]
		public SlateRef<string> storeAs;

		[NoTranslate]
		public SlateRef<string> storeFactionAs;

		[NoTranslate]
		public SlateRef<string> storeFactionLeaderAs;

		public SlateRef<Thing> mustBeHostileToFactionOf;

		private Settlement RandomNearbyEnemySettlement(int originTile, Slate slate)
		{
			return Find.WorldObjects.SettlementBases.OrderBy(x => Find.WorldGrid.ApproxDistanceInTiles(originTile, x.Tile)).Where(delegate (Settlement settlement)
			{
				if (!settlement.Faction.HostileTo(Faction.OfPlayer) || mustBeHostileToFactionOf.GetValue(slate) != null 
						&& !mustBeHostileToFactionOf.GetValue(slate).Faction.HostileTo(settlement.Faction))
				{
					return false;
				}
				return Find.WorldGrid.ApproxDistanceInTiles(originTile, settlement.Tile) < maxTileDistance.GetValue(slate) && Find.WorldReachability.CanReach(originTile, settlement.Tile);
			}).RandomElementWithFallback();
		}
		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			Map map = QuestGen.slate.Get<Map>("map");
			Settlement settlement = RandomNearbyEnemySettlement(map.Tile, slate);
			if (settlement != null)
            {
				var comp = Current.Game.GetComponent<QuestManager>();
				comp.threatLevels[settlement] = slate.Get<float>("challengeRating", 1);
				comp.allyFactions[settlement] = slate.Get<Pawn>("asker").Faction;
			}
			QuestGen.slate.Set(storeAs.GetValue(slate), settlement);
			if (!string.IsNullOrEmpty(storeFactionAs.GetValue(slate)))
			{
				QuestGen.slate.Set(storeFactionAs.GetValue(slate), settlement.Faction);
			}
			if (!storeFactionLeaderAs.GetValue(slate).NullOrEmpty())
			{
				QuestGen.slate.Set(storeFactionLeaderAs.GetValue(slate), settlement.Faction.leader);
			}
		}
		protected override bool TestRunInt(Slate slate)
		{
			Map map = slate.Get<Map>("map");
			Settlement settlement = RandomNearbyEnemySettlement(map.Tile, slate);
			if (map != null && settlement != null)
			{
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
			return false;
		}
	}
}
