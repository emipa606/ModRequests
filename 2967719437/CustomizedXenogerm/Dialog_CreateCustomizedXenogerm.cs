using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace CustomizedXenogerm
{
    class Dialog_CreateCustomizedXenogerm : Dialog_CreateXenogerm
    {
        protected override string AcceptButtonLabel => ("StartCombining".Translate()) + " for:\n" + (customPawn?.LabelShort ?? "");

        public Pawn customPawn;

        public float endoHeight;

        private Building_GeneAssembler geneAssembler;
        public Dialog_CreateCustomizedXenogerm(Building_GeneAssembler geneAssembler) : base(geneAssembler)
        {
            this.geneAssembler = geneAssembler;
        }

        protected override void DrawSearchRect(Rect rect)
        {
            base.DrawSearchRect(rect);
            DrawCustomPawnButton(rect);
        }

        protected override void DoBottomButtons(Rect rect)
        {
            base.DoBottomButtons(rect);
            if (this.customPawn != null)
            {
                int numTicks = Mathf.RoundToInt((float)Mathf.RoundToInt(GeneTuning.ComplexityToCreationHoursCurve.Evaluate(gcx) * 2500f) / geneAssembler.GetStatValue(StatDefOf.AssemblySpeedFactor));
                Rect rect2 = new Rect(rect.x + 150f + 10f, rect.y, rect.width / 2f - 150f - 10f - 10f, rect.height);
                TaggedString label ="Metabolism For " + customPawn.LabelShort + " After Implanting: " + getMetabolismAfterImplanting();
                TaggedString str = "This is the metabolism that " + customPawn.LabelShort + " would finally have after implanting this xenogerm";
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(rect2, label);
                Text.Anchor = TextAnchor.UpperLeft;
                if (Mouse.IsOver(rect2))
                {
                    Widgets.DrawHighlight(rect2);
                    TooltipHandler.TipRegion(rect2, str);
                }
            }
        }

        protected void DrawCustomPawnButton(Rect rect)
        {
            if (Widgets.ButtonText(new Rect(165f, rect.y, GeneCreationDialogBase.ButSize.x, GeneCreationDialogBase.ButSize.y), "Select Pawn"))
            {
                List<FloatMenuOption> menuSelectPawn = new List<FloatMenuOption>();
                foreach (Pawn p in Find.CurrentMap.mapPawns.FreeColonistsAndPrisoners)
                {
                    menuSelectPawn.Add(new FloatMenuOption(p.LabelShort, delegate { this.customPawn = p; }));
                }
                Find.WindowStack.Add(new FloatMenu(menuSelectPawn));
            }
        }

        public void PropagateCustomPawn()
        {
            geneAssembler.TryGetComp<CustomizedXenogermComp>().customPawn = customPawn;
        }

        public int getMetabolismAfterImplanting()
        {
            if (customPawn == null)
            {
                return met;
            }
            return GeneUtility.MetabolismAfterImplanting(customPawn, getGeneSetForSelectedGenepacks());
        }

        public GeneSet getGeneSetForSelectedGenepacks()
        {
            GeneSet geneSet = new GeneSet();
            List<Genepack> genepacks = getSelectedGenepacks();
            for (int i = 0; i < genepacks.Count; i++)
            {
                if (genepacks[i].GeneSet != null)
                {
                    List<GeneDef> genesListForReading = genepacks[i].GeneSet.GenesListForReading;
                    for (int j = 0; j < genesListForReading.Count; j++)
                    {
                        geneSet.AddGene(genesListForReading[j]);
                    }
                }
            }
            return geneSet;
        }
        

        public List<Genepack> getSelectedGenepacks()
        {
            return (List<Genepack>)this.GetType().BaseType.GetField("selectedGenepacks", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this);
        }

        public List<Genepack> getLibraryGenepacks()
        {
            return (List<Genepack>)this.GetType().BaseType.GetField("libraryGenepacks", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this);
        }

        public List<Genepack> getEndoGenepacks()
        {
            List<Genepack> pawnGenes = new List<Genepack>();
            foreach (Gene g in customPawn?.genes?.Endogenes ?? new List<Gene>())
            {
                Genepack gp = new Genepack();
                List<GeneDef> lgd = new List<GeneDef>();
                lgd.Add(g.def);
                gp.Initialize(lgd);
                pawnGenes.Add(gp);
            }
            return pawnGenes;
        }

        protected override void DrawGenes(Rect rect)
        {
            GUI.BeginGroup(rect);
            Rect rect2 = new Rect(0f, 0f, rect.width - 16f, scrollHeight);
            float curY = 0f;
            Widgets.BeginScrollView(rect.AtZero(), ref scrollPosition, rect2);
            Rect containingRect = rect2;
            containingRect.y = scrollPosition.y;
            containingRect.height = rect.height;

            if (customPawn != null)
            {
                Vector2 vector = CharacterCardUtility.PawnCardSize(customPawn);
                CharacterCardUtility.DrawCharacterCard(new Rect(0f, 0f, vector.x, vector.y), customPawn);
                curY += vector.y;
                DrawEndoSection(rect, getEndoGenepacks(), "Germline genes", ref curY, ref endoHeight, adding: false, containingRect);
                curY += 8f;
            }

            //BiostatsTable.Draw(rect, 0, endoMetabolic, 0, drawMax: true, ignoreRestrictions, maxGCX);
            //curY += 8f;

            //DrawSection(rect, selectedGenepacks, "SelectedGenepacks".Translate(), ref curY, ref selectedHeight, adding: false, containingRect);
            MethodInfo DrawSection = this.GetType().BaseType.GetMethod("DrawSection", BindingFlags.NonPublic | BindingFlags.Instance);
            object[] params1 = new object[] { rect, getSelectedGenepacks(), (string)"SelectedGenepacks".Translate(), curY, selectedHeight, false, containingRect };
            DrawSection.Invoke(this, params1);
            curY = (float)params1[3];
            selectedHeight = (float)params1[4];

            curY += 8f;

            //DrawSection(rect, libraryGenepacks, "GenepackLibrary".Translate(), ref curY, ref unselectedHeight, adding: true, containingRect);
            object[] params2 = new object[] { rect, getLibraryGenepacks(), (string)"GenepackLibrary".Translate(), curY, unselectedHeight, true, containingRect };
            DrawSection.Invoke(this, params2);
            curY = (float)params2[3];
            unselectedHeight = (float)params2[4];

            if (Event.current.type == EventType.Layout)
            {
                scrollHeight = curY;
            }
            Widgets.EndScrollView();
            GUI.EndGroup();
        }

        private void DrawEndoSection(Rect rect, List<Genepack> genepacks, string label, ref float curY, ref float sectionHeight, bool adding, Rect containingRect)
        {
            float curX = 4f;
            Rect rect2 = new Rect(10f, curY, rect.width - 16f - 10f, Text.LineHeight);
            Widgets.Label(rect2, label);
            //if (!adding)
            //{
            //    Text.Anchor = TextAnchor.UpperRight;
            //    GUI.color = ColoredText.SubtleGrayColor;
            //    Widgets.Label(rect2, "ClickToAddOrRemove".Translate());
            //    GUI.color = Color.white;
            //    Text.Anchor = TextAnchor.UpperLeft;
            //}
            curY += Text.LineHeight + 3f;
            float num = curY;
            Rect rect3 = new Rect(0f, curY, rect.width, sectionHeight);
            Widgets.DrawRectFast(rect3, Widgets.MenuSectionBGFillColor);
            curY += 4f;
            if (!genepacks.Any())
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                GUI.color = ColoredText.SubtleGrayColor;
                Widgets.Label(rect3, "(" + "NoneLower".Translate() + ")");
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;
            }
            else
            {
                for (int i = 0; i < genepacks.Count; i++)
                {
                    Genepack genepack = genepacks[i];
                    //if (quickSearchWidget.filter.Active && (!matchingGenepacks.Contains(genepack) || (adding && selectedGenepacks.Contains(genepack))))
                    //{
                    //    continue;
                    //}
                    float num2 = 34f + GeneCreationDialogBase.GeneSize.x * (float)genepack.GeneSet.GenesListForReading.Count + 4f * (float)(genepack.GeneSet.GenesListForReading.Count + 2);
                    if (curX + num2 > rect.width - 16f)
                    {
                        curX = 4f;
                        curY += GeneCreationDialogBase.GeneSize.y + 8f + 14f;
                    }
                    //if (adding && selectedGenepacks.Contains(genepack))
                    //{
                    //    Widgets.DrawLightHighlight(new Rect(curX, curY, num2, GeneCreationDialogBase.GeneSize.y + 8f));
                    //    curX += num2 + 14f;
                    //}
                    //else if (DrawGenepack(genepack, ref curX, curY, num2, containingRect))

                    //MethodInfo DrawGenepack = this.GetType().BaseType.GetMethod("DrawGenepack", BindingFlags.NonPublic | BindingFlags.Instance);
                    //object[] params1 = new object[] { genepack, curX, curY, num2, containingRect } ;
                    //bool result = (bool)DrawGenepack.Invoke(this, params1);
                    //curX = (float)params1[1];

                    if (DrawEndoGenepack(genepack, ref curX, curY, num2, containingRect))
                    {
                        if (adding)
                        {
                            SoundDefOf.Tick_High.PlayOneShotOnCamera();
                            //selectedGenepacks.Add(genepack);
                        }
                        else
                        {
                            SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                            //selectedGenepacks.Remove(genepack);
                        }
                        if (!xenotypeNameLocked)
                        {
                            xenotypeName = GeneUtility.GenerateXenotypeNameFromGenes(SelectedGenes);
                        }
                        OnGenesChanged();
                        break;
                    }
                }
            }
            curY += GeneCreationDialogBase.GeneSize.y + 12f;
            if (Event.current.type == EventType.Layout)
            {
                sectionHeight = curY - num;
            }
        }

        private bool DrawEndoGenepack(Genepack genepack, ref float curX, float curY, float packWidth, Rect containingRect)
        {
            bool result = false;
            if (genepack.GeneSet == null || genepack.GeneSet.GenesListForReading.NullOrEmpty())
            {
                return result;
            }
            Rect rect = new Rect(curX, curY, packWidth, GeneCreationDialogBase.GeneSize.y + 8f);
            if (!containingRect.Overlaps(rect))
            {
                curX = rect.xMax + 14f;
                return false;
            }
            Widgets.DrawHighlight(rect);
            GUI.color = GeneCreationDialogBase.OutlineColorUnselected;
            Widgets.DrawBox(rect);
            GUI.color = Color.white;
            curX += 4f;
            GeneUIUtility.DrawBiostats(genepack.GeneSet.ComplexityTotal, genepack.GeneSet.MetabolismTotal, genepack.GeneSet.ArchitesTotal, ref curX, curY, 4f);
            List<GeneDef> genesListForReading = genepack.GeneSet.GenesListForReading;
            for (int i = 0; i < genesListForReading.Count; i++)
            {
                GeneDef gene = genesListForReading[i];
                bool num = quickSearchWidget.filter.Active && matchingGenes.Contains(gene) ;// && matchingGenepacks.Contains(genepack);
                //bool overridden = leftChosenGroups.Any((GeneLeftChosenGroup x) => x.overriddenGenes.Contains(gene));
                bool overridden = false;
                foreach (Genepack gp in getSelectedGenepacks())
                {
                    foreach (GeneDef gd in gp.GeneSet.GenesListForReading)
                    {
                        if (gene.ConflictsWith(gd))
                        {
                            overridden = true;
                        }
                    }
                }
                Rect rect2 = new Rect(curX, curY + 4f, GeneCreationDialogBase.GeneSize.x, GeneCreationDialogBase.GeneSize.y);
                if (num)
                {
                    Widgets.DrawStrongHighlight(rect2.ExpandedBy(6f));
                }
                string extraTooltip = null;
                //if (leftChosenGroups.Any((GeneLeftChosenGroup x) => x.leftChosen == gene))
                //{
                //    extraTooltip = < DrawGenepack > g__GroupInfo | 21_0(leftChosenGroups.FirstOrDefault((GeneLeftChosenGroup x) => x.leftChosen == gene));
                //}
                //else if (cachedOverriddenGenes.Contains(gene))
                //{
                //    extraTooltip = < DrawGenepack > g__GroupInfo | 21_0(leftChosenGroups.FirstOrDefault((GeneLeftChosenGroup x) => x.overriddenGenes.Contains(gene)));
                //}
                //else if (randomChosenGroups.ContainsKey(gene))
                //{
                //    extraTooltip = ("GeneWillBeRandomChosen".Translate() + ":\n" + (from x in randomChosenGroups[gene]
                //                                                                    select x.label).ToLineList("  - ", capitalizeItems: true)).Colorize(ColoredText.TipSectionTitleColor);
                //}
                GeneUIUtility.DrawGeneDef_NewTemp(genesListForReading[i], rect2, GeneType.Xenogene, () => extraTooltip, doBackground: false, clickable: false, overridden);
                curX += GeneCreationDialogBase.GeneSize.x + 4f;
            }
            Widgets.InfoCardButton(rect.xMax - 24f, rect.y + 2f, genepack);
            //if (unpoweredGenepacks.Contains(genepack))
            //{
            //    Widgets.DrawBoxSolid(rect, UnpoweredColor);
            //    TooltipHandler.TipRegion(rect, "GenepackUnusableGenebankUnpowered".Translate().Colorize(ColorLibrary.RedReadable));
            //}
            if (Mouse.IsOver(rect))
            {
                Widgets.DrawHighlight(rect);
            }
            if (Widgets.ButtonInvisible(rect))
            {
                result = true;
            }
            curX = Mathf.Max(curX, rect.xMax + 14f);
            return result;
        }
    }
}
