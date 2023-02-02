using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace Clockwork
{
    public class ClockworkAndSteamSettings : ModSettings
    {

        public static bool steamGuns = false;
        public static bool teslaGun = false;
        public static bool turrets = false;

        public static bool brazenRot = true;
        public static bool brazenRotClockwork = true;
        public static bool brazenRotSteamwork = true;
        public static bool brazenRotIsDeadly = false;
        public static float brazenRotMultiplier = 1f;

        public static bool integratedResearch = false;

        public static bool showSteamSystemID = false;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref steamGuns, "steamGuns", false);
            Scribe_Values.Look(ref teslaGun, "teslaGun", false);
            Scribe_Values.Look(ref turrets, "turrets", false);
            Scribe_Values.Look(ref brazenRot, "brazenRot", true);
            Scribe_Values.Look(ref brazenRotClockwork, "brazenRotClockwork", true);
            Scribe_Values.Look(ref brazenRotSteamwork, "brazenRotSteamwork", true);
            Scribe_Values.Look(ref brazenRotIsDeadly, "brazenRotSteamwork", false);
            Scribe_Values.Look(ref brazenRotMultiplier, "brazenRotMultiplier", 1f);
            Scribe_Values.Look(ref integratedResearch, "integratedResearch", false);
            Scribe_Values.Look(ref showSteamSystemID, "showSteamSystemID", false);
            base.ExposeData();
        }

        private static float height_modifier = 100f;

        public static void DoWindowContents(Rect inRect)
        {
            Rect viewRect = new Rect(0f, 0f, inRect.width - 16f, inRect.height + height_modifier);

            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.maxOneColumn = true;
            listingStandard.ColumnWidth = viewRect.width;
            listingStandard.Begin(inRect);

            Text.Font = GameFont.Medium;
            listingStandard.Label("Research:");
            Text.Font = GameFont.Small;
            listingStandard.Gap(4f);
            listingStandard.CheckboxLabeled("Integrated Research (Requires Reload)", ref integratedResearch, "Integrated research make it so certain vanilla research will require research from this mod as a prerequisite. (Requires reload.)");

            Text.Font = GameFont.Medium;
            listingStandard.Label("Brazen Rot:");
            Text.Font = GameFont.Small;
            listingStandard.Gap(4f);
            listingStandard.CheckboxLabeled("Enable Brazen Rot", ref brazenRot, "Gives colonists a chance to contract brazen rot.");
            if (brazenRot)
            {
                listingStandard.CheckboxLabeled("Clockwork prosthetics cause brazen rot", ref brazenRotClockwork, "Colonists contract brazen rot from clockwork prosthesis.");
                listingStandard.CheckboxLabeled("Steamwork prosthetics cause brazen rot", ref brazenRotSteamwork, "Colonists contract brazen rot from steamwork prosthesis.");
                listingStandard.CheckboxLabeled("Brazen rot can be lethal", ref brazenRotIsDeadly, "Brazen rot can kill colonists after a while. (Changing this midgame can cause mild issues.)");
                listingStandard.Gap(20f);
                listingStandard.Label("Brazen rot multiplier: " + brazenRotMultiplier + "x", -1f, "How quickly brazen rot corrupts the body. By default it takes about 30 days with one organ.");
                brazenRotMultiplier = listingStandard.Slider(brazenRotMultiplier, 0f, 10f);
                if (listingStandard.ButtonText("Reset Multiplier"))
                {
                    brazenRotMultiplier = 1f;
                }
            }

            Text.Font = GameFont.Medium;
            listingStandard.Label("Secrets:");
            Text.Font = GameFont.Small;
            listingStandard.Gap(4f);
            listingStandard.CheckboxLabeled("Bird", ref steamGuns, "Or worm?");

            Text.Font = GameFont.Medium;
            listingStandard.Label("Debug:");
            Text.Font = GameFont.Small;
            listingStandard.Gap(4f);
            listingStandard.CheckboxLabeled("Show steam system ID", ref showSteamSystemID, "Steam system id can be seen on steam components.");
            listingStandard.End();
        }


    }

    public class ClockworkAndSteam : Mod
    {
        ClockworkAndSteamSettings settings;

        public ClockworkAndSteam(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<ClockworkAndSteamSettings>();
        }

        public override string SettingsCategory()
        {
            return "Clockwork And Steam";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            ClockworkAndSteamSettings.DoWindowContents(inRect);
        }

    }
}
