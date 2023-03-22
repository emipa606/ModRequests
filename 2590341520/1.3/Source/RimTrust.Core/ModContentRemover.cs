using RimTrust.Trade;
using RimWorld;
using RimWorld.Planet;
using System.Reflection;
using Verse;

namespace RimTrust.Core
{
    internal class ModContentRemover
    {
        internal static int RemoveAllModContentsFromWorldObjects()
        {
            int num = 0;
            FieldInfo field = typeof(Settlement_TraderTracker).GetField("stock", BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (Settlement settlement in Find.WorldObjects.Settlements)
            {
                if (settlement.trader != null && field.GetValue(settlement.trader) != null)
                {
                    for (int num2 = settlement.trader.StockListForReading.Count - 1; num2 > -1; num2--)
                    {
                        if (settlement.trader.StockListForReading[num2].def == BankDefOf.BankNote || settlement.trader.StockListForReading[num2].GetInnerIfMinified().def == CoreDefOf.TrustLedgerConsole)
                        {
                            num += settlement.trader.StockListForReading[num2].stackCount;
                            settlement.trader.StockListForReading.RemoveAt(num2);
                        }
                    }
                }
            }
            foreach (Caravan caravan in Find.WorldObjects.Caravans)
            {
                foreach (Pawn item in caravan.PawnsListForReading)
                {
                    if (item.inventory != null && item.inventory.innerContainer.Count > 0)
                    {
                        for (int num3 = item.inventory.innerContainer.Count - 1; num3 > -1; num3--)
                        {
                            if (item.inventory.innerContainer[num3].def == BankDefOf.BankNote || item.inventory.innerContainer[num3].GetInnerIfMinified().def == CoreDefOf.TrustLedgerConsole)
                            {
                                num += item.inventory.innerContainer[num3].stackCount;
                                item.inventory.innerContainer.RemoveAt(num3);
                            }
                        }
                    }
                }
            }
            return num;
        }

        internal static int RemoveAllModContentsFromPassingShips()
        {
            int num = 0;
            FieldInfo field = typeof(TradeShip).GetField("things", BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (Map map in Find.Maps)
            {
                foreach (PassingShip passingShip in map.passingShipManager.passingShips)
                {
                    if (passingShip is TradeShip tradeShip)
                    {
                        ThingOwner<Thing> thingOwner = (ThingOwner<Thing>)field.GetValue(tradeShip);
                        for (int num2 = thingOwner.Count - 1; num2 > -1; num2--)
                        {
                            if (thingOwner[num2].def == BankDefOf.BankNote || thingOwner[num2].GetInnerIfMinified().def == CoreDefOf.TrustLedgerConsole)
                            {
                                num += thingOwner[num2].stackCount;
                                thingOwner.RemoveAt(num2);
                            }
                        }
                        field.SetValue(tradeShip, thingOwner);
                    }
                }
            }
            return num;
        }

        internal static int RemoveAllModContentsFromMaps()
        {
            int num = 0;
            foreach (Map map in Find.Maps)
            {
                foreach (Zone zone in map.zoneManager.AllZones)
                {
                    if (zone is Zone_Stockpile stockpile)
                    {
                        stockpile.settings?.filter?.SetAllow(BankDefOf.BankNote, false);
                        stockpile.settings?.filter?.SetAllow(CoreDefOf.TrustLedgerConsole, false);
                        stockpile.settings?.filter?.SetAllow(CoreDefOf.NeuralInterfaceChair, false);
                        stockpile.settings?.filter?.SetAllow(CoreDefOf.NeuralInterfaceChair_TII, false);
                        stockpile.settings?.filter?.SetAllow(CoreDefOf.NutrientTube, false);
                        stockpile.settings?.filter?.SetAllow(CoreDefOf.MealNutrientSupplementPill, false);
                    }
                }

                for (int num2 = map.spawnedThings.Count - 1; num2 > -1; num2--)
                {
                    if (map.spawnedThings[num2].def == BankDefOf.BankNote || map.spawnedThings[num2].def == CoreDefOf.MealNutrientSupplementPill || map.spawnedThings[num2].GetInnerIfMinified().def == CoreDefOf.TrustLedgerConsole || map.spawnedThings[num2].GetInnerIfMinified().def == CoreDefOf.NeuralInterfaceChair || map.spawnedThings[num2].GetInnerIfMinified().def == CoreDefOf.NeuralInterfaceChair_TII || map.spawnedThings[num2].GetInnerIfMinified().def == CoreDefOf.NutrientTube)
                    {
                        Thing thing = map.spawnedThings[num2];
                        num += thing.stackCount;
                        thing.DeSpawn();
                        if (!thing.Destroyed)
                        {
                            thing.Destroy();
                        }
                        if (!thing.Discarded)
                        {
                            thing.Discard();
                        }
                    }
                }
            }
            return num;
        }

        internal static int RemoveAllModContentsFromAllPawns()
        {
            int num = 0;
            foreach (Pawn item in Find.WorldPawns.AllPawnsAliveOrDead)
            {
                if (item.inventory != null && item.inventory.innerContainer.Any)
                {
                    for (int num2 = item.inventory.innerContainer.Count - 1; num2 > -1; num2--)
                    {
                        if (item.inventory.innerContainer[num2].def == BankDefOf.BankNote || item.inventory.innerContainer[num2].def == CoreDefOf.MealNutrientSupplementPill || item.inventory.innerContainer[num2].GetInnerIfMinified().def == CoreDefOf.TrustLedgerConsole || item.inventory.innerContainer[num2].GetInnerIfMinified().def == CoreDefOf.NeuralInterfaceChair || item.inventory.innerContainer[num2].GetInnerIfMinified().def == CoreDefOf.NeuralInterfaceChair_TII || item.inventory.innerContainer[num2].GetInnerIfMinified().def == CoreDefOf.NutrientTube)
                        {
                            num += item.inventory.innerContainer[num2].stackCount;
                            item.inventory.innerContainer.RemoveAt(num2);
                        }
                    }
                }
            }
            foreach (Map map in Find.Maps)
            {
                foreach (Pawn allPawn in map.mapPawns.AllPawns)
                {
                    if (allPawn.inventory != null && allPawn.inventory.innerContainer.Any)
                    {
                        for (int num3 = allPawn.inventory.innerContainer.Count - 1; num3 > -1; num3--)
                        {
                            if (allPawn.inventory.innerContainer[num3].def == BankDefOf.BankNote || allPawn.inventory.innerContainer[num3].def == CoreDefOf.MealNutrientSupplementPill || allPawn.inventory.innerContainer[num3].GetInnerIfMinified().def == CoreDefOf.TrustLedgerConsole || allPawn.inventory.innerContainer[num3].GetInnerIfMinified().def == CoreDefOf.NeuralInterfaceChair || allPawn.inventory.innerContainer[num3].GetInnerIfMinified().def == CoreDefOf.NeuralInterfaceChair_TII || allPawn.inventory.innerContainer[num3].GetInnerIfMinified().def == CoreDefOf.NutrientTube)
                            {
                                num += allPawn.inventory.innerContainer[num3].stackCount;
                                allPawn.inventory.innerContainer.RemoveAt(num3);
                            }
                        }
                    }
                }
            }
            return num;
        }
    }
}