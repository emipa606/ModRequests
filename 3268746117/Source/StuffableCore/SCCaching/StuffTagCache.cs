using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace StuffableCore.SCCaching
{
    public static class StuffTagCache
    {

        public static Dictionary<string, ThingDef> ImplantsProstheticsCache = new Dictionary<string, ThingDef>();
        public static Dictionary<string, ThingDef> WeaponsCache = new Dictionary<string, ThingDef>();
        public static Dictionary<string, ThingDef> ClothingArmorCache = new Dictionary<string, ThingDef>();

        public static List<Dictionary<string, ThingDef>> AllCaches = new List<Dictionary<string, ThingDef>>()
        {
            ImplantsProstheticsCache, 
            WeaponsCache, 
            ClothingArmorCache
        };
    }
}
