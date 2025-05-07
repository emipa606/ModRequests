using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;
using RimWorld;

namespace DTechprinting
{
    class TechprintingSettings : ModSettings
    {

        public static Vector2 scrollPos = new Vector2();

        public static float shardYieldRatio = 1.0f;

        public static bool enableUnlockedTechPrinting = true;
        public static bool enablePrereqUnlocks = true;
        public static bool enablePrereqResearch = true;
        public static bool enableCompletedTechPrinting = false;
        public static bool profitableShards = true;
        public static bool printAllItems = false;
        public static bool addTechprintRequirements = false;
        public static float techLevelToAddPrints = (float)TechLevel.Spacer;
        public static bool weaponsApparelOnly = true;
        public static int numShardsToAdd = 100;
        public static bool splitProjects = false;
        public static bool lateLoad = true;
        public static bool shardBuildings = false;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref shardYieldRatio, "shardYieldRatio", 1f);
            Scribe_Values.Look(ref enableUnlockedTechPrinting, "enableUnlockedTechPrinting", true);
            Scribe_Values.Look(ref enablePrereqUnlocks, "enableCompletedTechPrinting", true);
            Scribe_Values.Look(ref enablePrereqResearch, "enableCompletedTechPrinting", true);
            Scribe_Values.Look(ref enableCompletedTechPrinting, "enableCompletedTechPrinting", false);
            Scribe_Values.Look(ref profitableShards, "profitableShards", true);
            Scribe_Values.Look(ref printAllItems, "printAllItems", false);
            Scribe_Values.Look(ref addTechprintRequirements, "addTechprintRequirements", false);
            Scribe_Values.Look(ref techLevelToAddPrints, "levelToAddPrints", (float)TechLevel.Spacer);
            Scribe_Values.Look(ref weaponsApparelOnly, "weaponsApparelOnly", true);
            Scribe_Values.Look(ref numShardsToAdd, "numShardToAdd", 100);
            Scribe_Values.Look(ref splitProjects, "splitProjects", false);
            Scribe_Values.Look(ref lateLoad, "lateLoad", true);
            Scribe_Values.Look(ref shardBuildings, "shardBuildings", false);
            base.ExposeData();
        }

        public static void WriteAll() // called when settings window closes
        {
            ValidateSettings();
            Base.UpdateAll();
            
        }

        public static void ValidateSettings()
        {
            if (!enableUnlockedTechPrinting)
            {
                enablePrereqResearch = false;
                enableCompletedTechPrinting = false;
                printAllItems = false;
            }
        }

        public static void DrawSettings(Rect rect)
        {
            Listing_Standard ls = new Listing_Standard(GameFont.Small);



            float height = rect.height * 1.5f;
            Rect contents = new Rect(rect.x, rect.y, rect.width - 30f, height);
            Widgets.BeginScrollView(rect, ref scrollPos, contents, true);
            ls.ColumnWidth = contents.width * 2.0f / 3.0f;
            ls.Begin(contents);
            ls.Gap();

            ls.CheckboxLabeled("Generate shard defs late (recommended)", ref lateLoad, "Generates defs as late as possible, and any missing defs on game load. Recommended unless you run into hash collisions, like with PawnMorpher");
            ls.GapLine();

            Rect yieldRect = ls.GetRect(Text.LineHeight);
            Rect yieldLabelRect = yieldRect.LeftPartPixels(300);
            Rect yieldSliderRect = yieldRect.RightPartPixels(yieldRect.width - 300);
            Widgets.Label(yieldLabelRect, "Techshard yield multiplier (Default: 1)");
            shardYieldRatio = Widgets.HorizontalSlider(yieldSliderRect, shardYieldRatio, 0.1f, 3.0f, middleAlignment: false, label: shardYieldRatio.ToString("F1"), leftAlignedLabel: null, rightAlignedLabel: null, roundTo: 0.1f);
            ls.Gap();

            ls.CheckboxLabeled("Allow sharding buildings", ref shardBuildings, "Allow buildings to be turned into shards, and, if enabled, allow shards to be added to projects with only buildings");
            ls.Gap();

            ls.CheckboxLabeled("Enable profitable techshards", ref profitableShards, "Gives techshards a value based on the research project they unlock. Requires a restart to take effect.");
            ls.GapLine();

            ls.CheckboxLabeled("Techshards apply to locked prereqs", ref enablePrereqUnlocks, "Applying techshards to an unlocked project with locked prereqs will help unlock one of the prereqs.");
            ls.Gap();
            ls.CheckboxLabeled("Enable printing after tech unlocked", ref enableUnlockedTechPrinting, "Allows you to continue turning items into techshards even after the associated tech has been unlocked. These shards can be used to boost research in the associated technology, or sold for profit if Profitable Shards is enabled.");
            ls.Gap();
            if (enableUnlockedTechPrinting)
            {
                ls.CheckboxLabeled("Techshards apply to prereq research", ref enablePrereqResearch, "Applying techshards to an unlocked project with unresearched prereqs will boost the research of one of its prereqs.");
                ls.Gap();
                ls.CheckboxLabeled("Enable printing after tech researched", ref enableCompletedTechPrinting, "Allows you to continue turning items into techshards even after the associated tech has been researched. These shards can be sold for profit if Profitable Shards is enabled.");
                ls.Gap();
                ls.CheckboxLabeled("Enable printing items for research with no shard requirement", ref printAllItems, "Allows you to techprint items that were never locked by shards, as long as they have a research requirement. This allows you to boost you research, or, if you have profitable shards enabled, make a profit by printing.");
            }
            else
            {
                ls.Label("Techshards apply to prereq research", tooltip: "(Requires enabling printing after tech unlocked) Applying techshards to an unlocked project with unresearched prereqs will boost the research of one of its prereqs.");
                ls.Gap();
                ls.Label("Enable printing after tech researched", tooltip: "(Requires enabling printing after tech unlocked) Allows you to continue turning items into techshards even after the associated tech has been researched. These shards can be sold for profit if Profitable Shards is enabled.");
                ls.Gap();
                ls.Label("Enable printing items for research with no shard requirement", tooltip: "(Requires enabling printing after tech unlocked) Allows you to techprint items that were never locked by shards, as long as they have a research requirement. This allows you to boost you research, or, if you have profitable shards enabled, make a profit by printing.");
            }

            ls.GapLine();
            ls.Gap();
            ls.Label("Advanced Settings: These options affect gameplay significantly");
            ls.Label("CHANGING REQUIRES RESTART");
            ls.GapLine();
            ls.CheckboxLabeled("Add techshard requirements to projects w/ printables", ref addTechprintRequirements, "Adds a techshard requirement to all projects above a designated tech level and which unlock techprintable items");
            ls.Gap();
            if (addTechprintRequirements)
            {
                ls.CheckboxLabeled("Projects with weapons or apparel only", ref weaponsApparelOnly, "Limit additional techsard requirements to projects that unlock weapons or apparel");
                ls.Gap();
                Rect techLevelRect = ls.GetRect(Text.LineHeight);
                Rect techLabelRect = techLevelRect.LeftPartPixels(300);
                Rect techSliderRect = techLevelRect.RightPartPixels(techLevelRect.width - 300);
                Widgets.Label(techLabelRect, "Minimium tech level to require shards");
                techLevelToAddPrints = Widgets.HorizontalSlider(techSliderRect, techLevelToAddPrints, (float)TechLevel.Animal, (float)TechLevel.Archotech, middleAlignment: false, label: Enum.GetName(typeof(TechLevel), Mathf.RoundToInt(techLevelToAddPrints)), leftAlignedLabel: null, rightAlignedLabel: null, roundTo: 1f);
                ls.Gap();
                Rect numShardsRect = ls.GetRect(Text.LineHeight);
                Rect labelRect = numShardsRect.LeftPartPixels(300);
                Rect sliderRect = numShardsRect.RightPartPixels(numShardsRect.width - 300);
                Widgets.Label(labelRect, "Number of techshards to require");
                numShardsToAdd = Mathf.RoundToInt(Widgets.HorizontalSlider(sliderRect, numShardsToAdd, 100, 300, middleAlignment: false, label: "200", leftAlignedLabel: "100", rightAlignedLabel: "300", roundTo: 100f));
                ls.Gap();
            }
            else
            {
                ls.Label("Projects with weapons or apparel only", tooltip: "(Requires adding techsard requirements to advanced projects) Limit additional techsard requirements to projects that unlock weapons or apparel");
                ls.Gap();
                ls.Label("Add techshard requirements to ALL projects w/ printables", tooltip: "(Requires adding techsard requirements to advanced projects) Adds a techshard requirement to ALL projects which unlock techprintable items");
            }
            ls.GapLine();
            ls.CheckboxLabeled("Split blocking projects", ref splitProjects, "Splits research projects with techshard requirements and that unlock further research into two projects: the original project with all printable items removed, and a new, cheap project locked by techshards.");
            ls.Gap();

            ls.End();
            Widgets.EndScrollView();
        }
           
    }
}
