using System.Collections.Generic;
using Verse;

namespace Reload.Data
{
    public class Stock : IExposable, ILoadReferenceable
    {
        public int uniqueId;

        public string label;

        Dictionary<ThingDef, int> _items = new Dictionary<ThingDef, int>();

        List<ThingDef> _tempKey = new List<ThingDef>();

        List<int> _tempValue = new List<int>();

        public Dictionary<ThingDef, int> Items => _items;

        public Stock()
        {
        }
        public Stock(int uniqueId, string label)
        {
            this.uniqueId = uniqueId;
            this.label = label;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref uniqueId, "uniqueId", 0);
            Scribe_Values.Look(ref label, "label");
            Scribe_Collections.Look(ref _items, "items",LookMode.Def, LookMode.Value, ref _tempKey, ref _tempValue);
        }
        public string GetUniqueLoadID()
        {
            return $"StockPoilcy_{label}{uniqueId}";
        }
        public int GetAmount(ThingDef def)
        {
            if (!_items.TryGetValue(def, out int amount))
                _items[def] = amount = 0;

            return amount;
        }
        public void SetAmount(ThingDef def, int amount)
        {
            _items.SetOrAdd(def, amount);
        }
    }
}