using RimWorld;
using StuffableCore.SCCaching;
using StuffableCore.SCComps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace StuffableCore.Settings
{
    public class ImplantProstheticSettings : StuffableCategorySettings
    {
        public ImplantProstheticSettings()
        {
            SettingsLabel = "Implant/prosthetic settings";
        }

        public override void Initialize()
        {
            base.Initialize();
            foreach (HediffDef hediffDef in DefDatabase<HediffDef>.AllDefsListForReading.Where(i => i.tags != null && i.tags.Contains(SCConstants.stuffableHediff)))
            {
                if (hediffDef.comps == null)
                    hediffDef.comps = new List<HediffCompProperties>();
                hediffDef.comps.Add(new HediffCompProperties_Stuffable());
            }
        }

        public override bool ApplySearch(ThingDef item)
        {
            string name = item.defName.ToLower();
            bool flag1 = item.techHediffsTags.NotNullAndContains(SCConstants.stuffableBodyPartTag) && item.recipeMaker != null;
            List<ThingCategoryDef> tCat = item.thingCategories;
            bool flag2 = false;
            bool flag3 = false;
            if (!tCat.NullOrEmpty())
            {
                flag2 = tCat.Contains(ThingCategoryDefOf.BodyParts) && (name.Contains("prosthetic") || name.Contains("bionic") || name.Contains("archotech"));
                flag3 = tCat.Contains(SCDefOf.BodyPartsProsthetic) || tCat.Contains(SCDefOf.BodyPartsBionic) || tCat.Contains(SCDefOf.BodyPartsArchotech);
            }
            bool flag = flag1 || flag2 || flag3;
            if(flag)
                CacheUtil.AddToCache(item.defName, item, StuffTagCache.ImplantsProstheticsCache);
            return flag;
        }

        public override bool ApplyAltSearch(ThingDef item)
        {
            return false;
        }
    }
}
