using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace StuffableCore.SCCaching
{
    public static class IngredientSelectionCache
    {
        private static Dictionary<string, List<ThingDef>> ingredientCache;

        public static Dictionary<string, List<ThingDef>> IngredientCache { get => ingredientCache = ingredientCache ?? new Dictionary<string, List<ThingDef>>(); set => ingredientCache = value; }

    }
}
