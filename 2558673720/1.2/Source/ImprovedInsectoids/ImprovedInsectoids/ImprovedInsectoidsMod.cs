using UnityEngine;
using Verse;

namespace ImprovedInsectoids
{
    public class ImprovedInsectoidsMod : Mod
    {
        public static ImprovedInsectoidsModSettings modSettings;

        /// <summary>
        /// Stores mod settings.
        /// </summary>
        public ImprovedInsectoidsMod(ModContentPack content) : base(content)
        {
            modSettings = this.GetSettings<ImprovedInsectoidsModSettings>();
        }

        /// <summary>
        /// Mod settings window UI.
        /// </summary>
        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);

            Listing_Standard window = new Listing_Standard();
            window.Begin(inRect);
            window.CheckboxLabeled("FearEnabledLabel".Translate(), ref modSettings.fearEnabled, "FearEnabledTooltip".Translate());
            window.CheckboxLabeled("FearAffectsPlayerLabel".Translate(), ref modSettings.fearAffectsPlayerFaction, "FearAffectsPlayerTooltip".Translate());
            window.End();
        }

        /// <summary>
        /// Display name in the settings menu.
        /// </summary>
        public override string SettingsCategory()
        {
            return "ImprovedInsectoidsDisplayName".Translate();
        }
    }
}
