using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace MoreArchotechGarbage
{
    [HotSwappable]
    public class Dialog_SelectGenes : Window
    {
        public Pawn pawn;
        public ArchotechGeneRipper ripper;
        public static List<GeneDef> selectedGenes = new List<GeneDef>();
        public Action closeAction;
        public Dialog_SelectGenes(Pawn pawn, ArchotechGeneRipper ripper, Action action)
        {
            this.pawn = pawn;
            this.ripper = ripper;
            this.closeAction = action;
            selectedGenes = new List<GeneDef>();
        }
        public override Vector2 InitialSize => new(736f, 700f);
        private Vector2 scrollPosition;
        public override void DoWindowContents(Rect inRect)
        {
            inRect.yMax -= CloseButSize.y;
            var rect = inRect;
            rect.xMin += 34f;
            Text.Font = GameFont.Medium;
            Widgets.Label(rect, "ViewGenes".Translate() + ": " + pawn.genes.XenotypeLabelCap);
            Text.Font = GameFont.Small;
            GUI.color = XenotypeDef.IconColor;
            GUI.DrawTexture(new Rect(inRect.x, inRect.y, 30f, 30f), pawn.genes.XenotypeIcon);
            GUI.color = Color.white;
            inRect.yMin += 34f;
            var zero = Vector2.zero;
            DrawGenesInfo(inRect, pawn, InitialSize.y, ref zero, ref scrollPosition);
            if (Widgets.ButtonText(
                    new Rect(inRect.xMax - CloseButSize.x, inRect.yMax, CloseButSize.x,
                        CloseButSize.y), "Cancel".Translate()))
            {
                Close();
            }

            if (Widgets.ButtonText(
                    new Rect(inRect.xMax - CloseButSize.x * 2 - 6, inRect.yMax, CloseButSize.x,
                        CloseButSize.y), "MAG_Select".Translate(), active: selectedGenes.Any()))
            {
                ripper.selectedGenes = selectedGenes.ListFullCopy();
                closeAction.Invoke();
                Close();
            }
        }

        public static void DrawGenesInfo(Rect rect, Thing target, float initialHeight, ref Vector2 size, ref Vector2 scrollPosition, GeneSet pregnancyGenes = null)
        {
            Rect rect2 = rect;
            Rect position = rect2.ContractedBy(10f);
            GUI.BeginGroup(position);
            Rect rect3 = new Rect(0f, 0f, position.width, position.height - 12f);
            DrawGeneSections(rect3, target, pregnancyGenes, ref scrollPosition);
            if (Event.current.type == EventType.Layout)
            {
                float num2 = GeneUIUtility.endogenesHeight + GeneUIUtility.xenogenesHeight + 12f + 70f;
                if (num2 > initialHeight)
                {
                    size.y = Mathf.Min(num2, (float)(UI.screenHeight - 35) - 165f - 30f);
                }
                else
                {
                    size.y = initialHeight;
                }
                GeneUIUtility.xenogenesHeight = 0f;
                GeneUIUtility.endogenesHeight = 0f;
            }
            GUI.EndGroup();
        }

        private static void DrawGeneSections(Rect rect, Thing target, GeneSet genesOverride, ref Vector2 scrollPosition)
        {
            RecacheGenes(target, genesOverride);
            GUI.BeginGroup(rect);
            Rect rect2 = new Rect(0f, 0f, rect.width - 16f, GeneUIUtility.scrollHeight);
            float curY = 0f;
            Widgets.BeginScrollView(rect.AtZero(), ref scrollPosition, rect2);
            Rect containingRect = rect2;
            containingRect.y = scrollPosition.y;
            containingRect.height = rect.height;
            if (target is Pawn)
            {
                if (GeneUIUtility.endogenes.Any())
                {
                    DrawSection(rect, xeno: false, GeneUIUtility.endogenes.Count, ref curY, ref GeneUIUtility.endogenesHeight, delegate (int i, Rect r)
                    {
                        DrawGene(GeneUIUtility.endogenes[i], r, GeneType.Endogene);
                    }, containingRect);
                    curY += 12f;
                }
                DrawSection(rect, xeno: true, GeneUIUtility.xenogenes.Count, ref curY, ref GeneUIUtility.xenogenesHeight, delegate (int i, Rect r)
                {
                    DrawGene(GeneUIUtility.xenogenes[i], r, GeneType.Xenogene);
                }, containingRect);
            }
            else
            {
                GeneType geneType = ((genesOverride == null && !(target is HumanEmbryo)) ? GeneType.Xenogene : GeneType.Endogene);
                DrawSection(rect, geneType == GeneType.Xenogene, GeneUIUtility.geneDefs.Count, ref curY, ref GeneUIUtility.xenogenesHeight, delegate (int i, Rect r)
                {
                    GeneUIUtility.DrawGeneDef_NewTemp(GeneUIUtility.geneDefs[i], r, geneType);
                }, containingRect);
            }
            if (Event.current.type == EventType.Layout)
            {
                GeneUIUtility.scrollHeight = curY;
            }
            Widgets.EndScrollView();
            GUI.EndGroup();
        }

        private static void RecacheGenes(Thing target, GeneSet genesOverride)
        {
            GeneUIUtility.geneDefs.Clear();
            GeneUIUtility.xenogenes.Clear();
            GeneUIUtility.endogenes.Clear();
            GeneUIUtility.gcx = 0;
            GeneUIUtility.met = 0;
            GeneUIUtility.arc = 0;
            Pawn pawn = target as Pawn;
            GeneSet geneSet = (target as GeneSetHolderBase)?.GeneSet ?? genesOverride;
            if (pawn != null)
            {
                foreach (Gene xenogene in pawn.genes.Xenogenes)
                {
                    if (!xenogene.Overridden)
                    {
                        AddBiostats(xenogene.def);
                    }
                    GeneUIUtility.xenogenes.Add(xenogene);
                }
                foreach (Gene endogene in pawn.genes.Endogenes)
                {
                    if (endogene.def.endogeneCategory != EndogeneCategory.Melanin || !pawn.genes.Endogenes.Any((Gene x) => x.def.skinColorOverride.HasValue))
                    {
                        if (!endogene.Overridden)
                        {
                            AddBiostats(endogene.def);
                        }
                        GeneUIUtility.endogenes.Add(endogene);
                    }
                }
                GeneUIUtility.xenogenes.SortGenes();
                GeneUIUtility.endogenes.SortGenes();
            }
            else
            {
                if (geneSet == null)
                {
                    return;
                }
                foreach (GeneDef item in geneSet.GenesListForReading)
                {
                    GeneUIUtility.geneDefs.Add(item);
                }
                GeneUIUtility.gcx = geneSet.ComplexityTotal;
                GeneUIUtility.met = geneSet.MetabolismTotal;
                GeneUIUtility.arc = geneSet.ArchitesTotal;
                GeneUIUtility.geneDefs.SortGeneDefs();
            }
            static void AddBiostats(GeneDef gene)
            {
                GeneUIUtility.gcx += gene.biostatCpx;
                GeneUIUtility.met += gene.biostatMet;
                GeneUIUtility.arc += gene.biostatArc;
            }
        }

        private static void DrawSection(Rect rect, bool xeno, int count, ref float curY, ref float sectionHeight, Action<int, Rect> drawer, Rect containingRect)
        {
            Widgets.Label(10f, ref curY, rect.width, (xeno ? "Xenogenes" : "Endogenes").Translate().CapitalizeFirst(), (xeno ? "XenogenesDesc" : "EndogenesDesc").Translate());
            float num = curY;
            Rect rect2 = new Rect(rect.x, curY, rect.width, sectionHeight);
            if (xeno && count == 0)
            {
                Text.Anchor = TextAnchor.UpperCenter;
                GUI.color = ColoredText.SubtleGrayColor;
                rect2.height = Text.LineHeight;
                Widgets.Label(rect2, "(" + "NoXenogermImplanted".Translate() + ")");
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;
                curY += 90f;
            }
            else
            {
                Widgets.DrawMenuSection(rect2);
                float num2 = (rect.width - 12f - 630f - 36f) / 2f;
                curY += num2;
                int num3 = 0;
                int num4 = 0;
                for (int i = 0; i < count; i++)
                {
                    if (num4 >= 6)
                    {
                        num4 = 0;
                        num3++;
                    }
                    else if (i > 0)
                    {
                        num4++;
                    }
                    Rect rect3 = new Rect(num2 + (float)num4 * 90f + (float)num4 * 6f, curY + (float)num3 * 90f + (float)num3 * 6f, 90f, 90f);
                    if (containingRect.Overlaps(rect3))
                    {
                        drawer(i, rect3);
                    }
                }
                curY += (float)(num3 + 1) * 90f + (float)num3 * 6f + num2;
            }
            if (Event.current.type == EventType.Layout)
            {
                sectionHeight = curY - num;
            }
        }

        public static void DrawGene(Gene gene, Rect geneRect, GeneType geneType, bool doBackground = true, bool clickable = true)
        {
            DrawGeneBasics(gene.def, geneRect, geneType, doBackground, clickable, !gene.Active);
            if (Mouse.IsOver(geneRect))
            {
                string text = gene.LabelCap.Colorize(ColoredText.TipSectionTitleColor) + "\n\n" + gene.def.DescriptionFull;
                if (gene.Overridden)
                {
                    text += "\n\n";
                    text = ((gene.overriddenByGene.def != gene.def) ? (text + ("OverriddenByGene".Translate() + ": " + gene.overriddenByGene.LabelCap).Colorize(ColorLibrary.RedReadable)) : (text + ("OverriddenByIdenticalGene".Translate() + ": " + gene.overriddenByGene.LabelCap).Colorize(ColorLibrary.RedReadable)));
                }
                if (clickable)
                {
                    text = text + "\n\n" + "ClickForMoreInfo".Translate().ToString().Colorize(ColoredText.SubtleGrayColor);
                }
                TooltipHandler.TipRegion(geneRect, text);
            }
        }

        private static void DrawGeneBasics(GeneDef gene, Rect geneRect, GeneType geneType, bool doBackground, bool clickable, bool overridden)
        {
            GUI.BeginGroup(geneRect);
            Rect rect = geneRect.AtZero();
            if (doBackground)
            {
                Widgets.DrawHighlight(rect);
                GUI.color = new Color(1f, 1f, 1f, 0.05f);
                Widgets.DrawBox(rect);
                GUI.color = Color.white;
            }
            float num = rect.width - Text.LineHeight;
            Rect rect2 = new Rect(geneRect.width / 2f - num / 2f, 0f, num, num);
            Color iconColor = gene.IconColor;
            if (overridden)
            {
                iconColor.a = 0.75f;
                GUI.color = ColoredText.SubtleGrayColor;
            }
            CachedTexture cachedTexture = GeneUIUtility.GeneBackground_Archite;
            if (gene.biostatArc == 0)
            {
                switch (geneType)
                {
                    case GeneType.Endogene:
                        cachedTexture = GeneUIUtility.GeneBackground_Endogene;
                        break;
                    case GeneType.Xenogene:
                        cachedTexture = GeneUIUtility.GeneBackground_Xenogene;
                        break;
                }
            }
            GUI.DrawTexture(rect2, cachedTexture.Texture);
            Widgets.DefIcon(rect2, gene, null, 0.9f, null, drawPlaceholder: false, iconColor);
            Text.Font = GameFont.Tiny;
            float num2 = Text.CalcHeight(gene.LabelCap, rect.width);
            Rect rect3 = new Rect(0f, rect.yMax - num2, rect.width, num2);
            GUI.DrawTexture(new Rect(rect3.x, rect3.yMax - num2, rect3.width, num2), TexUI.GrayTextBG);
            Text.Anchor = TextAnchor.LowerCenter;
            if (overridden)
            {
                GUI.color = ColoredText.SubtleGrayColor;
            }
            if (doBackground && num2 < (Text.LineHeight - 2f) * 2f)
            {
                rect3.y -= 3f;
            }
            Widgets.Label(rect3, gene.LabelCap);
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
            if (clickable)
            {
                if (Widgets.ButtonInvisible(rect))
                {
                    if (selectedGenes.Contains(gene))
                    {
                        selectedGenes.Remove(gene);
                    }
                    else if (selectedGenes.Count < MoreArchotechGarbageSettings.archotechGeneRipperMaxSelectableGenes)
                    {
                        selectedGenes.Add(gene);
                    }
                }
                if (selectedGenes.Contains(gene))
                {
                    Widgets.DrawHighlightSelected(rect);
                }
                if (Mouse.IsOver(rect))
                {
                    Widgets.DrawHighlight(rect);
                }
            }
            GUI.EndGroup();
        }

        private static void DrawStat(Rect iconRect, CachedTexture icon, string stat, float iconWidth)
        {
            GUI.DrawTexture(iconRect, icon.Texture);
            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.LabelFit(new Rect(iconRect.xMax, iconRect.y, 38f - iconWidth, iconWidth), stat);
            Text.Anchor = TextAnchor.UpperLeft;
        }
    }
}