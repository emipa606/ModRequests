using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Reload.Data
{
    public class StockDatabase : WorldComponent, IExposable
    {
        Dictionary<int, Stock> _pawnStockPolicyTracker = new Dictionary<int, Stock>();

        List<Stock> _stocks = new List<Stock>();

        List<int> _tempPawnID = new List<int>();

        List<Stock> _tempPawnStock = new List<Stock>();

        public List<Stock> AllStocks => _stocks;

        public StockDatabase(World world) : base(world)
        {
            
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref _pawnStockPolicyTracker, "pawnStockPolicyTracker", LookMode.Value, LookMode.Reference, ref _tempPawnID, ref _tempPawnStock);
            Scribe_Collections.Look(ref _stocks, "stocks", LookMode.Deep);
        }
        public Stock DefaultStock()
        {
            if(_stocks.Count == 0)
            {
                MakeNewStock();
            }
            return _stocks[0];
        }
        public AcceptanceReport TryDelete(Stock stock)
        {
            foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive)
            {
                if (GetStock(item) == stock)
                    return new AcceptanceReport("StockPolicyInUse".Translate(item));
            }

            foreach (Pawn item2 in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead)
            {
                if (GetStock(item2) == stock)
                {
                    SetStock(item2, null);
                }
            }
            _stocks.Remove(stock);
            return AcceptanceReport.WasAccepted;
        }
        public Stock MakeNewStock()
        {
            int uniqueId = _stocks.Any() ? (_stocks.Max((Stock o) => o.uniqueId) + 1) : 1;
            Stock stock = new Stock(uniqueId, $"{"StockPolicy".Translate()} {uniqueId}");
            _stocks.Add(stock);
            return stock;
        }
        public Stock GetStock(Pawn pawn)
        {
            if(_pawnStockPolicyTracker.TryGetValue(pawn.thingIDNumber, out Stock stock))
                return stock;
            stock = DefaultStock();
            _pawnStockPolicyTracker[pawn.thingIDNumber] = stock;
            return stock;
        }
        public void SetStock(Pawn pawn, Stock stock)
        {
            _pawnStockPolicyTracker.SetOrAdd(pawn.thingIDNumber, stock);
        }
    }
}