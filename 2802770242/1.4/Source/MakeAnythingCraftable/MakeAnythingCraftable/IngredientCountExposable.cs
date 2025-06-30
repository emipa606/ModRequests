using System.Collections.Generic;
using Verse;

namespace MakeAnythingCraftable
{
    public class IngredientCountExposable : IExposable
    {
        public float count;
        public List<string> thingDefs = new List<string>();
        public List<string> categories = new List<string>();
        public void ExposeData()
        {
            Scribe_Values.Look(ref count, "count");
            Scribe_Collections.Look(ref thingDefs, "thingDefs", LookMode.Value);
            Scribe_Collections.Look(ref categories, "categories", LookMode.Value);
        }
    }
}
