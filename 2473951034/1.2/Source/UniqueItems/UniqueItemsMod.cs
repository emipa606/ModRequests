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
    public class UniqueItemsMod : Mod
    {
        public static UniqueItemsSettings settings;

        public static string ModName = "Unique Items";
        public UniqueItemsMod(ModContentPack pack) : base(pack)
        {
            settings = GetSettings<UniqueItemsSettings>();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            settings.DoSettingsWindowContents(inRect);
        }
        public override string SettingsCategory()
        {
            return ModName;
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
            UniqueItemsHelper.ApplySettings();
        }
    }

    [StaticConstructorOnStartup]
    public static class UniqueItemsHelper
    {
        static UniqueItemsHelper()
        {
            foreach (var thingDef in DefDatabase<ThingDef>.AllDefs)
            {
                var compProps = thingDef.GetCompProperties<CompProperties_UniqueItem>();
                if (compProps != null)
                {
                    if (!UniqueItemsSettings.uniqueThingsMaxCount.ContainsKey(thingDef.defName))
                    {
                        UniqueItemsSettings.uniqueThingsMaxCount[thingDef.defName] = compProps.maxCount;
                    }
                }
            }
            ApplySettings();
        }

        public static void ApplySettings()
        {
            foreach (var thingDefName in UniqueItemsSettings.uniqueThingsMaxCount)
            {
                var thingDef = DefDatabase<ThingDef>.GetNamedSilentFail(thingDefName.Key);
                if (thingDef != null)
                {
                    var compProps = thingDef.GetCompProperties<CompProperties_UniqueItem>();
                    if (compProps != null && thingDefName.Value != compProps.maxCount)
                    {
                        compProps.maxCount = thingDefName.Value;
                    }
                }
            }
        }
    }
}
