using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace GeneExtractorWeights;

public class GeneWeight : IExposable
{
    public float Weight;


    public GeneWeight(string geneDefName, float weight)
    {
        GeneDefName = geneDefName;
        Weight = weight;
    }

    public GeneWeight()
    {
    }

    public string GeneDefName { get; set; }

    public void ExposeData()
    {
        var geneDefName = GeneDefName;
        Scribe_Values.Look(ref geneDefName, nameof(GeneDefName));
        GeneDefName = geneDefName;
        var weight = Weight;
        Scribe_Values.Look(ref weight, nameof(Weight), forceSave: true);
        Weight = weight;
    }
}

public class GeneWeightGroup
{
    public List<GeneWeight> Genes { get; }
    public GeneCategoryDef Category { get; }

    public GeneWeightGroup(GeneCategoryDef category, List<GeneWeight> genes)
    {
        Genes = genes;
        Category = category;
    }
}

public class GeneExtractorWeightsSettings : ModSettings
{
    private Dictionary<string, GeneWeight> _genesDictionary = new();
    private List<GeneWeight> _genes = new();
    private List<GeneWeightGroup> _geneGroups = new();

    internal bool _ignoreMetabolismLimit;
    internal bool _isInitialized;
    internal IntRange _regrowingDays = new(12, 20);

    public IReadOnlyDictionary<string, GeneWeight> GenesDictionary => _genesDictionary;
    public IReadOnlyList<GeneWeight> Genes => _genes;

    public IReadOnlyList<GeneWeightGroup> GeneGroups => _geneGroups;

    public bool IgnoreMetabolismLimit => _ignoreMetabolismLimit;

    public IntRange RegrowingDays => _regrowingDays;

    internal void AddGene(GeneWeight gene)
    {
        _genesDictionary[gene.GeneDefName] = gene;
    }

    public void InitializeGenes()
    {
        Log.Message("[GeneExtractorWeights]: Initializing genes");
        foreach (var gene in GeneExtractorWeights.GenesInOrder)
        {
            if (GenesDictionary.ContainsKey(gene.defName))
                continue;

            AddGene(new GeneWeight(gene.defName, GetDefaultWeight(gene)));
        }

        foreach (var gene in GenesDictionary.Values.ToList())
            if (GeneExtractorWeights.GenesInOrder.All(g => g.defName != gene.GeneDefName))
            {
                Log.Warning(
                    $"GeneExtractorWeights: Gene {gene.GeneDefName} is not in the list of genes. Removing it from the list.");
                _genesDictionary.Remove(gene.GeneDefName);
            }

        _genesDictionary = _genesDictionary
            .OrderBy(g => GeneExtractorWeights.GenesInOrder.IndexOf(GeneExtractorWeights.GenesInOrder.First(gg =>
                string.Equals(gg.defName, g.Value.GeneDefName, StringComparison.Ordinal))))
            .ToDictionary(g => g.Key, g => g.Value);
        _genes = _genesDictionary.Values.ToList();

        _geneGroups = _genes.GroupBy(g => DefDatabase<GeneDef>.GetNamed(g.GeneDefName).displayCategory).Select(g => new GeneWeightGroup(g.Key, g.ToList())).ToList();
        _isInitialized = true;
    }

    private static int GetDefaultWeight(GeneDef gene)
    {
        return gene.biostatArc > 0 || gene.endogeneCategory == EndogeneCategory.Melanin ? 0 : gene.biostatCpx > 0 ? 3 : 1;
    }

    public override void ExposeData()
    {
        Scribe_Values.Look(ref _ignoreMetabolismLimit, nameof(IgnoreMetabolismLimit));

        int minRegrowingDays = _regrowingDays.min;
        int maxRegrowingDays = _regrowingDays.max;

        Scribe_Values.Look(ref minRegrowingDays, "MinRegrowingDays");
        Scribe_Values.Look(ref maxRegrowingDays, "MaxRegrowingDays");
        _regrowingDays = new(minRegrowingDays, maxRegrowingDays);

        var genes = GenesDictionary.Values.ToList();
        Scribe_Collections.Look(ref genes, nameof(GenesDictionary), LookMode.Deep);

        _genesDictionary = genes.ToDictionary(g => g.GeneDefName);

        base.ExposeData();
    }

    public void ResetToDefault()
    {
        _ignoreMetabolismLimit = false;
        _regrowingDays = new(12, 20);

        foreach (var gene in GenesDictionary.Values)
        {
            var geneDef = DefDatabase<GeneDef>.GetNamed(gene.GeneDefName);
            gene.Weight = GetDefaultWeight(geneDef);
        }
    }
}