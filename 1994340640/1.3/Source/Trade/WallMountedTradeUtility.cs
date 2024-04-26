using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace WallStuff
{
    class WallMountedTradeUtility
    {
		public static void LaunchThingsOfType(ThingDef resDef, int debt, Map map, TradeShip trader)
		{
			while (debt > 0)
			{
				Thing thing = null;
				foreach (WallTradeBeacon item in WallTradeBeacon.AllPowered(map))
				{
					foreach (IntVec3 tradeableCell in item.TradeableCells)
					{
						foreach (Thing item2 in map.thingGrid.ThingsAt(tradeableCell))
						{
							if (item2.def != resDef)
							{
								continue;
							}
							thing = item2;
							goto IL_009d;
						}
					}
				}
				goto IL_009d;
				IL_009d:
				if (thing == null)
				{
					Log.Error(string.Concat("Could not find any ", resDef, " to transfer to trader."));
					break;
				}
				int num = Math.Min(debt, thing.stackCount);
				if (trader != null)
				{
					trader.GiveSoldThingToTrader(thing, num, TradeSession.playerNegotiator);
				}
				else
				{
					thing.SplitOff(num).Destroy();
				}
				debt -= num;
			}
		}

		public static IEnumerable<Thing> AllLaunchableThingsForTrade(IEnumerable<Thing> values, Map map, ITrader trader = null)
		{
			HashSet<Thing> yieldedThings = new HashSet<Thing>();
			foreach (Thing item in values)
			{
				if (!yieldedThings.Contains(item))
				{
					yieldedThings.Add(item);
					yield return item;
				}
			}
			foreach (WallTradeBeacon item in WallTradeBeacon.AllPowered(map))
			{
				foreach (IntVec3 tradeableCell in item.TradeableCells)
				{
					List<Thing> thingList = tradeableCell.GetThingList(map);
					for (int i = 0; i < thingList.Count; i++)
					{
						Thing thing = thingList[i];
						if (thing.def.category == ThingCategory.Item && TradeUtility.PlayerSellableNow(thing, trader) && !yieldedThings.Contains(thing))
						{
							yieldedThings.Add(thing);
							yield return thing;
						}
					}
				}
			}
		}

	}
}
