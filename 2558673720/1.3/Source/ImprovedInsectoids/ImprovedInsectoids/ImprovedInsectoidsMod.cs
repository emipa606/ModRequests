using UnityEngine;
using Verse;

namespace ImprovedInsectoids
{
    public class ImprovedInsectoidsMod : Mod
    {
        /// <summary>
        /// Initialises a new instance of <see cref="ImprovedInsectoidsMod"/> and stores mod settings.
        /// </summary>
        public ImprovedInsectoidsMod(ModContentPack content) : base(content)
        {
            ImprovedInsectoidsMod.ModSettings = this.GetSettings<ImprovedInsectoidsModSettings>();
        }
        
        /// <summary>
        /// Stores an instance of the mod settings.
        /// </summary>
        public static ImprovedInsectoidsModSettings ModSettings
        {
            get;
            private set;
        }

        /// <summary>
        /// Mod settings window UI.
        /// </summary>
        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);

            Listing_Standard window = new Listing_Standard();
            window.Begin(inRect);
            window.CheckboxLabeled("FearEnabledLabel".Translate(), ref ImprovedInsectoidsMod.ModSettings.fearEnabled, "FearEnabledTooltip".Translate());
            window.CheckboxLabeled("FearAffectsPlayerLabel".Translate(), ref ImprovedInsectoidsMod.ModSettings.fearAffectsPlayerFaction, "FearAffectsPlayerTooltip".Translate());
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
