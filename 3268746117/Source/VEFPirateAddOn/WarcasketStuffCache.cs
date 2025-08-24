using RimWorld;
using StuffableCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VEFPirateAddOn
{
    public class WarcasketStuffCache
    {

        private static Dictionary<string, ThingDef> warcasketStuff = new Dictionary<string, ThingDef>();

        public static void AddStuff(string key, ThingDef value)
        {
            warcasketStuff.SetOrAdd(key, value);
        }

        public static ThingDef GetStuff(string key)
        {
            return warcasketStuff.TryGetValue(key, ThingDefOf.Steel);
        }

        public static ThingDef GetStuffWithDefault(ThingDef item)
        {
            if (!warcasketStuff.TryGetValue(item.defName, out ThingDef value))
            {
                ThingDef defaultStuff = SCMod.settings.ArmorSettings.GetDefaultStuffFor(item);
                AddStuff(item.defName, defaultStuff);
                return defaultStuff;
            }
            return value;
        }
    }
}
