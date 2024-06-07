using System;
using System.Collections.Generic;
using Verse;
using UnityEngine;

namespace zed_0xff.GeneCollectorQOL;

public class MySettings : ModSettings
{
    public bool patchGeneAssembler_addQueue = true;
    public bool patchGeneAssembler_colorizeCombinedPart = true;
    public bool patchGeneAssembler_showCapsules = true;
    public bool patchGeneExtractor_colorize = true;
    public bool patchGene_colorize = true;
    public bool patchXenogerm_hideLetter = true;
    public bool patchXenogerm_prisonersGizmo = true;

    public bool patchAG = true;
    public bool patchRGA = true;
    public bool patchWVC = true;

    public override void ExposeData()
    {
        Scribe_Values.Look(ref patchGeneAssembler_addQueue, "patchGeneAssembler_addQueue", true);
        Scribe_Values.Look(ref patchGeneAssembler_colorizeCombinedPart, "patchGeneAssembler_colorizeCombinedPart", true);
        Scribe_Values.Look(ref patchGeneAssembler_showCapsules, "patchGeneAssembler_showCapsules", true);
        Scribe_Values.Look(ref patchGeneExtractor_colorize, "patchGeneExtractor_colorize", true);
        Scribe_Values.Look(ref patchGene_colorize, "patchGene_colorize", true);
        Scribe_Values.Look(ref patchXenogerm_hideLetter, "patchXenogerm_hideLetter", true);
        Scribe_Values.Look(ref patchXenogerm_prisonersGizmo, "patchXenogerm_prisonersGizmo", true);

        Scribe_Values.Look(ref patchAG, "patchAG", true);
        Scribe_Values.Look(ref patchRGA, "patchRGA", true);
        Scribe_Values.Look(ref patchWVC, "patchWVC", true);
        base.ExposeData();
    }
}

public class ModConfig : Mod
{
    public static MySettings Settings { get; private set; }

    public ModConfig(ModContentPack content) : base(content)
    {
        Settings = GetSettings<MySettings>();
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        Listing_Standard l = new Listing_Standard();
        l.Begin(inRect);

        l.CheckboxLabeled("Genepack/Xenogerm: colorize inspect string", ref Settings.patchGene_colorize);
        l.CheckboxLabeled("Gene Assembler: add xenogerm queue", ref Settings.patchGeneAssembler_addQueue);
        l.CheckboxLabeled("Gene Assembler: draw yellow bg on genes in a combined genepack that we don't have as separate genes", ref Settings.patchGeneAssembler_colorizeCombinedPart);
        l.CheckboxLabeled("Gene Assembler: show available archite capsules count", ref Settings.patchGeneAssembler_showCapsules);
        l.CheckboxLabeled("Gene Extractor: colorize \"Gene extraction complete\" message", ref Settings.patchGeneExtractor_colorize);
        l.CheckboxLabeled("Xenogerm: add \"Implant into a prisoner\" gizmo", ref Settings.patchXenogerm_prisonersGizmo);
        l.CheckboxLabeled("Xenogerm: suppress normal \"Xenogerm implantation ordered\" letter", ref Settings.patchXenogerm_hideLetter);

        l.Gap();

        l.CheckboxLabeled("Alpha Genes: suppress \"Unstable mutation gained\" message", ref Settings.patchAG);
        l.CheckboxLabeled("Random's Gene Assistant: support coloring of all genepacks [game restart required]", ref Settings.patchRGA);
        l.CheckboxLabeled("WVC - Xenotypes and Genes: notify about spawned genepacks [game restart required]", ref Settings.patchWVC);

        l.End();
        base.DoSettingsWindowContents(inRect);
    }

    public override string SettingsCategory()
    {
        return "Gene Collector QoL".Translate();
    }
}
