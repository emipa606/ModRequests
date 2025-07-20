using UnityEngine;
using RimWorld;
using Verse;

namespace CM_No_Pawn_Left_Behind
{
    public class RescueMod : Mod
    {
        private static RescueMod _instance;
        public static RescueMod Instance => _instance;

        public static RescueModSettings settings;

        public RescueMod(ModContentPack content) : base(content)
        {
            _instance = this;
            settings = GetSettings<RescueModSettings>();
        }

        public override string SettingsCategory()
        {
            return "No Pawn Left Behind";
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
