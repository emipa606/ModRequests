using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CM_Less_Shitty_Ambush
{
    public class LessShittyAmbushMod : Mod
    {
        private static LessShittyAmbushMod _instance;
        public static LessShittyAmbushMod Instance => _instance;

        public static AmbushModSettings settings;

        public LessShittyAmbushMod(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("CM_Less_Shitty_Ambush");
            harmony.PatchAll();

            _instance = this;

            settings = GetSettings<AmbushModSettings>();
        }

        public override string SettingsCategory()
        {
            return "Less Shitty Ambushes";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            settings.DoSettingsWindowContents(inRect);
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
            settings.UpdateSettings();
        }
    }
}
