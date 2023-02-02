using UnityEngine;
using Verse;

namespace SYS
{
    [StaticConstructorOnStartup]
    internal class SYSMod : Mod
    {
        /// <summary>
        /// Cunstructor
        /// </summary>
        /// <param name="content"></param>
        public SYSMod(ModContentPack content) : base(content)
        {
            instance = this;
        }

        /// <summary>
        /// The instance-settings for the mod
        /// </summary>
        internal SYSSettings Settings
        {
            get
            {
                if (settings == null)
                {
                    settings = GetSettings<SYSSettings>();
                }
                return settings;
            }
            set => settings = value;
        }

        /// <summary>
        /// The title for the mod-settings
        /// </summary>
        /// <returns></returns>
        public override string SettingsCategory()
        {
            return "Starship Troopers Remastered - Weapons";
        }

        /// <summary>
        /// The settings-window
        /// For more info: https://rimworldwiki.com/wiki/Modding_Tutorials/ModSettings
        /// </summary>
        /// <param name="rect"></param>
        public override void DoSettingsWindowContents(Rect rect)
        {
            var listing_Standard = new Listing_Standard();
            listing_Standard.Begin(rect);
            listing_Standard.Gap();
            listing_Standard.CheckboxLabeled("Weapon always visible", ref Settings.AlwaysVisible);
            listing_Standard.End();
            Settings.Write();
        }

        /// <summary>
        /// The instance of the settings to be read by the mod
        /// </summary>
        public static SYSMod instance;

        /// <summary>
        /// The private settings
        /// </summary>
        private SYSSettings settings;

    }
}
