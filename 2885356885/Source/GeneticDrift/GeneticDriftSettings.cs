using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
﻿using RimWorld;
using Verse;
using UnityEngine;

namespace GeneticDrift
{
    class GeneChancesList : List<float>
    {
        public override string ToString()
        {
            return string.Join(";", this.ToArray());
        }
        public static GeneChancesList FromString(string s)
        {
            var arr = s.Split(';').Select(f => float.Parse(f));
            var list = new GeneChancesList();
            list.AddRange(arr);
            return list;
        }
    }
    class GeneticDriftSet
    {
        public static float DefaultBase = 0.18f;
        public static float DefaultMultiplier = 0.2f;

        public const String GeneSetTypeAny = "any";
        public const String GeneSetTypeAnyNonArchite = "anyna";
        public const String GeneSetTypeInbuilt = "inbuilt";
        public const String GeneSetTypeCustom = "custom";
        public const String GeneSetTypeVanilla = "vanilla";
        public const String GeneSetTypeVanillaNonArchite = "vanillana";

        private String prefix = "";
        private String suffix = "";
        // Why prefix and suffix? Backwards compatibility with earlier configs
        private String geneSetType = GeneSetTypeAnyNonArchite;
        private String geneSetInbuilt;
        private String geneSetCustom;
        public GeneChancesList geneChances;

        public GeneticDriftSet(String prefix, String suffix)
        {
            this.prefix = prefix;
            this.suffix = suffix;
            this.geneChances = new GeneChancesList()
                { DefaultBase, DefaultMultiplier, 0 };
        }
        public void scribe()
        {
            Scribe_Values.Look(ref geneSetInbuilt,
                               prefix+"GeneSetInbuilt"+suffix); // Why not use
            // Scribe_Defs directly? Unsure, couldn't find xenotype on load
            Scribe_Values.Look(ref geneSetCustom,
                               prefix+"GeneSetCustom"+suffix);
            Scribe_Values.Look(ref geneSetType, prefix+"GeneSetType"+suffix,
                               GeneSetTypeAnyNonArchite);
            Scribe_Values.Look(ref geneChances, prefix+"geneChances"+suffix,
                new GeneChancesList() { DefaultBase, DefaultMultiplier, 0 });
        }
        public static String getGeneSetLabel(String geneSetType)
        {
            switch(geneSetType)
            {
                case GeneSetTypeAny:
                    return "AnyLower".Translate().CapitalizeFirst();
                case GeneSetTypeAnyNonArchite:
                    return "AnyNonArchite".Translate().CapitalizeFirst();
                case GeneSetTypeVanilla:
                    return "Vanilla";
                case GeneSetTypeVanillaNonArchite:
                    return "Vanilla (non-archite)";
                default:
                    return "Unknown";
            }
        }
        public static String getGeneSetLabel(XenotypeDef geneSetInbuilt)
        {
            return geneSetInbuilt.LabelCap;
        }
        public static String getGeneSetLabel(CustomXenotype geneSetCustom)
        {
            return geneSetCustom.name.CapitalizeFirst();
        }
        public String getGeneSetLabel()
        {
            switch(geneSetType)
            {
                case GeneSetTypeInbuilt:
                    return geneSetInbuilt;
                case GeneSetTypeCustom:
                    return geneSetCustom;
                default:
                    return getGeneSetLabel(this.geneSetType);
            }
        }
        public IEnumerable<GeneDef> getList()
        {
            switch(geneSetType)
            {
                default:
                case GeneSetTypeAny:
                    return DefDatabase<GeneDef>.AllDefs;
                case GeneSetTypeAnyNonArchite:
                    return DefDatabase<GeneDef>.AllDefs.Where(g => g.biostatArc == 0);
                case GeneSetTypeVanilla:
                    return DefDatabase<GeneDef>.AllDefs.Where(g => g.modContentPack.IsOfficialMod);
                case GeneSetTypeVanillaNonArchite:
                    return DefDatabase<GeneDef>.AllDefs.Where(g => g.biostatArc == 0 && g.modContentPack.IsOfficialMod);
                case GeneSetTypeInbuilt:
                    return DefDatabase<XenotypeDef>.GetNamed(geneSetInbuilt).AllGenes;
                case GeneSetTypeCustom:
                    var path = GenFilePaths.AbsFilePathForXenotype(geneSetCustom);
                    GameDataSaveLoader.TryLoadXenotype(path, out var xenotype);
                    return xenotype.genes;
            }
        }
        private void clear()
        {
            geneSetType = "";
            geneSetInbuilt = null;
            geneSetCustom = null;
        }
        public void setInbuilt(XenotypeDef item)
        {
            clear();
            geneSetType = GeneSetTypeInbuilt;
            geneSetInbuilt = item.defName;
        }
        public void setCustom(CustomXenotype item)
        {
            clear();
            geneSetType = GeneSetTypeCustom;
            geneSetCustom = item.name;
        }
        public void setAnyNA()
        {
            clear();
            geneSetType = GeneSetTypeAnyNonArchite;
        }
        public void setAny()
        {
            clear();
            geneSetType = GeneSetTypeAny;
        }
        public void setVanNA()
        {
            clear();
            geneSetType = GeneSetTypeVanillaNonArchite;
        }
        public void setVan()
        {
            clear();
            geneSetType = GeneSetTypeVanilla;
        }
    }
    class GeneticDriftEditor : Dialog_CreateXenotype
    {
        public GeneticDriftEditor(int index, Action callback)
        : base(index, callback)
        {
            ignoreRestrictions = true;
            this.callback = callback; // seems to be unreliably called, so doing
            // again in this class. Possibly because pawn generation methods,
            // called before the callbacks in the parent class, fail in this use
            // case.
        }
        private Action callback;
        protected override void DoBottomButtons(Rect rect)
        { // from grandparent
            if (Widgets.ButtonText(new Rect(rect.xMax - ButSize.x, rect.y, ButSize.x, ButSize.y), AcceptButtonLabel) && CanAccept())
            {
                Accept();
                callback.Invoke();
                Close();
            }
            if (Widgets.ButtonText(new Rect(rect.x, rect.y, ButSize.x, ButSize.y), "Close".Translate()))
            {
                Close();
                callback.Invoke();
            }
        }
        protected override void PostXenotypeOnGUI(float curX, float curY)
        { /* removes the complexity and metabolism counts */ }
        protected override void DrawSearchRect(Rect rect)
        {
            base.DrawSearchRect(rect);
            ignoreRestrictions = true;
        }

    }
    class GeneticDriftSettings : ModSettings
    {
        private static List<CustomXenotype> CustomXenotypes
        { // from CharacterCardUtility
            get
            {
                if (cachedCustomXenotypes == null)
                {
                    cachedCustomXenotypes = new List<CustomXenotype>();
                    foreach (FileInfo item in GenFilePaths.AllCustomXenotypeFiles.OrderBy((FileInfo f) => f.LastWriteTime))
                    {
                        string filePath = GenFilePaths.AbsFilePathForXenotype(Path.GetFileNameWithoutExtension(item.Name));
                        PreLoadUtility.CheckVersionAndLoad(filePath, ScribeMetaHeaderUtility.ScribeHeaderMode.Xenotype, delegate
                        {
                            if (GameDataSaveLoader.TryLoadXenotype(filePath, out var xenotype))
                            {
                                cachedCustomXenotypes.Add(xenotype);
                            }
                        }, skipOnMismatch: true);
                    }
                }
                return cachedCustomXenotypes;
            }
        }
        public static List<CustomXenotype> cachedCustomXenotypes;

