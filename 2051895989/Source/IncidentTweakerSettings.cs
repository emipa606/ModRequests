using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace IncidentTweaker
{
    public class IncidentTweak : IExposable
    {
        public float baseChanceMultiplier;

        public void ExposeData()
        {
            Scribe_Values.Look(ref baseChanceMultiplier, "baseChance");
        }
    }



    [StaticConstructorOnStartup]
    public class IncidentTweakerSetup
    {
        public static readonly Texture2D checkboxOn = ContentFinder<Texture2D>.Get("IncidentTweaker/CheckboxOn", true);
        public static readonly Texture2D checkboxOff = ContentFinder<Texture2D>.Get("IncidentTweaker/CheckboxOff", true);
        public static readonly Texture2D ShareBarTexture = SolidColorMaterials.NewSolidColorTexture(new ColorInt(46, 152, 53).ToColor);
        public static readonly Color colorNotApplicable = new Color(0.7f, 0.7f, 0.7f);

        public static IncidentCategoryDef[] excludeCategories = new IncidentCategoryDef[] { };
        public static IEnumerable<IncidentDef> defs = DefDatabase<IncidentDef>.AllDefs.Where(x => !excludeCategories.Contains(x.category)).OrderBy(x => x.category.defName).ThenBy(x => x.LabelCap.RawText);
        public static IEnumerable<IncidentCategoryDef> categoryDefs = defs.Select(x => x.category).Distinct();

        public static UiTable table = new UiTable(40, new float[] { -50, 30, 20, -130, 60 }, new string[] {
            "IncidentTweakerColCheckmark".Translate(),
            "IncidentTweakerColIncident".Translate(),
            "IncidentTweakerColMod".Translate(),
            "IncidentTweakerColChance".Translate(),
            "IncidentTweakerColMultiplier".Translate()
        });
    }

    public class IncidentTweakerSettings : ModSettings
    {
        public Dictionary<string, IncidentTweak> tweaks = new Dictionary<string, IncidentTweak>();

        override public void ExposeData()
        {
            Scribe_Collections.Look(ref tweaks, "tweaks");
        }

        Rect CenterRect(Rect rect, int w, int h)
        {
            rect.x += (rect.width - w) / 2;
            rect.y += (rect.height - h) / 2;
            rect.width = w;
            rect.height = h;
            return rect;
        }

        SimpleCurve valueCurve = new SimpleCurve
        {
            { new CurvePoint(0f, 0f), true },
            { new CurvePoint(0.5f, 1f), true },
            { new CurvePoint(0.75f, 5f), true },
            { new CurvePoint(1f, 100f), true }
        };

        float BaseChance(IncidentDef def)
        {
            try
            {
                return def.Worker.BaseChanceThisGame;
            }
            catch
            {
                return 0;
            }
        }

        public void DoSettingsWindowContents(Rect inRect)
        {
            UiTable table = IncidentTweakerSetup.table;

            IncidentCategoryDef currentCategory = null;
            Dictionary<IncidentCategoryDef, float> categoryTotals = CalculateCategoryTotals();

            table.Start(IncidentTweakerSetup.defs.Count() + IncidentTweakerSetup.categoryDefs.Count(), inRect);

            Text.Anchor = TextAnchor.MiddleLeft;
            foreach (IncidentDef def in IncidentTweakerSetup.defs)
            {
                if (def.category != currentCategory)
                {
                    Text.Font = GameFont.Medium;
                    currentCategory = def.category;
                    Rect row = table.WholeRow();
                    table.Text(row.RightPartPixels(row.width - 50), currentCategory.defName, "IncidentTweakerIncidentCategory".Translate());
                    table.NextRow();
                }
                Text.Font = GameFont.Small;

                IncidentTweak tweak = tweaks.TryGetValue(def.defName);
                float chance = BaseChance(def);
                bool applicable = def.baseChance > 0 || def.baseChanceWithRoyalty > 0;

                if (Widgets.ButtonImage(CenterRect(table.Cell(0), 32, 32), tweak == null ? IncidentTweakerSetup.checkboxOff : IncidentTweakerSetup.checkboxOn, Color.white))
                {
                    if (tweaks.ContainsKey(def.defName))
                        tweaks.Remove(def.defName);
                    else
                        GetOrCreateTweak(def).baseChanceMultiplier = 1f;
                }

                table.Text(1, def.LabelCap, def.defName);
                table.Text(2, def.modContentPack == null ? "" : def.modContentPack.Name, def.modContentPack == null ? "" : def.modContentPack.PackageIdPlayerFacing);

                Text.Anchor = TextAnchor.MiddleCenter;
                Text.Font = GameFont.Tiny;
                if (applicable)
                {
                    Rect rectChance = table.Cell(3);
                    float total = categoryTotals[def.category];
                    if (total != 0)
                    {
                        GUI.DrawTexture(rectChance.ContractedBy(4).LeftPart(chance / total), IncidentTweakerSetup.ShareBarTexture);
                    }

                    table.Text(rectChance, BaseChance(def).ToString(), "IncidentTweakerChanceExplanation".Translate());
                }
                else
                {
                    Rect rectNotApplicable = table.Cell(3);
                    Color color = GUI.color;
                    GUI.color = IncidentTweakerSetup.colorNotApplicable;
                    table.Text(rectNotApplicable, "IncidentTweakerNotApplicable".Translate(), "IncidentTweakerNotApplicableDesc".Translate());
                    GUI.color = color;
                }
                Text.Anchor = TextAnchor.MiddleLeft;
                Text.Font = GameFont.Small;

                Rect rect = table.Cell(4);
                float multiplier = tweak == null ? 1f : tweak.baseChanceMultiplier;
                if (WidgetsExtra.HorizontalSlider(rect, ref multiplier, valueCurve))
                {
                    GetOrCreateTweak(def).baseChanceMultiplier = multiplier;
                }

                table.NextRow();
            }

            table.Stop();
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private Dictionary<IncidentCategoryDef, float> CalculateCategoryTotals()
        {
            Dictionary<IncidentCategoryDef, float> categoryTotals = new Dictionary<IncidentCategoryDef, float>();

            foreach (IncidentDef def in IncidentTweakerSetup.defs)
            {
                if (!categoryTotals.ContainsKey(def.category)) categoryTotals[def.category] = BaseChance(def);
                else categoryTotals[def.category] += BaseChance(def);
            }

            return categoryTotals;
        }

        IncidentTweak GetOrCreateTweak(IncidentDef def)
        {
            IncidentTweak tweak = tweaks.TryGetValue(def.defName);

            if (tweak == null)
            {
                tweak = new IncidentTweak();
                tweaks[def.defName] = tweak;
            }

            return tweak;
        }
        public IncidentTweak GetTweak(IncidentDef def)
        {
            IncidentTweak tweak = tweaks.TryGetValue(def.defName);

            return tweak;
        }
    }

}
