using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;


namespace LoadFromStockpileZone
{
    public static class Utilities
    {
        public static bool IsTransfer(this Zone_Stockpile stockpile)
        {
            StBlacklist bl = stockpile.Map.GetBlacklist();
            return !bl.IsBlacklisted(stockpile);
        }

        public static void AddtoBlacklist(this Zone_Stockpile stockpile)
        {
            StBlacklist bl = stockpile.Map.GetBlacklist();
            bl.AddtoBlacklist(stockpile);
        }

        public static void RemovefromBlacklist(this Zone_Stockpile stockpile)
        {
            StBlacklist bl = stockpile.Map.GetBlacklist();
            bl.RemovefromBlacklist(stockpile);
        }

        public static bool IsTransfer(this Building_Storage storage)
        {
            StBlacklist bl = storage.Map.GetBlacklist();
            return !bl.IsBlacklisted(storage);
        }

        public static void AddtoBlacklist(this Building_Storage storage)
        {
            StBlacklist bl = storage.Map.GetBlacklist();
            bl.AddtoBlacklist(storage);
        }

        public static void RemovefromBlacklist(this Building_Storage storage)
        {
            StBlacklist bl = storage.Map.GetBlacklist();
            bl.RemovefromBlacklist(storage);
        }

        public static StBlacklist GetBlacklist(this Map map)
        {
            StBlacklist bl = (StBlacklist)map.components.FirstOrDefault(x => x is StBlacklist);
            if (bl == null)
            {
                bl = new StBlacklist(map);
                map.components.AddDistinct(bl);
            }
            return bl;
        }



    }
}
