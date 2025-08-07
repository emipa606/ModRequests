using RimWorld;
using UnityEngine;
using Verse;

namespace Stranger_Danger
{
    public class Mod : Verse.Mod
    {
        public Mod(ModContentPack content) : base(content) => GetSettings<Settings>();

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            GetSettings<Settings>().DoWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Stranger Danger";
        }
    }

    static class IDontKnowWhatImDoing
    {
        public static bool VFEMedievalFound = false;
        public static BackstoryDef blankstory = DefDatabase<BackstoryDef>.GetNamed("SD_Hidden");
    }

    //There's gotta be a better way to do this
    class Settings : ModSettings
    {
        public bool hideDead = false;

        public bool activeStory = true;
        public bool activeTraits = true;
        public bool activePassion = true;
        public bool activeSkills = true;

        public bool guiltyStory = true;
        public bool guiltyTraits = true;
        public bool guiltyPassion = true;
        public bool guiltySkills = true;

        public bool innocentStory = false;
        public bool innocentTraits = false;
        public bool innocentPassion = false;
        public bool innocentSkills = false;

        public bool slaveStory = false;
        public bool slaveTraits = false;
        public bool slavePassion = false;
        public bool slaveSkills = false;

        public bool friendStory = false;
        public bool friendTraits = false;
        public bool friendPassion = false;
        public bool friendSkills = false;

        public bool colonistStory = false;
        public bool colonistTraits = false;
        public bool colonistPassion = false;
        public bool colonistSkills = false;

        public static Settings Get()
        {
            return LoadedModManager.GetMod<Stranger_Danger.Mod>().GetSettings<Settings>();
        }

        public void DoWindowContents(Rect canvas)
        {
            var options = new Listing_Standard();

            options.Begin(canvas);
            options.ColumnWidth = canvas.width / 3f;

            options.CheckboxLabeled("Hide when dead?", checkOn: ref hideDead, tooltip: "Checking this option hides the bio of corpses according to the below settings");

            options.ColumnWidth = canvas.width;
            options.GapLine();
            options.End();

            int gaps = 12 * (3 + 1);

            options.Begin(canvas);
            options.ColumnWidth = canvas.width / 4f;

            options.Gap(gaps);
            float dent = options.ColumnWidth - 58;
            options.Indent(dent);
            options.Label("Hide Backstory");
            options.Outdent(dent);
            options.Gap();
            options.CheckboxLabeled("Active Enemies", checkOn: ref activeStory);
            options.Gap();
            options.CheckboxLabeled("Prisoners - Guilty", checkOn: ref guiltyStory);
            options.Gap();
            options.CheckboxLabeled("Prisoners - Innocent", checkOn: ref innocentStory);
            options.Gap();
            options.CheckboxLabeled("Slaves", checkOn: ref slaveStory);
            options.Gap();
            options.CheckboxLabeled("Allies", checkOn: ref friendStory);
            options.Gap();
            options.CheckboxLabeled("Colonists", checkOn: ref colonistStory);

            options.NewColumn();
            options.ColumnWidth = canvas.width / 8f;

            options.Gap(gaps);
            dent = options.ColumnWidth - 45;
            options.Indent(dent);
            options.Label("Hide Traits");
            options.Outdent(dent);
            options.Gap();
            options.CheckboxLabeled("", checkOn: ref activeTraits);
            options.Gap();
            options.CheckboxLabeled("", checkOn: ref guiltyTraits);
            options.Gap();
            options.CheckboxLabeled("", checkOn: ref innocentTraits);
            options.Gap();
            options.CheckboxLabeled("", checkOn: ref slaveTraits);
            options.Gap();
            options.CheckboxLabeled("", checkOn: ref friendTraits);
            options.Gap();
            options.CheckboxLabeled("", checkOn: ref colonistTraits);

            options.NewColumn();

            options.Gap(gaps);
            dent = options.ColumnWidth - 56;
            options.Indent(dent);
            options.Label("Hide Passion");
            options.Outdent(dent);
            options.Gap();
            options.CheckboxLabeled("", checkOn: ref activePassion);
            options.Gap();
            options.CheckboxLabeled("", checkOn: ref guiltyPassion);
            options.Gap();
            options.CheckboxLabeled("", checkOn: ref innocentPassion);
            options.Gap();
            options.CheckboxLabeled("", checkOn: ref slavePassion);
            options.Gap();
            options.CheckboxLabeled("", checkOn: ref friendPassion);
            options.Gap();
            options.CheckboxLabeled("", checkOn: ref colonistPassion);

            options.NewColumn();

            options.Gap(gaps);
            dent = options.ColumnWidth - 45;
            options.Indent(dent);
            options.Label("Hide Skills");
            options.Outdent(dent);
            options.Gap();
            options.CheckboxLabeled("", checkOn: ref activeSkills);
            options.Gap();
            options.CheckboxLabeled("", checkOn: ref guiltySkills);
            options.Gap();
            options.CheckboxLabeled("", checkOn: ref innocentSkills);
            options.Gap();
            options.CheckboxLabeled("", checkOn: ref slaveSkills);
            options.Gap();
            options.CheckboxLabeled("", checkOn: ref friendSkills);
            options.Gap();
            options.CheckboxLabeled("", checkOn: ref colonistSkills);

            options.End();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref hideDead, "HideDead");

            Scribe_Values.Look(ref activeStory, "activeStory");
            Scribe_Values.Look(ref activeTraits, "activeTraits");
            Scribe_Values.Look(ref activePassion, "activePassion");
            Scribe_Values.Look(ref activeSkills, "activeSkills");

            Scribe_Values.Look(ref guiltyStory, "guiltyStory");
            Scribe_Values.Look(ref guiltyTraits, "guiltyTraits");
            Scribe_Values.Look(ref guiltyPassion, "guiltyPassion");
            Scribe_Values.Look(ref guiltySkills, "guiltySkills");

            Scribe_Values.Look(ref innocentStory, "innocentStory");
            Scribe_Values.Look(ref innocentTraits, "innocentTraits");
            Scribe_Values.Look(ref innocentPassion, "innocentPassion");
            Scribe_Values.Look(ref innocentSkills, "innocentSkills");

            Scribe_Values.Look(ref slaveStory, "slaveStory");
            Scribe_Values.Look(ref slaveTraits, "slaveTraits");
            Scribe_Values.Look(ref slavePassion, "slavePassion");
            Scribe_Values.Look(ref slaveSkills, "slaveSkills");

            Scribe_Values.Look(ref friendStory, "friendStory");
            Scribe_Values.Look(ref friendTraits, "friendTraits");
            Scribe_Values.Look(ref friendPassion, "friendPassion");
            Scribe_Values.Look(ref friendSkills, "friendSkills");

            Scribe_Values.Look(ref colonistStory, "colonistStory");
            Scribe_Values.Look(ref colonistTraits, "colonistTraits");
            Scribe_Values.Look(ref colonistPassion, "colonistPassion");
            Scribe_Values.Look(ref colonistSkills, "colonistSkills");
        }

    }
}
