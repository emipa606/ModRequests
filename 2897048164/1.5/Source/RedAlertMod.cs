using UnityEngine;
using Verse;

namespace RedAlert
{
    public class RedAlertMod : Mod
    {
        public static RedAlertSettings settings;
        public RedAlertMod(ModContentPack pack) : base(pack)
        {
            settings = GetSettings<RedAlertSettings>();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            settings.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return this.Content.Name;
        }
    }
}
