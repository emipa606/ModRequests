using System.Collections.Generic;
using HugsLib;
using HugsLib.Settings;
using HugsLib.Utils;
using Verse;

namespace IHaveMaterials
{
    public class IHaveMaterials : ModBase
    {
        public override string ModIdentifier => "me.lubar.IHaveMaterials";

        internal static IHaveMaterials Instance;
        internal static ModLogger InstanceLogger => Instance.Logger;

        public SettingHandle<bool> StonyFromMap;

        public readonly Dictionary<ThingDef, SettingHandle<bool>> Stuff = new Dictionary<ThingDef, SettingHandle<bool>>();

        public IHaveMaterials()
        {
            Instance = this;
        }

        public override void DefsLoaded()
        {
            StonyFromMap = Settings.GetHandle<bool>(
                "stonyFromMap",
                "IHaveMaterials_stonyFromMap_title".Translate(),
                "IHaveMaterials_stonyFromMap_desc".Translate(),
                true
            );

            Stuff.Clear();

            foreach (var thing in DefDatabase<ThingDef>.AllDefs)
            {
                if (!thing.IsStuff)
                {
                    continue;
                }

                Stuff.Add(thing, Settings.GetHandle<bool>(
                    "stuff_" + thing.defName,
                    "IHaveMaterials_stuff_title".Translate(thing.Named("THING")),
                    "IHaveMaterials_stuff_desc".Translate(thing.description.Named("DESC"), (thing.modContentPack?.Name ?? "Unknown").Named("MOD")),
                    thing.stuffProps.commonality >= 1
                ));
            }
        }
    }
}
