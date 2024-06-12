// Nightvision NightVision Init_Apparel.cs
// 
// 06 12 2018
// 
// 06 12 2018

using System.Collections.Generic;
using Verse;

namespace NightVision
{
    public partial class Initialiser
    {
        public void FindAllValidApparel()
        {
            var headgearCategoryDef = ThingCategoryDef.Named(defName: "Headgear");
            var fullHead = Defs_Rimworld.Head;
            var eyes = Defs_Rimworld.Eyes;

            var allEyeCoveringHeadgearDefs = new HashSet<ThingDef>(
                collection: DefDatabase<ThingDef>.AllDefsListForReading.FindAll(
                    match: def => def.IsApparel
                                   && ((def.thingCategories?.Contains(item: headgearCategoryDef) ?? false)
                                       || def.apparel.bodyPartGroups.Any(predicate: bpg => bpg == eyes || bpg == fullHead)
                                    || def.HasComp(compType: typeof(Comp_NightVisionApparel)))
                )
            );
            var nvApparel = Settings.Store.NVApparel ?? new Dictionary<ThingDef, ApparelVisionSetting>();

            //Add defs that have NV comp
            foreach (ThingDef apparel in allEyeCoveringHeadgearDefs)
            {
                if (apparel.comps.Find(match: comp => comp is CompProperties_NightVisionApparel) is CompProperties_NightVisionApparel)
                {
                    if (!nvApparel.TryGetValue(key: apparel, value: out ApparelVisionSetting setting))
                    {
                        nvApparel[key: apparel] = new ApparelVisionSetting(apparel: apparel);
                    }
                    else
                    {
                        setting.InitExistingSetting(apparel: apparel);
                    }
                }
                else
                {
                    //This attaches a new comp to the def as a placeholder
                    ApparelVisionSetting.CreateNewApparelVisionSetting(apparel: apparel);
                }
            }

            Settings.Store.NVApparel = nvApparel;
            Settings.Store.AllEyeCoveringHeadgearDefs = allEyeCoveringHeadgearDefs;
        }
    }
}