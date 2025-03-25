using UnityEngine;
using Verse;

namespace SimpleSlaveryCollars
{
    class SimpleSlaveryCollarsMod : Mod
    {
        public static SimpleSlaveryCollarsSetting settings;
        public SimpleSlaveryCollarsMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<SimpleSlaveryCollarsSetting>();
        }
        public override string SettingsCategory() => "Simple Slavery Collars";
        public override void DoSettingsWindowContents(Rect inRect) => settings.DoSettingsWindowContents(inRect);
    }
}
