using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;

using CM_Callouts.Patches.InteractionWorkers;

namespace CM_Callouts
{
    public class CalloutMod : Mod
    {
        private static CalloutMod _instance;
        public static CalloutMod Instance => _instance;

        public static CalloutModSettings settings;

        public CalloutMod(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("CM_Callouts");
            harmony.PatchAll();

            InteractionWorker_RecruitAttempt_Patches.InteractionWorker_RecruitAttempt_DoRecruit.Patch(harmony);

            _instance = this;
            settings = GetSettings<CalloutModSettings>();
        }

        public override string SettingsCategory()
        {
            return "Callouts!";
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
