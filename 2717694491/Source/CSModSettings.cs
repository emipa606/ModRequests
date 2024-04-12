using Verse;
using UnityEngine;

namespace CustomSchedules
{
    public class CSModSettings : ModSettings
    {
        /// <summary>
        /// The three settings our mod has.
        /// </summary>
        public bool enableScheduleA = true;
        public string scheduleNameA = "Schedule A";
        public bool enableScheduleB = true;
        public string scheduleNameB = "Schedule B";
        public bool enableScheduleC = true;
        public string scheduleNameC = "Schedule C";
        public bool enableScheduleD = true;
        public string scheduleNameD = "Schedule D";
        public bool enableScheduleE = true;
        public string scheduleNameE = "Schedule E";
        public bool enableScheduleF = true;
        public string scheduleNameF = "Schedule F";
       

        /// <summary>
        /// The part that writes our settings to file.
        /// </summary>
        public override void ExposeData()
        {
            Scribe_Values.Look(ref enableScheduleA, "EnableScheduleA", true);
            Scribe_Values.Look(ref scheduleNameA, "scheduleNameA", "Schedule A");
            Scribe_Values.Look(ref enableScheduleB, "EnableScheduleB", true);
            Scribe_Values.Look(ref scheduleNameB, "scheduleNameB", "Schedule B");
            Scribe_Values.Look(ref enableScheduleC, "EnableScheduleC", true);
            Scribe_Values.Look(ref scheduleNameC, "scheduleNameC", "Schedule C");
            Scribe_Values.Look(ref enableScheduleD, "EnableScheduleD", true);
            Scribe_Values.Look(ref scheduleNameD, "scheduleNameD", "Schedule D");
            Scribe_Values.Look(ref enableScheduleE, "EnableScheduleE", true);
            Scribe_Values.Look(ref scheduleNameE, "scheduleNameE", "Schedule E");
            Scribe_Values.Look(ref enableScheduleF, "EnableScheduleF", true);
            Scribe_Values.Look(ref scheduleNameF, "scheduleNameF", "Schedule F");

            base.ExposeData();
        }
    }

    public class CSModStarter : Mod
    {
        /// <summary>
        /// A reference to our settings.
        /// </summary>
        public static CSModSettings settings;

        /// <summary>
        /// A mandatory constructor which resolves the reference to our settings.
        /// </summary>
        /// <param name="content"></param>
        public CSModStarter(ModContentPack content) : base(content)
        {
            settings = GetSettings<CSModSettings>();
        }

        /// <summary>
        /// The (optional) GUI part to set your settings.
        /// </summary>
        /// <param name="inRect">A Unity Rect with the size of the settings window.</param>
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            listingStandard.CheckboxLabeled("Display Schedule 0", ref settings.enableScheduleA);
            if (settings.enableScheduleA)
            {
                listingStandard.Label("Name");
                settings.scheduleNameA = listingStandard.TextEntry(settings.scheduleNameA, 1);
            }

            listingStandard.CheckboxLabeled("Display Schedule 1", ref settings.enableScheduleB);
            if (settings.enableScheduleB)
            {
                listingStandard.Label("Name");
                settings.scheduleNameB = listingStandard.TextEntry(settings.scheduleNameB, 1);
            }

            listingStandard.CheckboxLabeled("Display Schedule 2", ref settings.enableScheduleC);
            if (settings.enableScheduleC)
            {
                listingStandard.Label("Name");
                settings.scheduleNameC = listingStandard.TextEntry(settings.scheduleNameC, 1);
            }

            listingStandard.CheckboxLabeled("Display Schedule 3", ref settings.enableScheduleD);
            if (settings.enableScheduleD)
            {
                listingStandard.Label("Name");
                settings.scheduleNameD = listingStandard.TextEntry(settings.scheduleNameD, 1);
            }

            listingStandard.CheckboxLabeled("Display Schedule 4", ref settings.enableScheduleE);
            if (settings.enableScheduleE)
            {
                listingStandard.Label("Name");
                settings.scheduleNameE = listingStandard.TextEntry(settings.scheduleNameE, 1);
            }

            listingStandard.CheckboxLabeled("Display Schedule 5", ref settings.enableScheduleF);
            if (settings.enableScheduleF)
            {
                listingStandard.Label("Name");
                settings.scheduleNameF = listingStandard.TextEntry(settings.scheduleNameF, 1);
            }
            listingStandard.End();

            base.DoSettingsWindowContents(inRect);
        }

        /// <summary>
        /// Override SettingsCategory to show up in the list of settings.
        /// Using .Translate() is optional, but does allow for localisation.
        /// </summary>
        /// <returns>The (translated) mod name.</returns>
        public override string SettingsCategory() => this.Content.Name;

        public override void WriteSettings()
        {
            base.WriteSettings();

            CSDefOf.CS_SchedA.label = settings.scheduleNameA;
            CSDefOf.CS_SchedB.label = settings.scheduleNameB;
            CSDefOf.CS_SchedC.label = settings.scheduleNameC;
            CSDefOf.CS_SchedD.label = settings.scheduleNameD;
            CSDefOf.CS_SchedE.label = settings.scheduleNameE;
            CSDefOf.CS_SchedF.label = settings.scheduleNameF;
        }
    }
}