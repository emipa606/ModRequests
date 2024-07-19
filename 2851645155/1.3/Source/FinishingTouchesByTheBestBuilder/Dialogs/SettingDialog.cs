using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld.Planet;
using System.Text;

namespace FinishingTouchesByTheBestBuilder
{
    public class SettingDialog : Window
    {
        public bool opened = false;

        protected override float Margin => 0f;

        public override void DoWindowContents(Rect inRect)
        {
            DataContext dc = DataContext.current;
            int mapId = Find.CurrentMap.uniqueID;
            int tick = Find.TickManager.TicksGame;
            if (!dc.bestPawnStatesMap.TryGetValue(mapId, out HashSet<PawnState> pawnStates))
            {
                dc.bestPawnStatesMap[mapId] = pawnStates = new HashSet<PawnState>();
            }
            PawnState pawnState = pawnStates.FirstOrDefault();
            Pawn bestPawn = pawnState?.pawn;
            if (!dc.bestPawnLastRefreshTicks.TryGetValue(mapId, out int lastRefreshTick))
            {
                dc.bestPawnLastRefreshTicks[mapId] = 0;
                lastRefreshTick = 0;
            }

            bool textWordWrapBk = Text.WordWrap;
            TextAnchor textAnchorBk = Text.Anchor;
            GameFont fontOriginBk = Text.Font;
            GameFont fontBk = GameFont.Small;
            Color colorBk = GUI.color;
            Text.Anchor = TextAnchor.MiddleLeft;

            bool valueChanged = false;
            float heightSum = 0f;
            float boxWidth = InitialSize.x - (UIHelper.MARGIN_LEFT * 2f);
            float innerWidth = boxWidth - (UIHelper.MARGIN_LEFT * 2f);

            //TitleBar position fixed
            float titleX = inRect.x, titleY = inRect.y, titleWidth = inRect.width;
            heightSum += UIHelper.HEIGHT_ROW + UIHelper.MARGIN_TOP;

            //pawnSection
            Text.Font = GameFont.Medium;
            Rect pawnSection = new Rect(inRect.x + UIHelper.MARGIN_LEFT, inRect.y + heightSum, boxWidth, UIHelper.HEIGHT_SECTION_TITLE);
            heightSum += UIHelper.WriteFixedSectionTitleBox(pawnSection, "FT_BestPawnLabel".Translate(), new Rect(pawnSection.x + UIHelper.MARGIN_LEFT, pawnSection.y + pawnSection.height, pawnSection.width - (UIHelper.MARGIN_LEFT * 2f), 0f), innerRect =>
            {
                float innerHeightSum = 0f;
                Text.Font = fontBk;
                float leftMarginInRow = 0f;
                if (bestPawn != null)
                {
                    Rect pawnImage = new Rect(innerRect.x + leftMarginInRow, innerRect.y + innerHeightSum, 64f, 64f);
                    RenderTexture texPawn = PortraitsCache.Get(bestPawn, new Vector2(64f, 64f), Rot4.South);
                    Widgets.DrawTextureFitted(pawnImage, texPawn, 1f);
                    leftMarginInRow += 64f + UIHelper.MARGIN_LEFT;

                    string pawnName = bestPawn.Name.ToStringFull;
                    string pawnTips = $"{SkillDefOf.Construction.LabelCap}{pawnState.skill} {"Priority".Translate()}{bestPawn.workSettings.GetPriority(WorkTypeDefOf.Construction)}";
                    float pawnNameWidth = new float[] { Text.CalcSize(pawnName).x, Text.CalcSize(pawnTips).x }.Max();
                    Rect pawnNameLabel = new Rect(innerRect.x + leftMarginInRow, innerRect.y + innerHeightSum, pawnNameWidth, 32f);
                    Widgets.Label(pawnNameLabel, pawnName);
                    Rect pawnTipsLabel = pawnNameLabel;
                    pawnTipsLabel.y += 32f;
                    Widgets.Label(pawnTipsLabel, pawnTips);
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine($"{SkillDefOf.Construction.LabelCap}{"Skills".Translate()}: {CommonHelper.GetSkillWhenSkillEnabled(bestPawn)}");
                    sb.AppendLine($"{"FT_AddLevelByInspireLabel".Translate()}: {(CommonHelper.Inspired(bestPawn)? $"+{dc.addLevelByInspire}" : "-")}");
                    if (ModsConfig.IdeologyActive)
                    {
                        sb.AppendLine($"{"FT_AddLevelByRoleEffectLabel".Translate()}: {(CommonHelper.HasRoleEffect(bestPawn) ? $"+{dc.addLevelByRoleEffect}" : "-")}");
                    }
                    TooltipHandler.TipRegion(pawnTipsLabel, sb.ToString());

                    leftMarginInRow += pawnNameWidth + UIHelper.MARGIN_LEFT;
                }
                else
                {
                    Text.Font = GameFont.Medium;
                    string notFoundMessage = "FT_BestPawnNotFound".Translate();
                    float labelWidth = Text.CalcSize(notFoundMessage).x;
                    Rect notFoundLabel = new Rect(innerRect.x + leftMarginInRow, innerRect.y + innerHeightSum, labelWidth, 64f);
                    Widgets.Label(notFoundLabel, notFoundMessage);
                    Text.Font = fontBk;
                    leftMarginInRow += labelWidth + UIHelper.MARGIN_LEFT;
                }
                if (dc.allowMultiBuilderMode && pawnStates.Count >= 2)
                {
                    Rect plusRect = new Rect(innerRect.x + leftMarginInRow, innerRect.y + innerHeightSum, 32f, 64f);
                    Widgets.DrawTextureFitted(plusRect, TexButton.Add, 1f);
                    leftMarginInRow += 34f;

                    Text.Font = GameFont.Medium;
                    string numberOfOtherMemberText = (pawnStates.Count - 1).ToString();
                    float numberRectWidth = Text.CalcSize(numberOfOtherMemberText).x;
                    Rect numberRect = new Rect(innerRect.x + leftMarginInRow, innerRect.y + innerHeightSum, numberRectWidth, 64f);
                    Widgets.Label(numberRect, numberOfOtherMemberText);
                    leftMarginInRow += numberRectWidth + 2f;
                    Text.Font = fontBk;

                    Rect memberRect = new Rect(innerRect.x + leftMarginInRow, innerRect.y + innerHeightSum, 32f, 64f);
                    Widgets.DrawTextureFitted(memberRect, TexCommand.GatherSpotActive, 1f);
                    leftMarginInRow += 32f + UIHelper.MARGIN_LEFT;

                    StringBuilder sb2 = new StringBuilder();
                    for (int i = 1; i < pawnStates.Count; i++)
                    {
                        if (i > 1)
                        {
                            sb2.AppendLine("----------");
                        }
                        if (i == 10)
                        {
                            sb2.AppendLine("FT_AndNumOthers".Translate(pawnStates.Count - 10));
                            break;
                        }
                        PawnState state = pawnStates.ElementAt(i);
                        sb2.AppendLine($"{state.pawn.Name.ToStringFull}({SkillDefOf.Construction.LabelCap}{state.skill},{"Priority".Translate()}{state.pawn.workSettings.GetPriority(WorkTypeDefOf.Construction)})");
                    }
                    TooltipHandler.TipRegion(memberRect, sb2.ToString());
                }
                Rect refreshRect = new Rect(innerRect.x + leftMarginInRow, innerRect.y + innerHeightSum, 32f, 64f);
                if (tick <= lastRefreshTick)
                {
                    Widgets.DrawTextureFitted(refreshRect, Images.TEX_ICON_RELOAD_OFF, 1f);
                }
                else
                {
                    if (Widgets.ButtonImageFitted(refreshRect, Images.TEX_ICON_RELOAD_ON))
                    {
                        dc.bestPawnLastRefreshTicks[mapId] = tick;
                        dc.ValidateAndRefreshPawnStates();
                        dc.bestPawnStatesMap[mapId] = pawnStates = pawnStates.OrderByDescending(x => x.skill).ThenBy(x => CommonHelper.GetSettingPriority(x.pawn)).ToHashSet();
                    }
                }
                innerHeightSum += 64f + UIHelper.MARGIN_TOP;
                return innerHeightSum;
            }, fontBk);

            //choice option section
            heightSum += UIHelper.MARGIN_TOP;
            Rect choiseOptionSection = new Rect(inRect.x + UIHelper.MARGIN_LEFT, inRect.y + heightSum, boxWidth, UIHelper.HEIGHT_SECTION_TITLE);
            heightSum += UIHelper.WriteFixedSectionTitleBox(choiseOptionSection, "FT_ChoiceBestPawnOptionLabel".Translate(), new Rect(choiseOptionSection.x + UIHelper.MARGIN_LEFT, choiseOptionSection.y + choiseOptionSection.height, choiseOptionSection.width - (UIHelper.MARGIN_LEFT * 2f), 0f), innerRect =>
            {
                float innerHeightSum = 0f;

                int choiceBestPawnOptionPriorityBk = dc.choiceBestPawnOptionPriority;
                bool allowMultiBuilderModeBk = dc.allowMultiBuilderMode;
                ThresholdType thresholdTypeBk = dc.thresholdType;
                int staticSkillThresholdGEBk = dc.staticSkillThresholdGE;
                int dynamicSkillNegativeCorrectionBk = dc.dynamicSkillNegativeCorrection;

                Text.Font = GameFont.Medium;
                Rect choiseBestPawnOptionPriorityLabel = new Rect(innerRect.x, innerRect.y + innerHeightSum, innerWidth, UIHelper.HEIGHT_ROW);
                Widgets.Label(choiseBestPawnOptionPriorityLabel, "FT_ChoiceBestPawnOptionPriorityLabel".Translate());
                Text.Font = fontBk;
                innerHeightSum += UIHelper.HEIGHT_ROW + UIHelper.MARGIN_TOP;

                Rect choiseBestPawnOptionPriorityInput = new Rect(innerRect.x, innerRect.y + innerHeightSum, innerWidth, UIHelper.HEIGHT_ROW);
                dc.choiceBestPawnOptionPriority = (int)Widgets.HorizontalSlider(choiseBestPawnOptionPriorityInput, dc.choiceBestPawnOptionPriority, 1, 9, true, "FT_ChoiceBestPawnOptionPriorityValue".Translate(dc.choiceBestPawnOptionPriority), "1", "9");
                innerHeightSum += UIHelper.HEIGHT_ROW;

                Rect choiseBestPawnOptionPriorityTips = new Rect(innerRect.x, innerRect.y + innerHeightSum, innerWidth, UIHelper.HEIGHT_ROW);
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(choiseBestPawnOptionPriorityTips, "FT_ChoiceBestPawnOptionPriorityTips".Translate(dc.choiceBestPawnOptionPriority));
                Text.Anchor = textAnchorBk;
                innerHeightSum += UIHelper.HEIGHT_ROW + UIHelper.MARGIN_TOP;

                //@@@
                Rect horizontalLine = new Rect(new Rect(innerRect.x, innerRect.y + innerHeightSum, innerWidth, 0.5f));
                Widgets.DrawBoxSolid(horizontalLine, Color.grey);

                Text.Font = GameFont.Medium;
                Rect allowMultiBuilderModeCheck = new Rect(innerRect.x, innerRect.y + innerHeightSum, innerWidth, UIHelper.HEIGHT_ROW);
                UIHelper.LabeledCheckBoxLeft(allowMultiBuilderModeCheck, "FT_AllowMultiBuilder".Translate(), ref dc.allowMultiBuilderMode, ref valueChanged);
                Text.Font = fontBk;
                innerHeightSum += UIHelper.HEIGHT_ROW + UIHelper.MARGIN_TOP;

                if (dc.allowMultiBuilderMode)
                {
                    Rect selectThresholdTypeButton = new Rect(innerRect.x, innerRect.y + innerHeightSum, 120f, UIHelper.HEIGHT_ROW);
                    if (Widgets.ButtonText(selectThresholdTypeButton, dc.thresholdType.Label()))
                    {
                        FloatMenuUtility.MakeMenu((ThresholdType[])Enum.GetValues(typeof(ThresholdType)), x => x.Label(), x => () => { 
                            dc.thresholdType = x;
                            dc.bestPawnLastRefreshTicks[mapId] = tick;
                            dc.ValidateAndRefreshPawnStates();
                            dc.bestPawnStatesMap[mapId] = pawnStates = pawnStates.OrderByDescending(y => y.skill).ThenBy(y => CommonHelper.GetSettingPriority(y.pawn)).ToHashSet();
                        });
                    }
                    Rect multiBuilderSliderAndDesc = new Rect(innerRect.x + 120f + UIHelper.MARGIN_LEFT, innerRect.y + innerHeightSum, innerWidth - (120f + (UIHelper.MARGIN_LEFT * 2f)), UIHelper.HEIGHT_ROW);
                    switch (dc.thresholdType)
                    {
                        case ThresholdType.Static:
                            dc.staticSkillThresholdGE = (int)Widgets.HorizontalSlider(multiBuilderSliderAndDesc, dc.staticSkillThresholdGE, 0, dc.staticSkillThresholdMax, true, "FT_StaticSkillThresholdGEValue".Translate(dc.staticSkillThresholdGE), "0", dc.staticSkillThresholdMax.ToString());
                            innerHeightSum += UIHelper.HEIGHT_ROW + UIHelper.MARGIN_TOP;
                            multiBuilderSliderAndDesc.y += UIHelper.HEIGHT_ROW;
                            Text.Anchor = TextAnchor.MiddleCenter;
                            Widgets.Label(multiBuilderSliderAndDesc, "FT_StaticSkillThresholdGETips".Translate(dc.staticSkillThresholdGE));
                            Text.Anchor = textAnchorBk;
                            break;
                        case ThresholdType.Dynamic:
                            dc.dynamicSkillNegativeCorrection = (int)Widgets.HorizontalSlider(multiBuilderSliderAndDesc, dc.dynamicSkillNegativeCorrection, 0, dc.dynamicSkillNegativeCorrectionMax, true, "FT_DynamicSkillNegativeCorrectionValue".Translate(dc.dynamicSkillNegativeCorrection), "0", dc.dynamicSkillNegativeCorrectionMax.ToString());
                            innerHeightSum += UIHelper.HEIGHT_ROW + UIHelper.MARGIN_TOP;
                            multiBuilderSliderAndDesc.y += UIHelper.HEIGHT_ROW;
                            Text.Anchor = TextAnchor.MiddleCenter;
                            Widgets.Label(multiBuilderSliderAndDesc, "FT_DynamicSkillNegativeCorrectionTips".Translate(dc.dynamicSkillNegativeCorrection));
                            Text.Anchor = textAnchorBk;
                            break;
                    }
                    innerHeightSum += UIHelper.HEIGHT_ROW + UIHelper.MARGIN_TOP;
                }
                //@@@

                if (choiceBestPawnOptionPriorityBk != dc.choiceBestPawnOptionPriority ||
                allowMultiBuilderModeBk != dc.allowMultiBuilderMode ||
                thresholdTypeBk != dc.thresholdType || 
                staticSkillThresholdGEBk != dc.staticSkillThresholdGE ||
                dynamicSkillNegativeCorrectionBk != dc.dynamicSkillNegativeCorrection)
                {
                    dc.bestPawnLastRefreshTicks[mapId] = tick;
                    dc.ValidateAndRefreshPawnStates();
                    dc.bestPawnStatesMap[mapId] = pawnStates = pawnStates.OrderByDescending(x => x.skill).ThenBy(x => CommonHelper.GetSettingPriority(x.pawn)).ToHashSet();
                }

                return innerHeightSum;
            }, fontBk);

            //add level section
            heightSum += UIHelper.MARGIN_TOP;
            Rect addLevelSection = new Rect(inRect.x + UIHelper.MARGIN_LEFT, inRect.y + heightSum, boxWidth, UIHelper.HEIGHT_SECTION_TITLE);
            heightSum += UIHelper.WriteFixedSectionTitleBox(addLevelSection, "FT_AddLevelLabel".Translate(), new Rect(addLevelSection.x + UIHelper.MARGIN_LEFT, addLevelSection.y + addLevelSection.height, addLevelSection.width - (UIHelper.MARGIN_LEFT * 2f), 0f), innerRect =>
            {
                float innerHeightSum = 0f;

                bool notBuildInspireCheckBk = dc.notBuildInspireCheck;
                int addLevelByInspireBk = dc.addLevelByInspire;
                int addLevelByRoleEffectBk = dc.addLevelByRoleEffect;

                Text.Font = GameFont.Medium;
                Rect addLevelByInspireLabel = new Rect(innerRect.x, innerRect.y + innerHeightSum, innerWidth, UIHelper.HEIGHT_ROW);
                Widgets.Label(addLevelByInspireLabel, "FT_AddLevelByInspireLabel".Translate());
                Text.Font = fontBk;
                innerHeightSum += UIHelper.HEIGHT_ROW + UIHelper.MARGIN_TOP;

                Rect notBuildInspireCheck = new Rect(innerRect.x, innerRect.y + innerHeightSum, innerWidth, UIHelper.HEIGHT_ROW);
                UIHelper.LabeledCheckBoxLeft(notBuildInspireCheck, "FT_NotBuildInspireLabel".Translate(), ref dc.notBuildInspireCheck, ref valueChanged);
                innerHeightSum += UIHelper.HEIGHT_ROW + UIHelper.MARGIN_TOP;

                if (!dc.notBuildInspireCheck)
                {
                    Rect addLevelByInspireInput = new Rect(innerRect.x, innerRect.y + innerHeightSum, innerWidth, UIHelper.HEIGHT_ROW);
                    dc.addLevelByInspire = (int)Widgets.HorizontalSlider(addLevelByInspireInput, dc.addLevelByInspire, 0, 20, true, "FT_AddLevelByInspireValue".Translate(dc.addLevelByInspire), "0", "20");
                    innerHeightSum += UIHelper.HEIGHT_ROW;

                    Rect addLevelByInspireTips = new Rect(innerRect.x, innerRect.y + innerHeightSum, innerWidth, UIHelper.HEIGHT_ROW);
                    Text.Anchor = TextAnchor.MiddleCenter;
                    if (dc.addLevelByInspire > 0)
                    {
                        Widgets.Label(addLevelByInspireTips, "FT_AddLevelByInspireOnTips".Translate(dc.addLevelByInspire));
                    }
                    else
                    {
                        Widgets.Label(addLevelByInspireTips, "FT_AddLevelByInspireOffTips".Translate());
                    }
                    Text.Anchor = textAnchorBk;
                    innerHeightSum += UIHelper.HEIGHT_ROW + UIHelper.MARGIN_TOP;
                }

                if (ModsConfig.IdeologyActive)
                {
                    Rect horizontalLine = new Rect(new Rect(innerRect.x, innerRect.y + innerHeightSum, innerWidth, 0.5f));
                    Widgets.DrawBoxSolid(horizontalLine, Color.grey);

                    Text.Font = GameFont.Medium;
                    Rect addLevelByRoleEffectLabel = new Rect(innerRect.x, innerRect.y + innerHeightSum, innerWidth, UIHelper.HEIGHT_ROW);
                    Widgets.Label(addLevelByRoleEffectLabel, "FT_AddLevelByRoleEffectLabel".Translate());
                    Text.Font = fontBk;
                    innerHeightSum += UIHelper.HEIGHT_ROW + UIHelper.MARGIN_TOP;

                    Rect addLevelByRoleEffectInput = new Rect(innerRect.x, innerRect.y + innerHeightSum, innerWidth, UIHelper.HEIGHT_ROW);
                    dc.addLevelByRoleEffect = (int)Widgets.HorizontalSlider(addLevelByRoleEffectInput, dc.addLevelByRoleEffect, 0, 20, true, "FT_AddLevelByRoleEffectValue".Translate(dc.addLevelByRoleEffect), "0", "20");
                    innerHeightSum += UIHelper.HEIGHT_ROW;

                    Rect addLevelByRoleEffectTips = new Rect(innerRect.x, innerRect.y + innerHeightSum, innerWidth, UIHelper.HEIGHT_ROW);
                    Text.Anchor = TextAnchor.MiddleCenter;
                    if (dc.addLevelByRoleEffect > 0)
                    {
                        Widgets.Label(addLevelByRoleEffectTips, "FT_AddLevelByRoleEffectOnTips".Translate(dc.addLevelByRoleEffect));
                    }
                    else
                    {
                        Widgets.Label(addLevelByRoleEffectTips, "FT_AddLevelByRoleEffectOffTips".Translate());
                    }
                    Text.Anchor = textAnchorBk;
                    innerHeightSum += UIHelper.HEIGHT_ROW + UIHelper.MARGIN_TOP;
                }

                if (notBuildInspireCheckBk != dc.notBuildInspireCheck ||
                addLevelByInspireBk != dc.addLevelByInspire ||
                addLevelByRoleEffectBk != dc.addLevelByRoleEffect)
                {
                    dc.bestPawnLastRefreshTicks[mapId] = tick;
                    dc.ValidateAndRefreshPawnStates();
                    dc.bestPawnStatesMap[mapId] = pawnStates = pawnStates.OrderByDescending(x => x.skill).ThenBy(x => CommonHelper.GetSettingPriority(x.pawn)).ToHashSet();
                }

                return innerHeightSum;
            }, fontBk);

            //TitleBar
            Text.Font = GameFont.Medium;
            Rect titleBar = new Rect(titleX, titleY, titleWidth, UIHelper.HEIGHT_ROW);
            GUI.DrawTexture(titleBar, ActiveTip.TooltipBGAtlas);
            //Rect titleIcon = new Rect(titleX, titleY, UIHelper.SIZE_BUTTON, UIHelper.HEIGHT_ROW);
            //Widgets.DrawTextureFitted(titleIcon, Images.TEX_ICON_BIG, 1f);

            string enableState = dc.enabled ? "FT_EnableState_Enable".Translate() : "FT_EnableState_Disable".Translate();
            string enabledCheckBoxLabel = "FT_EnableState".Translate(enableState);
            Rect enabledCheckBox = new Rect(titleX, titleY, 500f, UIHelper.HEIGHT_ROW);
            UIHelper.LabeledCheckBoxLeft(enabledCheckBox, enabledCheckBoxLabel, ref dc.enabled, ref valueChanged);

            //Rect titleLabel = new Rect(titleX + UIHelper.SIZE_BUTTON + UIHelper.MARGIN_LEFT, titleY, 300f, UIHelper.HEIGHT_ROW);
            //Widgets.Label(titleLabel, "FT_TitleBar_TitleLabel".Translate());

            //TitlePanel Buttons
            Rect titleButtonBG = new Rect(titleX + titleWidth - 29f, titleY + 1f, 28f, 28f);
            Widgets.DrawTextureFitted(titleButtonBG, Images.TEX_BG_ICON, 1f);
            Rect titleButtonClose = new Rect(titleX + titleWidth - 26f, titleY + 5f, 22f, 22f);
            if (Widgets.ButtonImageFitted(titleButtonClose, TexButton.CloseXSmall))
            {
                this.Close();
            }

            this.windowRect.height = heightSum + UIHelper.MARGIN_TOP;

            Text.WordWrap = textWordWrapBk;
            Text.Anchor = textAnchorBk;
            Text.Font = fontOriginBk;
            GUI.color = colorBk;
        }

        public SettingDialog()
        {
            base.closeOnClickedOutside = true;
            base.doCloseButton = false;
            base.resizeable = false;
            this.absorbInputAroundWindow = true;
            this.forcePause = true;
            this.doWindowBackground = true;
            this.draggable = true;
        }

        public override void PostOpen()
        {
            this.opened = true;
        }

        public override void PreClose()
        {
            this.opened = false;
        }

        public override Vector2 InitialSize => new Vector2(500f, 500f);
    }
}
