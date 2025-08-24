using HarmonyLib;
using RimWorld;
using StuffableCore.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace StuffableCore.SCUtils
{
    public static class ScenarioManager
    {
        public static void UpdateScenarioItem(ThingDef thingdef, StuffableCategorySettings categorySettings)
        {
            StuffCategoryDef cacheStuffCategoryDef = DefDatabase<StuffCategoryDef>.AllDefs.Where(i =>
            {
                categorySettings.stuffCategoriesSetting.TryGetValue(i.defName, out bool val);
                return val;
            }).RandomElement();

            ThingDef defaultThindDef = DefDatabase<ThingDef>.AllDefs.Where(i => i.stuffProps != null && i.stuffProps.categories != null && i.stuffProps.categories.Contains(cacheStuffCategoryDef)).RandomElement();

            foreach (ScenarioDef sceneDef in DefDatabase<ScenarioDef>.AllDefs)
            {
                IEnumerable<ScenPart> parts = sceneDef.scenario.AllParts.Where(part =>
                {
                    return part as ScenPart_ThingCount != null && thingdef.Equals((ThingDef)Traverse.Create(part).Field("thingDef").GetValue());
                });
                foreach (ScenPart_ThingCount part in parts.Cast<ScenPart_ThingCount>())
                {
                    Traverse.Create(part).Field("stuff").SetValue(defaultThindDef);
                }
            }
        }
    }
}