        public static bool DefaultInstability = true;
        public static bool DefaultBaselinersOnly = false;

        public bool Instability = DefaultInstability;
        public bool BaselinersOnly = DefaultBaselinersOnly;
        public GeneticDriftSet genesBaby = new("", "Baby");
        public GeneticDriftSet endogenesAdult = new("endo", "Adult");
        public GeneticDriftSet xenogenesAdult = new("xeno", "Adult");
        

        public override void ExposeData()
        {
            Scribe_Values.Look(ref Instability, "instability", DefaultInstability);
            Scribe_Values.Look(ref BaselinersOnly, "baselinersOnly", DefaultBaselinersOnly);
            genesBaby.scribe();
            endogenesAdult.scribe();
            xenogenesAdult.scribe();

            base.ExposeData();
        }

        public void DoWindowContents(Rect inRect)
        {
            void GeneTypeSelector(ref Listing_Standard ls,
                                  ref GeneticDriftSet geneSet, String label,
                                  Action<XenotypeDef> inbuiltSetter,
                                  Action<CustomXenotype> customSetter,
                                  bool showArchite)
            // Setters work around reference lifetimes because C# has no way to
            // indicate geneSet will be around longer than delegates made here
            { // adapted from CharacterCardUtility.LifestageAndXenotypeOptions
                if(!ls.ButtonText(label + geneSet.getGeneSetLabel(), "This does not mean the pawn will be turned into a member of this xenotype. The mod is simply using Rimworld's xenotype system as a way of letting you specify a list of genes."))
                {
                    return;
                }
                List<FloatMenuOption> list = new List<FloatMenuOption>
                {
                    new FloatMenuOption(GeneticDriftSet.getGeneSetLabel(GeneticDriftSet.GeneSetTypeAnyNonArchite), geneSet.setAnyNA),
                    new FloatMenuOption(GeneticDriftSet.getGeneSetLabel(GeneticDriftSet.GeneSetTypeVanillaNonArchite), geneSet.setVanNA),
                    new FloatMenuOption("XenotypeEditor".Translate() + "...", delegate
                    {
                        Find.WindowStack.Add(new GeneticDriftEditor(0, delegate
                        {
                            cachedCustomXenotypes = null;
                        }));
                    })
                };
                if(showArchite)
                {
                    list.Insert(0, new FloatMenuOption(GeneticDriftSet.getGeneSetLabel(GeneticDriftSet.GeneSetTypeAny), geneSet.setAny));
                    list.Insert(2, new FloatMenuOption(GeneticDriftSet.getGeneSetLabel(GeneticDriftSet.GeneSetTypeVanilla), geneSet.setVan));
                }

                foreach (XenotypeDef item in DefDatabase<XenotypeDef>.AllDefs.Where(x => x != XenotypeDefOf.Baseliner).OrderBy((XenotypeDef x) => 0f - x.displayPriority))
                {
                    XenotypeDef xenotype = item;
                    list.Add(new FloatMenuOption(GeneticDriftSet.getGeneSetLabel(xenotype), delegate
                    {
                        inbuiltSetter(item);
                    }, xenotype.Icon, XenotypeDef.IconColor, MenuOptionPriority.Default, delegate(Rect r)
                    {
                        TooltipHandler.TipRegion(r, xenotype.descriptionShort ?? xenotype.description);
                    }, null, 24f, (Rect r) => false, null, playSelectionSound: true, 0, HorizontalJustification.Left, extraPartRightJustified: true));
                }

                foreach (CustomXenotype customXenotype in CustomXenotypes)
                {
                    CustomXenotype customInner = customXenotype;
                    list.Add(new FloatMenuOption(GeneticDriftSet.getGeneSetLabel(customInner) + " (" + "Custom".Translate() + ")", delegate
                    {
                        customSetter(customXenotype);
                    }, customInner.IconDef.Icon, XenotypeDef.IconColor, MenuOptionPriority.Default, null, null, 24f, null, null, playSelectionSound: true, 0, HorizontalJustification.Left, extraPartRightJustified: true));
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }

            void MultiGenes(ref Listing_Standard ls, ref GeneChancesList list, string label)
            {
                var count = list.Count;
                ls.Label(label + count);
                ls.IntAdjuster(ref count, 1, 1);

                for(; list.Count < count; list.Add(0.0f)){}
                for(; list.Count > count; list.RemoveAt(list.Count-1)){}

                for(int i = 0; i < count; i++)
                {
                    list[i] = ls.SliderLabeled("Chance for gene " + (i+1) + ": "
                    + list[i].ToStringPercent(), list[i], 0.0f,
                    1.0f);
                }
                ls.GapLine();
            }

            Rect rect = new(inRect.x, inRect.y, inRect.width, inRect.height - 20);
            Rect rect2 = new(0f, 0f, inRect.width - 30f, inRect.height*2);
            Widgets.BeginScrollView(rect, ref scrollPosition, rect2, true);
            var ls = new Listing_Standard();

            ls.Begin(rect2);

            ls.CheckboxLabeled("Scale endogenes with cell instability? (CancerRate)", ref Instability, "If the genes inherited from parents or present in the base xenotype include a gene for cell instability, multiply the set chances by the level of instability, gauged by the increase in cancer prevalence.");
            ls.CheckboxLabeled("Generate genes for adults only if they are of the Baseliner xenotype?", ref BaselinersOnly);
            bool showArchite = false;
            GeneTypeSelector(ref ls, ref genesBaby, "Pool of genes for newborns: ", genesBaby.setInbuilt, genesBaby.setCustom, showArchite);
            GeneTypeSelector(ref ls, ref endogenesAdult, "Pool of endogenes for adults: ", endogenesAdult.setInbuilt, endogenesAdult.setCustom, showArchite);
            showArchite = true;
            GeneTypeSelector(ref ls, ref xenogenesAdult, "Pool of xenogenes for adults: ", xenogenesAdult.setInbuilt, xenogenesAdult.setCustom, showArchite);

            ls.GapLine();
            ls.Label("Each gene addition is attempted sequentially, continuing until the first one fails to be added.");
            ls.GapLine();

            MultiGenes(ref ls, ref genesBaby.geneChances, "Max addl. genes for newborns: ");
            MultiGenes(ref ls, ref endogenesAdult.geneChances, "Max addl. endogenes for generated adults: ");
            MultiGenes(ref ls, ref xenogenesAdult.geneChances, "Max addl. xenogenes for generated adults: ");

            ls.End();
            Widgets.EndScrollView();
        }
        private static Vector2 scrollPosition = Vector2.zero;
    }
}
