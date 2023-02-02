using System.Collections.Generic;
using Verse;

namespace VarietyMattersMoreCompat
{
    public static class DefLists
    {
        public static Dictionary<ThingDef, int> desserts = new();
        public static HashSet<ThingDef> archotechs = new();

        public static void Init()
        {
            desserts.Clear();
            archotechs.Clear();

            foreach (var def in DefDatabase<ThingDef>.AllDefs)
            {
                (bool isDessert, bool isArchotech, int quality) = def.defName switch
                {
                    // Vanilla cooking expanded
                    "VCE_SimpleDessert" => (true, false, 0),
                    "VCE_FineDessert" => (true, false, 1),
                    "VCE_LavishDessert" => (true, false, 2),
                    "VCE_GourmetDessert" => (true, false, 3),
                    // More archotech garbage
                    "MAG_ArchotechDessert" => (true, true, 4),
                    "MealArchoAdvanced" => (false, true, -1),
                    "MAG_ArchotechSoup" => (false, true, -1),
                    "MAG_archotechgrill" => (false, true, -1),
                    "MAG_ArchotechBake" => (false, true, -1),
                    // Default
                    _ => (false, false, -1),
                };

                if (isDessert)
                    desserts[def] = quality;
                if (isArchotech)
                    archotechs.Add(def);
            }
        }
    }
}
