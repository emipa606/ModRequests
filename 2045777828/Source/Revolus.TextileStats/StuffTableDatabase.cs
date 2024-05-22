using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Revolus.TextileStats {
    public class StuffTableDatabase {
        // stats

        public static readonly int StatIndexLabel = 0;
        public static readonly int StatIndexCount = 1;
        public static readonly int StatIndexSpecialCount = 2;

        public static readonly List<KindAndDef> StatDefs = new List<KindAndDef> {
            new KindAndDef(StatKind.Special, SpecialDef.Stat_Label),
            new KindAndDef(StatKind.Special, SpecialDef.Stat_Count),
        };

        public static readonly int CountStatSpecial = 2;
        public static readonly int CountStatOffset, CountStatFactor, CountStatBase;

        // stuffs

        public static readonly StuffCategoryDef StuffCategory_All = SpecialDef.Register<StuffCategoryDef>("StuffCategory_All", "(all)");
        public static readonly StuffCategoryDef StuffCategory_Material = SpecialDef.Register<StuffCategoryDef>("StuffCategory_Material", "(materials)");
        public static readonly StuffCategoryDef StuffCategory_Apparell = SpecialDef.Register<StuffCategoryDef>("StuffCategory_Apparell", "(apparell materials)");
        public static readonly StuffCategoryDef StuffCategory_Building = SpecialDef.Register<StuffCategoryDef>("StuffCategory_Building", "(building materials)");

        public static readonly int CatIndexAll = 0;
        public static readonly int CatIndexMaterial = 1;
        public static readonly int CatIndexApparell = 2;
        public static readonly int CatIndexBuilding = 3;
        public static readonly int CatIndexSpecialCount = 4;

        public static readonly List<StuffCategoryDef> CategoryDefs = new List<StuffCategoryDef> {
            StuffCategory_All,
            StuffCategory_Material,
            StuffCategory_Apparell,
            StuffCategory_Building,
        };

        public static readonly List<List<RowData>> InCategory;

        // implementation

        static StuffTableDatabase() {
            var stricmp = StringComparer.InvariantCultureIgnoreCase;

            var statToIndex = StatDefs.Select((k, v) => (k, v)).ToDictionary(kv => kv.k, kv => kv.v);

            var categoryToIndex = new Dictionary<string, int> {
                { "StuffCategory_All", CatIndexAll },
                { "StuffCategory_Material", CatIndexMaterial },
                { "StuffCategory_Apparell", CatIndexApparell },
                { "StuffCategory_Building", CatIndexBuilding },
            };

            // collect usable thingDefs

            var thingDefs = new List<ThingDef>();
            foreach (var thingDef in DefDatabase<ThingDef>.AllDefs) {
                var stuffProps = thingDef.stuffProps;
                if (stuffProps is null) {
                    continue;
                }

                var categories = stuffProps.categories;
                if (categories is null) {
                    continue;
                }

                foreach (var category in categories) {
                    if (category.defName != null) {
                        thingDefs.Add(thingDef);
                        break;
                    }
                }
            }

            // collect categories
            {
                var categoryDict = new Dictionary<string, StuffCategoryDef>();
                foreach (var thingDef in thingDefs) {
                    foreach (var category in thingDef.stuffProps.categories) {
                        var defName = category.defName;
                        if (defName != null && !categoryDict.ContainsKey(defName)) {
                            categoryDict.Add(defName, category);
                        }
                    }
                }

                var catList = categoryDict.Values.ToList();
                var catIndices = Enumerable.Range(0, catList.Count()).ToList();
                catIndices.SortStable((int l, int r) => stricmp.Compare(catList[l].label, catList[r].label));
                foreach (var catIndex in catIndices) {
                    var category = catList[catIndex];
                    categoryToIndex.Add(category.defName, CategoryDefs.Count);
                    CategoryDefs.Add(category);
                }
            }

            // collect stats
            {
                void collectStatMods(HashSet<Def> dest, List<StatModifier> statMods) {
                    if (statMods is null) {
                        return;
                    }
                    foreach (var statMod in statMods) {
                        var stat = statMod.stat;
                        if (stat != null) {
                            dest.Add(stat);
                        }
                    }
                }

                void addStatMods(HashSet<Def> list, StatKind kind) {
                    foreach (var def in list.OrderBy((Def def) => def.label, stricmp)) {
                        var kindAndDef = new KindAndDef(kind, def);
                        statToIndex.Add(kindAndDef, StatDefs.Count);
                        StatDefs.Add(kindAndDef);
                    }
                }

                var statOffsets = new HashSet<Def>();
                var statFactors = new HashSet<Def>();
                var statBases = new HashSet<Def>();

                foreach (var thingDef in thingDefs) {
                    collectStatMods(statOffsets, thingDef.stuffProps.statOffsets);
                    collectStatMods(statFactors, thingDef.stuffProps.statFactors);
                    collectStatMods(statBases, thingDef.statBases);
                }

                addStatMods(statOffsets, StatKind.Offset);
                addStatMods(statFactors, StatKind.Factor);
                addStatMods(statBases, StatKind.Base);

                CountStatOffset = statOffsets.Count;
                CountStatFactor = statFactors.Count;
                CountStatBase = statBases.Count;
            }

            // memorize stuff
            {
                RowData memorizeStuff(ThingDef thingDef) {
                    var values = new Dictionary<KindAndDef, float>();

                    void addStatModsOfKind(List<StatModifier> statMods, StatKind kind) {
                        if (statMods is null) {
                            return;
                        }

                        foreach (var statMod in statMods) {
                            var stat = statMod.stat;
                            if (stat is null) {
                                continue;
                            }

                            var key = new KindAndDef(kind, stat);
                            if (stat.defName != null || statToIndex.ContainsKey(key)) {
                                values[key] = statMod.value;
                            }
                        }
                    }

                    foreach (var kindAndDef in StatDefs) {
                        switch (kindAndDef.kind) {
                            case StatKind.Factor:
                                values[kindAndDef] = 1;
                                break;
                            case StatKind.Offset:
                                values[kindAndDef] = 0;
                                break;
                            case StatKind.Base:
                                values[kindAndDef] = ((StatDef) kindAndDef.def).defaultBaseValue;
                                break;
                        }
                    }

                    addStatModsOfKind(thingDef.stuffProps.statOffsets, StatKind.Offset);
                    addStatModsOfKind(thingDef.stuffProps.statFactors, StatKind.Factor);
                    addStatModsOfKind(thingDef.statBases, StatKind.Base);

                    var colorTexture = SolidColorMaterials.NewSolidColorTexture(thingDef.stuffProps.color);
                    return new RowData(thingDef, values, colorTexture);
                }

                List<int> makeExtraCategoryIndicesList(IEnumerable<string> strings) {
                    var result = new List<int>();
                    foreach (var categoryName in strings) {
                        if (categoryToIndex.TryGetValue(categoryName, out var index)) {
                            result.Add(index);
                        }
                    }
                    return result;
                }

                var settings = TextileStatsMod.settings;
                var indicesApparell = makeExtraCategoryIndicesList(settings.CategoryApparell);
                var indicesBuilding = makeExtraCategoryIndicesList(settings.StuffTableCategoryBuilding);

                InCategory = (from _ in Enumerable.Range(0, categoryToIndex.Count) select new List<RowData>()).ToList();

                foreach (var thingDef in thingDefs) {
                    var categoriesToAddTo = new List<int>();
                    foreach (var category in thingDef.stuffProps.categories) {
                        var defName = category.defName;
                        if (defName != null && categoryToIndex.TryGetValue(defName, out var index)) {
                            categoriesToAddTo.Add(index);
                        }
                    }
                    if (categoriesToAddTo.Count == 0) {
                        continue;
                    }

                    var addToApparell = (from i in categoriesToAddTo where indicesApparell.Contains(i) select true).Any();
                    var addToBuilding = (from i in categoriesToAddTo where indicesBuilding.Contains(i) select true).Any();
                    if (addToApparell) {
                        categoriesToAddTo.Add(CatIndexApparell);
                    }
                    if (addToBuilding) {
                        categoriesToAddTo.Add(CatIndexBuilding);
                    }
                    if (addToApparell || addToBuilding) {
                        categoriesToAddTo.Add(CatIndexMaterial);
                    }
                    categoriesToAddTo.Add(CatIndexAll);

                    RowData stuff = memorizeStuff(thingDef);
                    foreach (var categoryIndex in categoriesToAddTo) {
                        InCategory[categoryIndex].Add((RowData) stuff);
                    }
                }
            }
        }
    }
}
