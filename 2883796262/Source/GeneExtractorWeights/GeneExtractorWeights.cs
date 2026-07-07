using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace GeneExtractorWeights;

public class GeneExtractorWeights : Mod
{
    private static List<GeneDef> cachedGeneDefsInOrder;
    private readonly GeneExtractorWeightsSettings _settings;
    //private List<GeneWeight> _genes;
    private float _scrollHeight;

    private Vector2 _scrollPosition = new(0, 0);

    public GeneExtractorWeights(ModContentPack pack) : base(pack)
    {
        var harmony = new Harmony("ObiVayneKenobi.GeneExtractorWeights");
        harmony.PatchAll();

        _settings = GetSettings<GeneExtractorWeightsSettings>();


        Log.Message("GeneExtractorWeights loaded");
    }

    internal static List<GeneDef> GenesInOrder
    {
        get
        {
            if (cachedGeneDefsInOrder == null)
            {
                cachedGeneDefsInOrder = new List<GeneDef>();
                foreach (var allDef in DefDatabase<GeneDef>.AllDefs)
                    //if (allDef.endogeneCategory != EndogeneCategory.Melanin)
                    cachedGeneDefsInOrder.Add(allDef);
                cachedGeneDefsInOrder.SortBy(x => -x.displayCategory.displayPriorityInXenotype,
                    x => x.displayCategory.label, x => x.displayOrderInCategory);
            }

            return cachedGeneDefsInOrder;
        }
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        if (!_settings._isInitialized) _settings.InitializeGenes();

        const float geneIconSize = 90;
        const float rowMargin = 6;
        const float rowHeight = 96;

        var listingStandard = new Listing_Standard();
        listingStandard.Begin(inRect);
        listingStandard.CheckboxLabeled("Ignore metabolism limit", ref _settings._ignoreMetabolismLimit, height: 24,
            tooltip:
            "by default, the gene extractor will not add a gene to the extracted genepack if it would exceed the metabolism limit of the pawn. This option will ignore that limit.");

        TooltipHandler.TipRegion(new Rect(0, 30, inRect.width, 28), (TipSignal)"Minimum and maximum number of days it takes for genes to regrow after extracting");

        Widgets.Label(new Rect(0, 30, inRect.width / 2, 28), "Regrowing days");
        Widgets.IntRange(new(inRect.width / 2,  30,  inRect.width / 2, 28), 30, ref _settings._regrowingDays, 1, 30);


        var viewRect = new Rect(0.0f, 0.0f, inRect.width - 16f, _scrollHeight);
        Widgets.BeginScrollView(inRect with { y = 60, height = inRect.height - 96 }, ref _scrollPosition,
            viewRect);
        
        var geneWeightGroups = VisibleGroups(_scrollPosition.y, inRect.height, out float skippedHeight);
        var top = _scrollPosition.y + skippedHeight;
        
        foreach (var group in geneWeightGroups!)
        {
            DrawGeneWeightGroup(group, top, inRect.width);
            top += GeneCategoryHeight(group);
        }

        if (Event.current.type == EventType.Layout)
            _scrollHeight = _settings.GeneGroups.Sum(GeneCategoryHeight) ;
        Widgets.EndScrollView();

        if (Widgets.ButtonText(new Rect(inRect.width - 100, inRect.height - 30, 100, 30), "Reset"))
        {
            Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("Do you want to reset all settings to vanilla?",
                () =>
                {
                    _settings.ResetToDefault();
                }));
        }

        listingStandard.End();
        base.DoSettingsWindowContents(inRect);
    }

    private static float GeneCategoryHeight(GeneWeightGroup group)
    {
        return (float)(42 +  Math.Ceiling((float)group.Genes.Count / 2) * 96);
    }

    [CanBeNull]
    private IEnumerable<GeneWeightGroup> VisibleGroups(float viewTop, float viewHeight, out float skippedHeight)
    {
        skippedHeight = 0;
        var currentGroupTop = 0f;
        List<GeneWeightGroup> result = new();
        foreach (var group in _settings.GeneGroups)
        {
            var currentGroupHeight = GeneCategoryHeight(group);

            if (currentGroupTop + currentGroupHeight > viewTop)
            {
                
                result.Add(group);
            } else skippedHeight = currentGroupTop + currentGroupHeight;
            currentGroupTop += currentGroupHeight;
            if (currentGroupTop > viewTop + viewHeight) break;
            
        }
        return result;
    }

    private void DrawGeneWeightGroup(GeneWeightGroup group, float top, float width)
    {
        var innerWidth = width - 12;
        var height = GeneCategoryHeight(group);
        Widgets.DrawMenuSection(new Rect(0, top - _scrollPosition.y, width, height - 6));
        Rect innerRect = new(6, top - _scrollPosition.y + 6, innerWidth, height - 12);
        const float geneIconSize = 90;
        const float rowHeight = 96;

        Widgets.Label(innerRect with {width = innerWidth / 4, height = 24}, group.Category.LabelCap);
        float groupWeight = group.Genes.OrderBy(g => g.Weight).Skip(group.Genes.Count / 2).First().Weight;
        var sliderRect = innerRect with {x = innerWidth / 4, width = innerWidth / 4 * 3 - 12, height = 24};
        TooltipHandler.TipRegion(sliderRect, (TipSignal)"Set weight for all genes in this category");
        float newGroupWeight = Widgets.HorizontalSlider(sliderRect, groupWeight, 0, 10, roundTo: .1f);
        if (Math.Abs(newGroupWeight - groupWeight) >= 0.1)
        {
            group.Genes.ForEach(g => g.Weight = newGroupWeight);
        }

        top += 36;
        var left = true;
        for (int row = 0; row < group.Genes.Count; row++)
        {

            var gene = group.Genes[row];
            var geneDef = DefDatabase<GeneDef>.GetNamedSilentFail(gene.GeneDefName);
            if (geneDef == null) continue;
            GeneUIUtility.DrawGeneDef(geneDef,
                new Rect(left ? 6 : width / 2, top + row / 2 * rowHeight - _scrollPosition.y, geneIconSize, geneIconSize),
                GeneType.Endogene, null);
            Widgets.HorizontalSlider(
                new Rect(geneIconSize + 24 + (left ? 6 : width / 2),
                    top + row / 2 * rowHeight + geneIconSize / 2 - 12 - _scrollPosition.y, width / 2 - geneIconSize - 54, 24),
                ref gene.Weight, new FloatRange(0, 10), roundTo: .1f);
            Widgets.Label(
                new Rect(geneIconSize + 24 + (left ? 6 : width / 2),
                    top + row / 2 * rowHeight + geneIconSize / 2 + 12 - _scrollPosition.y, width / 2 - geneIconSize - 54, 24),
                gene.Weight.ToString("0.0"));

            left = !left;
        }

    }

    public override string SettingsCategory()
    {
        return "Gene Extractor Weights";
    }
}