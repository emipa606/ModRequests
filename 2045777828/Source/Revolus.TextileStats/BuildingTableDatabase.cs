using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Revolus.TextileStats {
    public class BuildingTableDatabase {
        // stats

        public static readonly List<KindAndDef> StatDefs = new List<KindAndDef> {
            new KindAndDef(StatKind.Special, SpecialDef.Stat_Label),
        };

        private static StatDef SpecialDefRegister(string name, string label) {
            var result = SpecialDef.Register<StatDef>(name, label);
            StatDefs.Add(new KindAndDef(StatKind.Special, result));
            return result;
        }

        
        private static readonly StatDef Stat_Building_bed_healPerDay = SpecialDefRegister("Stat_Building_bed_healPerDay", "bed heal/day");
        private static readonly StatDef Stat_Building_bed_maxBodySize = SpecialDefRegister("Stat_Building_bed_maxBodySize", "bed max body size");
        private static readonly StatDef Stat_Building_heatPerTickWhileWorking = SpecialDefRegister("Stat_Building_heatPerTickWhileWorking", "heat/tick while working");
        private static readonly StatDef Stat_Building_mineableDropChance = SpecialDefRegister("Stat_Building_mineableDropChance", "mineable drop chance");
        private static readonly StatDef Stat_Building_mineableNonMinedEfficiency = SpecialDefRegister("Stat_Building_mineableNonMinedEfficiency", "mineable non mined efficiency");
        private static readonly StatDef Stat_Building_mineableScatterCommonality = SpecialDefRegister("Stat_Building_mineableScatterCommonality", "mineable scatter commonality");
        private static readonly StatDef Stat_Building_nutritionCostPerDispense = SpecialDefRegister("Stat_Building_nutritionCostPerDispense", "nutrition cost / dispense");
        private static readonly StatDef Stat_Building_roofCollapseDamageMultiplier = SpecialDefRegister("Stat_Building_roofCollapseDamageMultiplier", "roof collapse damage multiplier");
        private static readonly StatDef Stat_Building_trapPeacefulWildAnimalsSpringChanceFactor = SpecialDefRegister("Stat_Building_trapPeacefulWildAnimalsSpringChanceFactor", "trap peaceful wild animals spring chance factor");
        private static readonly StatDef Stat_Building_turretBurstCooldownTime = SpecialDefRegister("Stat_Building_turretBurstCooldownTime", "turret burst coldown time");
        private static readonly StatDef Stat_Building_turretBurstWarmupTime = SpecialDefRegister("Stat_Building_turretBurstWarmupTime", "turret burst warmup time");
        private static readonly StatDef Stat_Building_turretTopDrawSize = SpecialDefRegister("Stat_Building_turretTopDrawSize", "turret top draw size");
        private static readonly StatDef Stat_Building_uninstallWork = SpecialDefRegister("Stat_Building_uninstallWork", "uninstall work");
        private static readonly StatDef Stat_Building_unpoweredWorkTableWorkSpeedFactor = SpecialDefRegister("Stat_Building_unpoweredWorkTableWorkSpeedFactor", "unpowered work table work speed factor");
        
        public static readonly int StatIndexLabel = 0;

        // things

        private static readonly ThingCategoryDef ThingCategory_All = SpecialDef.Register<ThingCategoryDef>("ThingCategory_All", "(all)");

        public static readonly int CatIndexAll = 0;

        public static readonly List<ThingCategoryDef> CategoryDefs = new List<ThingCategoryDef> {
            ThingCategory_All,
        };

        public static readonly List<List<RowData>> InCategory;

        // implementation

        static BuildingTableDatabase() {
            var stricmp = StringComparer.InvariantCultureIgnoreCase;

            var statToIndex = StatDefs.Select((k, v) => (k, v)).ToDictionary(kv => kv.k, kv => kv.v);

            var categoryToIndex = new Dictionary<string, int> {
                { "ThingCategory_All", CatIndexAll },
            };

            // collect usable thingDefs

            var thingDefs = new List<ThingDef>();
            foreach (var thingDef in DefDatabase<ThingDef>.AllDefs) {
                var building = thingDef.building;
                if (building is null) {
                    continue;
                }

                var categories = thingDef.thingCategories;
                if (categories is null) {
                    continue;
                }

                var thingClass = thingDef.thingClass;
                if (thingClass is null || !typeof(Building).IsAssignableFrom(thingClass)) {
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
                var categoryDict = new Dictionary<string, ThingCategoryDef>();
                foreach (var thingDef in thingDefs) {
                    foreach (var category in thingDef.thingCategories) {
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

                var statBases = new HashSet<Def>();
                foreach (var thingDef in thingDefs) {
                    var statMods = thingDef.statBases;
                    collectStatMods(statBases, thingDef.statBases);
                }
                addStatMods(statBases, StatKind.Base);
            }

            // memorize thing
            
            var defaultBuilding = new BuildingProperties();

            RowData memorizeThing(ThingDef thingDef) {
                var building = thingDef.building;
                var values = new Dictionary<KindAndDef, float>();

                void addValue(StatDef stat, float actualValue, float defaultValue) {
                    if (actualValue != defaultValue) {
                        values.Add(new KindAndDef(StatKind.Special, stat), actualValue);
                    }
                }

                addValue(Stat_Building_uninstallWork, building.uninstallWork, defaultBuilding.uninstallWork);
                addValue(Stat_Building_heatPerTickWhileWorking, building.heatPerTickWhileWorking, defaultBuilding.heatPerTickWhileWorking);
                addValue(Stat_Building_roofCollapseDamageMultiplier, building.roofCollapseDamageMultiplier, defaultBuilding.roofCollapseDamageMultiplier);
                addValue(Stat_Building_bed_healPerDay, building.bed_healPerDay, defaultBuilding.bed_healPerDay);
                addValue(Stat_Building_bed_maxBodySize, building.bed_maxBodySize, defaultBuilding.bed_maxBodySize);
                addValue(Stat_Building_nutritionCostPerDispense, building.nutritionCostPerDispense, defaultBuilding.nutritionCostPerDispense);
                addValue(Stat_Building_turretBurstWarmupTime, building.turretBurstWarmupTime.Average, defaultBuilding.turretBurstWarmupTime.Average);
                addValue(Stat_Building_turretBurstCooldownTime, building.turretBurstCooldownTime, defaultBuilding.turretBurstCooldownTime);
                addValue(Stat_Building_turretTopDrawSize, building.turretTopDrawSize, defaultBuilding.turretTopDrawSize);
                addValue(Stat_Building_mineableNonMinedEfficiency, building.mineableNonMinedEfficiency, defaultBuilding.mineableNonMinedEfficiency);
                addValue(Stat_Building_mineableDropChance, building.mineableDropChance, defaultBuilding.mineableDropChance);
                addValue(Stat_Building_mineableScatterCommonality, building.mineableScatterCommonality, defaultBuilding.mineableScatterCommonality);
                addValue(Stat_Building_trapPeacefulWildAnimalsSpringChanceFactor, building.trapPeacefulWildAnimalsSpringChanceFactor, defaultBuilding.trapPeacefulWildAnimalsSpringChanceFactor);
                addValue(Stat_Building_unpoweredWorkTableWorkSpeedFactor, building.unpoweredWorkTableWorkSpeedFactor, defaultBuilding.unpoweredWorkTableWorkSpeedFactor);

                // foreach (var kindAndDef in StatDefs) {
                //     switch (kindAndDef.kind) {
                //         case StatKind.Factor:
                //             values[kindAndDef] = 1f;
                //             break;
                //         case StatKind.Offset:
                //             values[kindAndDef] = 0f;
                //             break;
                //         case StatKind.Base:
                //             values[kindAndDef] = ((StatDef) kindAndDef.def).defaultBaseValue;
                //             break;
                //     }
                // }

                var statBases = thingDef.statBases;
                if (statBases != null) {
                    foreach (var statMod in statBases) {
                        var stat = statMod.stat;
                        if (stat is null) {
                            continue;
                        }

                        var key = new KindAndDef(StatKind.Base, stat);
                        if (stat.defName != null || statToIndex.ContainsKey(key)) {
                            values[key] = statMod.value;
                        }
                    }
                }

                return new RowData(thingDef, values);
            }

            InCategory = (from _ in Enumerable.Range(0, categoryToIndex.Count) select new List<RowData>()).ToList();

            foreach (var thingDef in thingDefs) {
                var categoriesToAddTo = new List<int>();
                foreach (var category in thingDef.thingCategories) {
                    var defName = category.defName;
                    if (defName != null && categoryToIndex.TryGetValue(defName, out var index)) {
                        categoriesToAddTo.Add(index);
                    }
                }
                if (categoriesToAddTo.Count == 0) {
                    continue;
                }
                categoriesToAddTo.Add(CatIndexAll);

                RowData thing = memorizeThing(thingDef);
                foreach (var categoryIndex in categoriesToAddTo) {
                    InCategory[categoryIndex].Add(thing);
                }
            }
        }
    }
}
