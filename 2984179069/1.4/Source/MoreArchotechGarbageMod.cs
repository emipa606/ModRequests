using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace MoreArchotechGarbage
{
    public class MoreArchotechGarbageMod : Mod
    {
        public static MoreArchotechGarbageSettings settings;
        public MoreArchotechGarbageMod(ModContentPack pack) : base(pack)
        {
            settings = GetSettings<MoreArchotechGarbageSettings>();
            new Harmony("MoreArchotechGarbageMod").PatchAll();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            settings.DoSettingsWindowContents(inRect);
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
            Startup.ApplySettings();
        }

        public override string SettingsCategory()
        {
            return Content.Name;
        }
    }

    public class MoreArchotechGarbageSettings : ModSettings
    {
        public static float archotechGeneExtractorPower = 400;
        public static int archotechGeneExtractorDuration = 30000;
        public static float archotechGeneRipperPower = 400;
        public static int archotechGeneRipperDuration = 30000;
        public static int archotechGeneRipperMaxSelectableGenes = 6;
        public static bool archotechGeneRipperAllowFreshDeadBodies = true;
        public static IntRange archotechGeneRipperRandomAmountOfExtractedGenesRange = new IntRange(1, 6);
        public static int architeGenepackCountDownDuration = 3600000;
        public static IntRange architeGenepackAssemblerRandomAmountOfGenesRange = new IntRange(1, 6);
        public static int architeCapsuleMatterCraftCost = 500;
        public static int architeCapsuleFragmentCraftCost = 100;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref archotechGeneExtractorPower, "archotechGeneExtractorPower", 400);
            Scribe_Values.Look(ref archotechGeneExtractorDuration, "archotechGeneExtractorDuration", 30000);
            Scribe_Values.Look(ref archotechGeneRipperPower, "archotechGeneRipperPower", 400);
            Scribe_Values.Look(ref archotechGeneRipperDuration, "archotechGeneRipperDuration", 30000);
            Scribe_Values.Look(ref archotechGeneRipperMaxSelectableGenes, "archotechGeneRipperMaxSelectableGenes", 6);
            Scribe_Values.Look(ref archotechGeneRipperAllowFreshDeadBodies, "archotechGeneRipperAllowFreshDeadBodies", true);
            Scribe_Values.Look(ref archotechGeneRipperRandomAmountOfExtractedGenesRange, "archotechGeneRipperRandomAmountOfExtractedGenesRange", new IntRange(1, 6));
            Scribe_Values.Look(ref architeGenepackCountDownDuration, "architeGenepackCountDownDuration", 3600000);
            Scribe_Values.Look(ref architeGenepackAssemblerRandomAmountOfGenesRange, "architeGenepackAssemblerRandomAmountOfGenesRange", new IntRange(1, 6));
            Scribe_Values.Look(ref architeCapsuleMatterCraftCost, "architeCapsuleMatterCraftCost", 500);
            Scribe_Values.Look(ref architeCapsuleFragmentCraftCost, "architeCapsuleFragmentCraftCost", 100);
        }

        public void DoSettingsWindowContents(Rect inRect)
        {
            var ls = new Listing_Standard();
            ls.Begin(inRect);
            archotechGeneExtractorPower = ls.SliderLabeled("Archotech Gene Extractor: power draw: " + archotechGeneExtractorPower, archotechGeneExtractorPower, 0, 2000);
            archotechGeneExtractorDuration = (int)ls.SliderLabeled("Archotech Gene Extractor: process duration: " + archotechGeneExtractorDuration.ToStringTicksToPeriod(), archotechGeneExtractorDuration, 0, 300000);
            ls.GapLine();
            archotechGeneRipperPower = ls.SliderLabeled("Archotech Gene Ripper: power draw: " + archotechGeneRipperPower, archotechGeneRipperPower, 0, 2000);
            archotechGeneRipperDuration = (int)ls.SliderLabeled("Archotech Gene Ripper: process duration: " + archotechGeneRipperDuration.ToStringTicksToPeriod(), archotechGeneRipperDuration, 0, 300000);
            archotechGeneRipperMaxSelectableGenes = (int)ls.SliderLabeled("Archotech Gene Ripper: max selectable genes amount: " + archotechGeneRipperMaxSelectableGenes, 
                archotechGeneRipperMaxSelectableGenes, 1, 30);
            ls.Label("Archotech Gene Ripper extracted genes amount range");
            ls.IntRange(ref archotechGeneRipperRandomAmountOfExtractedGenesRange, 1, 30);
            ls.CheckboxLabeled("Archotech Gene Ripper: allow fresh dead bodies", ref archotechGeneRipperAllowFreshDeadBodies);
            ls.GapLine();
            architeGenepackCountDownDuration = (int)ls.SliderLabeled("Archite Genepack assembler: work duration: " + architeGenepackCountDownDuration.ToStringTicksToPeriod(), 
                architeGenepackCountDownDuration, 0, 3600000 * 5);
            ls.Label("Archite Genepack assembler: archite genes in genepack amount range");
            ls.IntRange(ref architeGenepackAssemblerRandomAmountOfGenesRange, 1, 30);
            ls.GapLine();
            architeCapsuleMatterCraftCost = (int)ls.SliderLabeled("Archite capsule recipe: archotech mass count: " 
                + architeCapsuleMatterCraftCost, architeCapsuleMatterCraftCost, 1, 2000);
            architeCapsuleFragmentCraftCost = (int)ls.SliderLabeled("Archite capsule recipe: archotech fragment count: " 
                + architeCapsuleFragmentCraftCost, architeCapsuleFragmentCraftCost, 1, 2000);
            ls.End();
        }
	}
}
