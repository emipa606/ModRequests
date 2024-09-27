using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace UniqueItems
{
    public class UniqueItemsSettings : ModSettings
    {
        public static Dictionary<string, int> uniqueThingsMaxCount = new Dictionary<string, int>();
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref uniqueThingsMaxCount, "uniqueThingsMaxCount", LookMode.Value, LookMode.Value, ref stringKeys, ref intValues);
        }
        private List<string> stringKeys;
        private List<int> intValues;
        public void DoSettingsWindowContents(Rect inRect)
        {
            Rect rect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
            Listing_Standard ls = new Listing_Standard();
            ls.Begin(rect);
            ls.Label("DM.AdjustUniqueItemsMaxCount".Translate());
            ls.Label("DM.ModSettingsSliderWarning".Translate());
            var keys = uniqueThingsMaxCount.Keys.ToList();
            for (int num = keys.Count - 1; num >= 0; num--)
            {
                var value = uniqueThingsMaxCount[keys[num]];
                var thingDef = DefDatabase<ThingDef>.GetNamedSilentFail(keys[num]);
                if (thingDef != null && thingDef.HasComp(typeof(CompProperties_UniqueItem)));
                {
                    ls.Label(thingDef.defName + " - " + thingDef.GetRareOrUniqueLabel() + ": " + value);
                    value = (int)ls.Slider(value, 1, 100);
                    uniqueThingsMaxCount[keys[num]] = value;
                }
            }
            ls.End();
            base.Write();
        }
    }
}
