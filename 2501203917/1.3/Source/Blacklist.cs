using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;


namespace LoadFromStockpileZone
{
    public class StBlacklist : MapComponent
    {
        private List<Building_Storage> storages = new List<Building_Storage>();
        private List<Zone_Stockpile> stockpiles = new List<Zone_Stockpile>(); 

        public StBlacklist(Map map) : base(map) { }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref storages, "storages", LookMode.Reference);
            Scribe_Collections.Look(ref stockpiles, "stockpiles", LookMode.Reference);

        }

        public void AddtoBlacklist(Building_Storage storage)
        {
            storages.AddDistinct(storage);
        }
        
        public void AddtoBlacklist(Zone_Stockpile stockpile)
        {
            stockpiles.AddDistinct(stockpile);
        }

        public void RemovefromBlacklist(Building_Storage storage)
        {
            storages.Remove(storage);
        }

        public void RemovefromBlacklist(Zone_Stockpile stockpile)
        {
            stockpiles.Remove(stockpile);
        }

        public bool IsBlacklisted(Building_Storage storage)
        {
            return storages.Contains(storage);
        }
        public bool IsBlacklisted(Zone_Stockpile stockpile)
        {
            return stockpiles.Contains(stockpile);
        }


    }
}
