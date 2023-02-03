using HarmonyLib;
using RimWorld;
using Verse;
using UnityEngine;

namespace CM_Who_Next
{
    public class WhoNextMod : Mod
    {
        private static WhoNextMod _instance;
        public static WhoNextMod Instance => _instance;

        public static WhoNextModSettings settings;

        public WhoNextMod(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("CM_Who_Next");
            harmony.PatchAll();

            _instance = this;

            settings = GetSettings<WhoNextModSettings>();
        }

        public override string SettingsCategory()
        {
            return "Who's Next?";
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
