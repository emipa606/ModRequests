// BetterTimeFormat - ModSettings.cs
// Created on 2022.11.09
// Last modified at 2022.11.14 16:50

#region
using HarmonyLib;

using UnityEngine;

using Verse;
#endregion

namespace BetterTimeFormat
{
    public class BetterTimeFormatSettings : ModSettings
    {
        public string AmString = "AM";
        public string PmString = "PM";
        public string TimeFormat = "HH:MM";
        public bool TwelveHourFormat;
        public bool UpdateHours;
        public bool UpdateMinutes;
        public bool UpdateSeconds;
        public bool UpdateTime;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref TimeFormat, "BetterTimeFormatLabel", "HH:MM");
            Scribe_Values.Look(ref AmString, "BetterTimeFormatAMLabel", "AM");
            Scribe_Values.Look(ref PmString, "BetterTimeFormatPMLabel", "PM");
            Scribe_Values.Look(ref TwelveHourFormat, "BetterTimeFormat12hLabel");
            Scribe_Values.Look(ref UpdateHours, "BetterTimeUpdateHours", true);
            Scribe_Values.Look(ref UpdateMinutes, "BetterTimeUpdateMinutes", true);
            Scribe_Values.Look(ref UpdateSeconds, "BetterTimeUpdateSeconds");
            Scribe_Values.Look(ref UpdateTime, "BetterTimeUpdateTime", true);
            base.ExposeData();
        }
    }

    public class BetterTimeFormatMod : Mod
    {
        public static BetterTimeFormatSettings Settings;

        public BetterTimeFormatMod(ModContentPack content) : base(content)
        {
            Log.Message("[BetterTimeFormat] Applying Harmony patch.");
            new Harmony(Content.PackageIdPlayerFacing).PatchAll();
            Settings = GetSettings<BetterTimeFormatSettings>();
        }

        public override string SettingsCategory()
        {
            return "BetterTimeFormatCategoryLabel".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var lS = new Listing_Standard();
            lS.Begin(inRect);
            lS.verticalSpacing = 12f;

            lS.Label("BetterTimeFormatDesc".Translate());
            Settings.TimeFormat = lS.TextEntryLabeled("BetterTimeFormatLabel".Translate(), Settings.TimeFormat);

            lS.Label("BetterTimeFormat12hDesc".Translate());
            lS.CheckboxLabeled("BetterTimeFormat12hLabel".Translate(), ref Settings.TwelveHourFormat);

            if (Settings.TwelveHourFormat)
            {
                lS.Label("BetterTimeFormatAMDesc".Translate());
                Settings.AmString = lS.TextEntryLabeled("BetterTimeFormatAMLabel".Translate(), Settings.AmString);

                lS.Label("BetterTimeFormatPMDesc".Translate());
                Settings.PmString = lS.TextEntryLabeled("BetterTimeFormatPMLabel".Translate(), Settings.PmString);
            }

            lS.End();

            Settings.UpdateSeconds = Settings.TimeFormat.Contains("S");
            Settings.UpdateMinutes = Settings.TimeFormat.Contains("M");
            Settings.UpdateHours = Settings.TimeFormat.Contains("H");

            Settings.UpdateTime = Settings.UpdateHours || Settings.UpdateMinutes || Settings.UpdateSeconds;

            base.DoSettingsWindowContents(inRect);
        }
    }
}
