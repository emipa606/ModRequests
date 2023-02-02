using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CM_Desperate_Hunger
{
    public class DesperateHungerMod : Mod
    {
        private static DesperateHungerMod _instance;
        public static DesperateHungerMod Instance => _instance;

        public static DesperateHungerModSettings settings;

        public DesperateHungerMod(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("CM_Desperate_Hunger");
            harmony.PatchAll();

            _instance = this;
            settings = GetSettings<DesperateHungerModSettings>();
        }

        public override string SettingsCategory()
        {
            return "Desperate Hunger";
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
