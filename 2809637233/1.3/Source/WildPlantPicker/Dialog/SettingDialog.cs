using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using WildPlantPicker.DataStore.Entities;
using WildPlantPicker.Helper;
using WildPlantPicker.DataStore;
using WildPlantPicker.StaticResources;
using RimWorld.Planet;

namespace WildPlantPicker.Dialog
{
    public class SettingDialog : Window
    {
        const float MAIN_MARGIN = UIHelper.MARGIN_TOP;
        private Vector2 m_ScrollbarPosition = Vector2.zero;
        private float m_ScrollHeightBk = 0f;

        private bool m_CommonSectionOpened = true;
        private bool m_TargetPlantSectionOpened = true;
        private bool m_MapConditionSectionOpened = true;

        public bool m_Opened = false;

        protected override float Margin => 0f;

        public override void DoWindowContents(Rect inRect)
        {
            DataContext dc = DataContext.Current();
            HashSet<TargetPlantCondition> targetPlantDataObjects = dc.GetTargetPlantConditions();
            int currentCount = targetPlantDataObjects.Count();

            bool textWordWrapBk = Text.WordWrap;
            TextAnchor textAnchorBk = Text.Anchor;
            GameFont fontOriginBk = Text.Font;
            GameFont fontBk = GameFont.Small;
            Color colorBk = GUI.color;
            Text.Anchor = TextAnchor.MiddleLeft;

            bool valueChanged = false;
            float fixHeaderHeightSum = 0f;

            //TitleBar and BottomBar position fixed
            float titleX = inRect.x, titleY = inRect.y, titleWidth = inRect.width;
            float bottomX = titleX, bottomY = inRect.height - UIHelper.HEIGHT_ROW, bottomWidth = titleWidth, bottomHeight = UIHelper.HEIGHT_ROW;
            fixHeaderHeightSum += UIHelper.HEIGHT_ROW;

            //AutoHarvest Setting
            Rect autoHarvestSection = new Rect(inRect.x, inRect.y + fixHeaderHeightSum, inRect.width, UIHelper.HEIGHT_ROW);
            Widgets.DrawTitleBG(autoHarvestSection);
            float xIndent = 0f;
            Text.Font = GameFont.Medium;
            string autoHarvestEnabledCheckBoxLabel = "WPP_AutoHarvestEnabledLabel".Translate();
            float autoHarvestEnabledCheckBoxWidth = Text.CalcSize(autoHarvestEnabledCheckBoxLabel).x + UIHelper.WIDTH_BUTTON + UIHelper.MARGIN_LEFT;
            Rect autoHarvestEnabledCheckBox = new Rect(inRect.x, inRect.y + fixHeaderHeightSum, autoHarvestEnabledCheckBoxWidth, UIHelper.HEIGHT_ROW);
            UIHelper.LabeledCheckBoxLeft(autoHarvestEnabledCheckBox, autoHarvestEnabledCheckBoxLabel, ref dc.m_AutoHarvestEnabled, ref valueChanged);
            Text.Font = fontBk;
            if (dc.m_AutoHarvestEnabled)
            {
                xIndent = autoHarvestEnabledCheckBoxWidth + UIHelper.MARGIN_LEFT;
                float autoHarvestTypeSelectButtonWidth = UIHelper.GetMaxTextWidth(((AutoHarvestType[])Enum.GetValues(typeof(AutoHarvestType))).Select(enumValue => CommonHelper.TranslateEnum(enumValue, dc))) + UIHelper.WIDTH_BUTTON;
                Rect autoHarvestTypeSelectButton = new Rect(inRect.x + xIndent, inRect.y + fixHeaderHeightSum, autoHarvestTypeSelectButtonWidth, UIHelper.HEIGHT_ROW);
                Text.Anchor = TextAnchor.MiddleCenter;
                if (Widgets.ButtonTextSubtle(autoHarvestTypeSelectButton, CommonHelper.TranslateEnum(dc.m_AutoHarvestType, dc)))
                {
                    FloatMenuHelper.AutoHarvestTypeGetter(selectedAutoHarvestType => dc.m_AutoHarvestType = selectedAutoHarvestType);
                }
                xIndent += autoHarvestTypeSelectButtonWidth + UIHelper.MARGIN_LEFT;
                if (dc.m_AutoHarvestType == AutoHarvestType.DayCycle)
                {
                    Text.Anchor = TextAnchor.MiddleLeft;
                    string autoHarvestTypeDayCycleNoMssageCheckBoxLabel = "WPP_DayCycle_NoMessage".Translate();
                    Rect autoHarvestTypeDayCycleNoMssageCheckBox = new Rect(inRect.x + xIndent, inRect.y + fixHeaderHeightSum, Text.CalcSize(autoHarvestTypeDayCycleNoMssageCheckBoxLabel).x + UIHelper.WIDTH_BUTTON, UIHelper.HEIGHT_ROW);
                    UIHelper.LabeledCheckBoxLeft(autoHarvestTypeDayCycleNoMssageCheckBox, autoHarvestTypeDayCycleNoMssageCheckBoxLabel, ref dc.m_DayCycleNoMessage, ref valueChanged);
                }
                else
                {
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Rect autoHarvestTypeAlwaysFrequencySlider = new Rect(inRect.x + xIndent, inRect.y + fixHeaderHeightSum, 150f, UIHelper.HEIGHT_ROW);
                    dc.m_AlwaysFrequency = (AlwaysFrequency)Mathf.RoundToInt(Widgets.HorizontalSlider(autoHarvestTypeAlwaysFrequencySlider, (byte)dc.m_AlwaysFrequency, 0, Defined.ALWAYS_FREQUENCY_MAX, true, CommonHelper.TranslateEnum(AlwaysFrequency.Middle, dc), CommonHelper.TranslateEnum(AlwaysFrequency.Low, dc), CommonHelper.TranslateEnum(AlwaysFrequency.High, dc), 0));

                    Text.Anchor = TextAnchor.MiddleLeft;
                    xIndent += 185f;
                    string recommended = dc.m_AlwaysFrequency == AlwaysFrequency.Low ? "WPP_AlwaysFrequency_Recommended".Translate() : null;
                    Rect autoHarvestTypeAlwaysFrequencyLabel = new Rect(inRect.x + xIndent, inRect.y + fixHeaderHeightSum, 150f, UIHelper.HEIGHT_ROW);
                    Widgets.Label(autoHarvestTypeAlwaysFrequencyLabel, "WPP_AlwaysFrequency_SelectedLabel_Temp".Translate(CommonHelper.TranslateEnum(dc.m_AlwaysFrequency, dc), recommended));
                }
            }
            Text.Anchor = TextAnchor.MiddleLeft;
            fixHeaderHeightSum += UIHelper.HEIGHT_ROW;
            Widgets.DrawLineHorizontal(inRect.x, inRect.y + fixHeaderHeightSum, inRect.width);

            inRect.x = MAIN_MARGIN;
            inRect.y = MAIN_MARGIN;
            inRect.width = inRect.width - (MAIN_MARGIN * 2f);
            inRect.height = inRect.height - (MAIN_MARGIN * 2f);

            Rect outRect = inRect;
            outRect.x += UIHelper.MARGIN_LEFT;
            outRect.y += (fixHeaderHeightSum + UIHelper.MARGIN_TOP);
            outRect.width -= (UIHelper.MARGIN_LEFT * 2f);
            outRect.height -= (fixHeaderHeightSum + UIHelper.MARGIN_TOP + UIHelper.HEIGHT_ROW);

            Map map = null;
            bool anyMapRenderedNow = false;
            float viewHeightSum = 0f;
            Rect viewRect = new Rect(outRect.x, outRect.y, outRect.width - UIHelper.MARGIN_SCROLL_BAR, m_ScrollHeightBk);
            UIHelper.ScrollBlock(outRect, ref m_ScrollbarPosition, ref viewRect, ref viewHeightSum, ref m_ScrollHeightBk, () =>
            {
                float viewX = viewRect.x + UIHelper.MARGIN_LEFT;
                float viewY = viewRect.y + UIHelper.MARGIN_TOP;
                float viewWidth = viewRect.width - (UIHelper.MARGIN_LEFT * 2f);

                //共通設定
                Rect commonSection = new Rect(viewX, viewY + viewHeightSum, viewWidth, UIHelper.HEIGHT_SECTION_TITLE);
                viewHeightSum += UIHelper.WriteFixedSectionTitleBox(commonSection, "WPP_CommonSectionTitle".Translate(), ref m_CommonSectionOpened, new Rect(commonSection.x + UIHelper.MARGIN_LEFT, commonSection.y + commonSection.height, commonSection.width - (UIHelper.MARGIN_LEFT * 2f), 0f), innerRect => {

                    float innerHeightSum = 0f;
                    Rect forceDesignationCheckBox = new Rect(innerRect.x, innerRect.y + innerHeightSum, innerRect.width, UIHelper.HEIGHT_ROW);
                    UIHelper.LabeledCheckBoxLeft(forceDesignationCheckBox, "WPP_ForceDesignationLabel".Translate(), ref dc.m_ForceDesignation, ref valueChanged);
                    innerHeightSum += UIHelper.HEIGHT_ROW;

                    Rect onlyPlayerColonyCheckBox = new Rect(innerRect.x, innerRect.y + innerHeightSum, innerRect.width, UIHelper.HEIGHT_ROW);
                    UIHelper.LabeledCheckBoxLeft(onlyPlayerColonyCheckBox, "WPP_OnlyPlayerColonyLabel".Translate(), ref dc.m_OnlyPlayerColony, ref valueChanged);
                    innerHeightSum += UIHelper.HEIGHT_ROW;

                    return innerHeightSum;
                }, fontBk) + UIHelper.HEIGHT_ROW;

                //収穫対象設定
                Rect targetPlantSection = new Rect(viewX, viewY + viewHeightSum, viewWidth, UIHelper.HEIGHT_SECTION_TITLE);
                viewHeightSum += UIHelper.WriteFixedSectionTitleBox(targetPlantSection, "WPP_TargetPlantSectionTitle".Translate(), ref m_TargetPlantSectionOpened, new Rect(targetPlantSection.x + UIHelper.MARGIN_LEFT, targetPlantSection.y + targetPlantSection.height, targetPlantSection.width - (UIHelper.MARGIN_LEFT * 2f), 0f), innerRect => {
                    float innerHeightSum = 0f;
                    float plantNameMaxWidth = new float[] { Text.CalcSize(Defs.ThingCategoryDefOf.Plants.LabelCap).x }.Concat(targetPlantDataObjects.Select(p => Text.CalcSize(p.m_PlantDef.LabelCap).x)).Max() + 5f;
                    float[] targetPlantGridColumnWidths = new float[] { UIHelper.SIZE_ICON, plantNameMaxWidth, 250f };
                    TargetPlantColumnConfig[] columns = new TargetPlantColumnConfig[] {
                        new TargetPlantColumnConfig(targetPlantGridColumnWidths[0], Images.TEX_HARVEST_PLANT, (targetPlant, dataRect, isEnabled) => {  //植物画像
                            Text.Anchor = TextAnchor.MiddleCenter;
                            Widgets.DrawTextureFitted(dataRect, Widgets.GetIconFor(targetPlant.m_PlantDef), 1f);
                            Text.Anchor = textAnchorBk;
                        }),
                        new TargetPlantColumnConfig(targetPlantGridColumnWidths[1], "WPP_Plant".Translate().ToString(), (targetPlant, dataRect, isEnabled) => {  //植物
                            Text.Anchor = TextAnchor.MiddleCenter;
                            if (!isEnabled || targetPlant.m_HarvestThreshold == HarvestThreshold.None)
                            {
                                GUI.color = Color.grey;
                            }
                            Widgets.Label(dataRect, targetPlant.m_PlantDef.LabelCap);
                            GUI.color = colorBk;
                            Text.Anchor = textAnchorBk;
                            if (!targetPlant.m_Immutable && Widgets.ButtonInvisible(dataRect))
                            {
                                FloatMenuUtility.MakeMenu(new TargetPlantCondition[] { targetPlant }, tp => "WPP_TargetPlantRemoveFromRow_Temp".Translate(tp.m_PlantDef.LabelCap), tp => () => {
                                    targetPlantDataObjects.Remove(tp);
                                    dc.RemoveCondition(tp);
                                    dc.Restore();
                                });
                            }
                        }),
                        new TargetPlantColumnConfig(targetPlantGridColumnWidths[2], "WPP_HarvestThreshold".Translate().ToString(), (targetPlant, dataRect, isEnabled) => {  //収穫閾値
                            Text.Anchor = TextAnchor.MiddleCenter;
                            if (isEnabled)
                            {
                                Rect sliderRect = new Rect(dataRect.x, dataRect.y, dataRect.width, dataRect.height - 10f);
                                targetPlant.m_HarvestThreshold = (HarvestThreshold)Mathf.RoundToInt(Widgets.HorizontalSlider(sliderRect, (byte)targetPlant.m_HarvestThreshold, 0, Defined.HARVEST_THRESHOLD_MAX, true, CommonHelper.TranslateEnum(HarvestThreshold.Harvestable, dc), CommonHelper.TranslateEnum(HarvestThreshold.None, dc), CommonHelper.TranslateEnum(HarvestThreshold.MaxGrowth, dc), 0));
                                Text.Anchor = TextAnchor.LowerCenter;
                                Text.Font = GameFont.Tiny;
                                switch (targetPlant.m_HarvestThreshold)
                                {
                                    case HarvestThreshold.None:
                                        Widgets.Label(dataRect, "WPP_HarvestThreshold_None_Text".Translate());
                                        break;
                                    case HarvestThreshold.Harvestable:
                                        Widgets.Label(dataRect, "WPP_HarvestThreshold_Harvestable_Text".Translate());
                                        break;
                                    case HarvestThreshold.MaxGrowth:
                                        Widgets.Label(dataRect, "WPP_HarvestThreshold_MaxGrowth_Text".Translate());
                                        break;
                                }
                                Text.Font = fontBk;
                            }
                            else
                            {
                                GUI.color = Color.grey;
                                Widgets.Label(dataRect, "WPP_DisabledBySetting".Translate());
                                GUI.color = colorBk;
                            }
                            Text.Anchor = textAnchorBk;
                        }),
                    };
                    float gridX = innerRect.x, gridY = innerRect.y + innerHeightSum, headerY = gridY, bodyY = headerY + UIHelper.HEIGHT_ROW, gridWidth = columns.Select(column => column.Width).Sum() + UIHelper.MARGIN_LEFT, gridHeight = 0f;

                    //収穫対象設定グリッドHeader
                    Text.Anchor = TextAnchor.MiddleCenter;
                    float headerX = gridX;
                    foreach (TargetPlantColumnConfig column in columns)
                    {
                        Rect headerColumnRect = new Rect(headerX, headerY, column.Width, UIHelper.HEIGHT_ROW);
                        if (column.TryGetTexture(out Texture2D tex))
                        {
                            Widgets.DrawTextureFitted(headerColumnRect, tex, 1f);
                        }
                        else
                        {
                            Widgets.Label(headerColumnRect, column.HeaderItem as string);
                        }
                        headerX += column.Width + UIHelper.MARGIN_GRID_COLUMN;
                    }
                    innerHeightSum += UIHelper.HEIGHT_ROW;

                    //収穫対象設定グリッドBody
                    for (int i = 0; currentCount == targetPlantDataObjects.Count() && i < currentCount; i++)
                    {
                        TargetPlantCondition bodyData = targetPlantDataObjects.ElementAt(i);
                        float bodyX = gridX;
                        Rect rowRect = new Rect(gridX, bodyY, innerRect.width, UIHelper.HEIGHT_ROW_GRID_WIDE);
                        foreach (TargetPlantColumnConfig column in columns)
                        {
                            Rect bodyColumnRect = new Rect(bodyX, bodyY, column.Width, UIHelper.HEIGHT_ROW_GRID_WIDE);
                            column.BodyColumnWriterGetter(bodyData, bodyColumnRect, bodyData.IsEnabled(dc));
                            bodyX += column.Width + UIHelper.MARGIN_GRID_COLUMN;
                        }
                        GUI.color = colorBk;
                        UIHelper.HighlightRect(rowRect);
                        GUI.DrawTexture(new Rect(gridX, bodyY, innerRect.width, 0.5f), BaseContent.GreyTex);
                        bodyY += UIHelper.HEIGHT_ROW_GRID_WIDE;
                        innerHeightSum += UIHelper.HEIGHT_ROW_GRID_WIDE;
                    }
                    if (currentCount != targetPlantDataObjects.Count())
                    {
                        return innerHeightSum;
                    }
                    GUI.DrawTexture(new Rect(gridX, bodyY, innerRect.width, 0.5f), BaseContent.GreyTex);
                    gridHeight = bodyY - gridY;

                    //収穫対象設定グリッドAdd
                    Rect targetPlantsAddButton = new Rect(gridX, gridY + gridHeight, UIHelper.WIDTH_BUTTON, UIHelper.HEIGHT_ROW);
                    if (Widgets.ButtonImageFitted(targetPlantsAddButton, TexButton.Add))
                    {
                        FloatMenuHelper.TargetPlantAddGetter(targetPlantDataObjects.Select(plant => plant.m_PlantDef), addPlant => {
                            dc.AddCondition(new TargetPlantCondition(addPlant));
                            dc.Restore();
                        });
                    }
                    //収穫対象設定グリッドRemove
                    Rect targetPlantsMinusButton = new Rect(gridX + UIHelper.WIDTH_BUTTON, gridY + gridHeight, UIHelper.WIDTH_BUTTON, UIHelper.HEIGHT_ROW);
                    if (Widgets.ButtonImageFitted(targetPlantsMinusButton, TexButton.Empty))
                    {
                        FloatMenuHelper.TargetPlantRemoveGetter(targetPlantDataObjects, removePlant => {
                            targetPlantDataObjects.Remove(removePlant);
                            dc.RemoveCondition(removePlant);
                            dc.Restore();
                        });
                    }

                    innerHeightSum += UIHelper.HEIGHT_ROW + UIHelper.MARGIN_TOP;
                    Text.Anchor = TextAnchor.MiddleLeft;

                    Rect onlyStrictlyWildPlantsCheckBox = new Rect(innerRect.x, innerRect.y + innerHeightSum, innerRect.width, UIHelper.HEIGHT_ROW);
                    UIHelper.LabeledCheckBoxLeft(onlyStrictlyWildPlantsCheckBox, "WPP_OnlyStrictlyWildPlantsLabel".Translate(), ref dc.m_OnlyStrictlyWildPlants, ref valueChanged);
                    innerHeightSum += UIHelper.HEIGHT_ROW;

                    return innerHeightSum;

                }, fontBk);

                map = Find.CurrentMap;
                anyMapRenderedNow = !WorldRendererUtility.WorldRenderedNow && map != null;
                if (currentCount != targetPlantDataObjects.Count() || !anyMapRenderedNow || !dc.GetMapConditions().TryGetValue(map.uniqueID, out MapCondition mapCondition))
                {
                    viewHeightSum += UIHelper.MARGIN_TOP;
                    return;
                }
                viewHeightSum += UIHelper.HEIGHT_ROW;

                //マップ別設定
                Rect mapConditionSection = new Rect(viewX, viewY + viewHeightSum, viewWidth, UIHelper.HEIGHT_SECTION_TITLE);
                viewHeightSum += UIHelper.WriteFixedSectionTitleBox(mapConditionSection, "WPP_MapConditionSectionTitle".Translate(), ref m_MapConditionSectionOpened, new Rect(mapConditionSection.x + UIHelper.MARGIN_LEFT, mapConditionSection.y + mapConditionSection.height, mapConditionSection.width - (UIHelper.MARGIN_LEFT * 2f), 0f), innerRect => {
                    float innerHeightSum = 0f;

                    Rect mapAllowedLabel = new Rect(innerRect.x, innerRect.y + innerHeightSum, innerRect.width, UIHelper.HEIGHT_ROW);
                    UIHelper.LabeledCheckBoxLeft(mapAllowedLabel, "WPP_IsMapAllowedLabel".Translate(), ref mapCondition.m_IsMapAllowed, ref valueChanged);
                    innerHeightSum += UIHelper.HEIGHT_ROW + UIHelper.MARGIN_TOP;

                    float harvestedThingDefIconAndNameMaxWidth = new float[] { Text.CalcSize("WPP_PlantAndHarvestedThingDefLabel".Translate()).x }.Concat(targetPlantDataObjects.Select(p => Text.CalcSize(p.m_PlantDef.plant.harvestedThingDef.LabelCap).x)).Max() + (UIHelper.SIZE_ICON * 2f) + (UIHelper.MARGIN_LEFT * 3f);
                    float intMaxWidth = Text.CalcSize(int.MaxValue.ToString()).x + UIHelper.MARGIN_LEFT;
                    float wishCountWidth = UIHelper.SIZE_BUTTON + (intMaxWidth * 2f) + UIHelper.MARGIN_LEFT;
                    int wishCountMaxLength = int.MaxValue.ToString().Count();
                    float allowGrowingZoneWidth = Text.CalcSize("WPP_AllowGrowingZoneLabel".Translate()).x + UIHelper.MARGIN_LEFT;

                    float[] mapConditionGridColumnWidths = new float[] { UIHelper.SIZE_BUTTON, harvestedThingDefIconAndNameMaxWidth, 150f, wishCountWidth, allowGrowingZoneWidth };
                    MapConditionColumnConfig[] columns = new MapConditionColumnConfig[] {
                            new MapConditionColumnConfig(mapConditionGridColumnWidths[0], Images.TEX_HARVEST_PLANT, (modConditionUnit, dataRect, isEnabled) => {  //収穫許可チェックボックス
                                Text.Anchor = TextAnchor.MiddleCenter;
                                if (mapCondition.m_IsMapAllowed)
                                {
                                    UIHelper.SingleCheckBox(dataRect, ref modConditionUnit.m_Harvestable, ref valueChanged, Widgets.CheckboxSize, Widgets.CheckboxOnTex, Widgets.CheckboxOffTex);
                                }
                                Text.Anchor = textAnchorBk;
                            }),
                            new MapConditionColumnConfig(mapConditionGridColumnWidths[1], "WPP_PlantAndHarvestedThingDefLabel".Translate().ToString(), (modConditionUnit, dataRect, isEnabled) => {  //収穫物画像および名前
                                Text.Anchor = TextAnchor.MiddleLeft;
                                Rect plantIconRect = new Rect(dataRect.x, dataRect.y, UIHelper.SIZE_ICON, dataRect.height);
                                Widgets.DrawTextureFitted(plantIconRect, Widgets.GetIconFor(modConditionUnit.m_HarvestSourceDef), 1f);
                                UIHelper.ToolTipRect(plantIconRect, modConditionUnit.m_HarvestSourceDef.LabelCap);
                                Rect harvestedThingIconRect = new Rect(dataRect.x + plantIconRect.width + UIHelper.MARGIN_LEFT, dataRect.y, UIHelper.SIZE_ICON, dataRect.height);
                                Widgets.DrawTextureFitted(harvestedThingIconRect, Widgets.GetIconFor(modConditionUnit.m_HarvestSourceDef.plant.harvestedThingDef), 1f);
                                Rect labelRect = new Rect(harvestedThingIconRect.x + harvestedThingIconRect.width + UIHelper.MARGIN_LEFT, dataRect.y, dataRect.width - (harvestedThingIconRect.width + UIHelper.MARGIN_LEFT), dataRect.height);
                                if (!mapCondition.m_IsMapAllowed || !modConditionUnit.m_Harvestable || (modConditionUnit.m_UseWishResouceCount && map.resourceCounter.GetCount(modConditionUnit.m_HarvestSourceDef.plant.harvestedThingDef) >= modConditionUnit.m_WishResouceCount))
                                {
                                    GUI.color = Color.grey;
                                }
                                Widgets.Label(labelRect, modConditionUnit.m_HarvestSourceDef.plant.harvestedThingDef.LabelCap);
                                GUI.color = colorBk;
                                Text.Anchor = textAnchorBk;
                            }) { m_ShouldShowHeaderDisabled = true },
                            new MapConditionColumnConfig(mapConditionGridColumnWidths[2], $"BUTTON${"WPP_AllowedAreaLabel".Translate()}", (modConditionUnit, dataRect, isEnabled) => {  //収穫エリア
                                Text.Anchor = TextAnchor.MiddleCenter;
                                    Rect disableRect = new Rect(dataRect.x, dataRect.y, 200f, dataRect.height);
                                if (mapCondition.m_IsMapAllowed)
                                {
                                    if (modConditionUnit.m_Harvestable)
                                    {
                                        Texture2D fillTex;
                                        if (modConditionUnit.m_Area == null)
                                        {
                                            fillTex = BaseContent.GreyTex;
                                        }
                                        else
                                        {
                                            fillTex = modConditionUnit.m_Area.ColorTexture;
                                        }
                                        GUI.DrawTexture(dataRect, fillTex);
                                        Text.Font = GameFont.Tiny;
                                        Widgets.Label(dataRect, AreaUtility.AreaAllowedLabel_Area(modConditionUnit.m_Area));
                                        Text.Font = fontBk;
                                        if (Mouse.IsOver(dataRect))
                                        {
                                            modConditionUnit.m_Area?.MarkForDraw();
                                            Widgets.DrawBox(dataRect.ContractedBy(-1f), 1, null);
                                        }
                                        if (Widgets.ButtonInvisible(dataRect, true))
                                        {
                                            AreaUtility.MakeAllowedAreaListFloatMenu(a => modConditionUnit.SetArea(a), true, false, map);
                                        }
                                    }
                                    else
                                    {
                                        Text.Anchor = TextAnchor.MiddleLeft;
                                        GUI.color = Color.grey;
                                        Widgets.Label(disableRect, "WPP_DisabledBySetting".Translate());
                                        GUI.color = colorBk;
                                    }
                                }
                                else
                                {
                                    Text.Anchor = TextAnchor.MiddleLeft;
                                    GUI.color = Color.grey;
                                    Widgets.Label(disableRect, "WPP_DisabledBySetting".Translate());
                                    GUI.color = colorBk;
                                }
                                Text.Anchor = textAnchorBk;
                            }),
                            new MapConditionColumnConfig(mapConditionGridColumnWidths[3], "WPP_WishResouceCountLabel".Translate().ToString(), (modConditionUnit, dataRect, isEnabled) => {  //希望在庫数まで収穫

                                Text.Anchor = TextAnchor.MiddleCenter;
                                if (mapCondition.m_IsMapAllowed && modConditionUnit.m_Harvestable)
                                {
                                    Rect useWishCountRect = new Rect(dataRect.x, dataRect.y, UIHelper.SIZE_BUTTON, dataRect.height);
                                    UIHelper.SingleCheckBox(useWishCountRect, ref modConditionUnit.m_UseWishResouceCount, ref valueChanged, Widgets.CheckboxSize, Widgets.CheckboxOnTex, Widgets.CheckboxOffTex);
                                    if (modConditionUnit.m_UseWishResouceCount)
                                    {
                                        int resourceCount = map.resourceCounter.GetCount(modConditionUnit.m_HarvestSourceDef.plant.harvestedThingDef);
                                        if (resourceCount >= modConditionUnit.m_WishResouceCount)
                                        {
                                            GUI.color = Color.grey;
                                        }
                                        Text.Anchor = TextAnchor.MiddleRight;
                                        Rect resouceCountLabel = new Rect(dataRect.x + useWishCountRect.width + UIHelper.MARGIN_LEFT, dataRect.y, intMaxWidth, dataRect.height);
                                        Widgets.Label(resouceCountLabel, $"{resourceCount} /");
                                        GUI.color = colorBk;
                                        Rect wishCountInput = new Rect(dataRect.x + useWishCountRect.width + UIHelper.MARGIN_LEFT + resouceCountLabel.width + UIHelper.MARGIN_LEFT, dataRect.y, intMaxWidth, dataRect.height);
                                        string inputBuffer = modConditionUnit.m_WishResouceCount.ToString();
                                        string inputBufferResult = Widgets.TextField(wishCountInput, inputBuffer, wishCountMaxLength, Defined.INT_MAX_REGEX);
                                        if (inputBuffer != inputBufferResult && Defined.INT_MAX_REGEX.Match(inputBufferResult).Success)
                                        {
                                            modConditionUnit.m_WishResouceCount = Mathf.Clamp((int)long.Parse(inputBufferResult), 0, int.MaxValue);
                                        }
                                    }
                                    else
                                    {
                                        Text.Anchor = TextAnchor.MiddleLeft;
                                        Rect unUseWishResourceCountLabel = new Rect(dataRect.x + useWishCountRect.width + UIHelper.MARGIN_LEFT, dataRect.y, dataRect.width - (useWishCountRect.width + UIHelper.MARGIN_LEFT), dataRect.height);
                                        Widgets.Label(unUseWishResourceCountLabel, "WPP_UnUseWishResouceCountLabel".Translate());
                                    }
                                }
                                Text.Anchor = textAnchorBk;
                            }),
                            new MapConditionColumnConfig(mapConditionGridColumnWidths[4], "WPP_AllowGrowingZoneLabel".Translate().ToString(), (modConditionUnit, dataRect, isEnabled) => {  //農業ゾーンを許可チェックボックス
                                Text.Anchor = TextAnchor.MiddleCenter;
                                if (mapCondition.m_IsMapAllowed && modConditionUnit.m_Harvestable)
                                {
                                    UIHelper.SingleCheckBox(dataRect, ref modConditionUnit.m_AllowGrowingZone, ref valueChanged, Widgets.CheckboxSize, Widgets.CheckboxOnTex, Widgets.CheckboxOffTex);
                                }
                                Text.Anchor = textAnchorBk;
                            }),
                        };
                    float gridX = innerRect.x, gridY = innerRect.y + innerHeightSum, headerY = gridY, bodyY = headerY + UIHelper.HEIGHT_ROW, gridWidth = innerRect.width;

                    //マップ毎条件対象設定グリッドHeader
                    GUI.DrawTexture(new Rect(gridX, gridY, innerRect.width, 0.5f), BaseContent.GreyTex);
                    Text.Anchor = TextAnchor.MiddleCenter;
                    float headerX = gridX;
                    foreach (MapConditionColumnConfig column in columns)
                    {
                        Rect headerColumnRect = new Rect(headerX, headerY, column.Width, UIHelper.HEIGHT_ROW);
                        if (column.TryGetTexture(out Texture2D tex))
                        {
                            if (mapCondition.m_IsMapAllowed)
                            {
                                Widgets.DrawTextureFitted(headerColumnRect, tex, 1f);
                            }
                        }
                        else
                        {
                            string columnHeaderText = column.HeaderItem as string;
                            if (mapCondition.m_IsMapAllowed && (columnHeaderText?.StartsWith("BUTTON$") ?? false))
                            {
                                string buttonText = columnHeaderText.Split('$').Last();
                                if (Widgets.ButtonTextSubtle(headerColumnRect, buttonText))
                                {
                                    AreaUtility.MakeAllowedAreaListFloatMenu(a => {
                                        foreach (MapConditionUnit unit in mapCondition.m_Units)
                                        {
                                            unit.SetArea(a);
                                        }
                                    }, true, false, map);
                                }
                            }
                            else
                            {
                                if (mapCondition.m_IsMapAllowed || column.m_ShouldShowHeaderDisabled)
                                {
                                    Widgets.Label(headerColumnRect, columnHeaderText);
                                }
                            }
                        }
                        headerX += column.Width + UIHelper.MARGIN_GRID_COLUMN;
                        Text.Anchor = TextAnchor.MiddleCenter;
                    }
                    innerHeightSum += UIHelper.HEIGHT_ROW;

                    //収穫対象設定グリッドBody
                    foreach (MapConditionUnit bodyData in mapCondition.m_Units)
                    {
                        float bodyX = gridX;
                        Rect rowRect = new Rect(gridX, bodyY, innerRect.width, UIHelper.HEIGHT_ROW_GRID);
                        foreach (MapConditionColumnConfig column in columns)
                        {
                            Rect bodyColumnRect = new Rect(bodyX, bodyY, column.Width, UIHelper.HEIGHT_ROW_GRID);
                            column.BodyColumnWriterGetter(bodyData, bodyColumnRect, true);
                            bodyX += column.Width + UIHelper.MARGIN_GRID_COLUMN;
                        }
                        GUI.color = colorBk;
                        UIHelper.HighlightRect(rowRect);
                        GUI.DrawTexture(new Rect(gridX, bodyY, innerRect.width, 0.5f), BaseContent.GreyTex);
                        bodyY += UIHelper.HEIGHT_ROW_GRID;
                        innerHeightSum += UIHelper.HEIGHT_ROW_GRID;
                    }

                    return innerHeightSum;
                }, fontBk) + UIHelper.MARGIN_TOP;
            });

            //TitleBar
            Text.Font = GameFont.Medium;
            Rect titleBar = new Rect(titleX, titleY, titleWidth, UIHelper.HEIGHT_ROW);
            GUI.DrawTexture(titleBar, ActiveTip.TooltipBGAtlas);
            Rect titleIcon = new Rect(titleX, titleY, UIHelper.SIZE_BUTTON, UIHelper.HEIGHT_ROW);
            Widgets.DrawTextureFitted(titleIcon, Images.TEX_ICON_BIG, 1f);
            Rect titleLabel = new Rect(titleX + UIHelper.SIZE_BUTTON + UIHelper.MARGIN_LEFT, titleY, 300f, UIHelper.HEIGHT_ROW);
            Widgets.Label(titleLabel, "WPP_TitleBar_TitleLabel".Translate());

            //TitlePanel Buttons
            Rect titleButtonBG = new Rect(titleX + titleWidth - 29f, titleY + 1f, 28f, 28f);
            Widgets.DrawTextureFitted(titleButtonBG, Images.TEX_BG_ICON, 1f);
            Rect titleButtonClose = new Rect(titleX + titleWidth - 26f, titleY + 5f, 22f, 22f);
            if (Widgets.ButtonImageFitted(titleButtonClose, TexButton.CloseXSmall))
            {
                this.Close();
            }
            titleButtonBG.x = titleX + titleWidth - 57f;
            Widgets.DrawTextureFitted(titleButtonBG, Images.TEX_BG_ICON, 1f);
            Rect titleButtonReset = new Rect(titleX + titleWidth - 54f, titleY + 5f, 22f, 22f);
            if (Widgets.ButtonImageFitted(titleButtonReset, TexUI.RotLeftTex))
            {
                FloatMenuHelper.SimpleConfirmFloatMenu("WPP_TitleBar_SettingReset".Translate(), () => {
                    FloatMenuHelper.SimpleConfirmFloatMenu("WPP_ConfirmImportant".Translate(), () => {
                        dc.SettingDataReset();
                        dc.Restore();
                    });
                });
            }

            //BottomPanel
            Rect bottomBar = new Rect(bottomX, bottomY, bottomWidth, bottomHeight);
            GUI.DrawTexture(bottomBar, ActiveTip.TooltipBGAtlas);

            //BottomPanel Buttons
            Rect bottomButtonBG = new Rect(bottomX + 1f, bottomY + 1f, 28f, 28f);
            Rect bttomButtonHarvestNow = bottomButtonBG;
            Rect resourceCountRect = new Rect(bottomButtonBG.x, bottomButtonBG.y + 14f, 15f, 15f);

            Widgets.DrawTextureFitted(bottomButtonBG, Images.TEX_BG_ICON, 1f);
            if (Widgets.ButtonImageFitted(bttomButtonHarvestNow, TexButton.ShowExpandingIcons))
            {
                FloatMenuHelper.SimpleConfirmFloatMenu("WPP_BottomBar_ReCache".Translate(), () => dc.Restore());
            }
            UIHelper.ToolTipRect(bottomButtonBG, "WPP_BottomBar_ReCache".Translate());

            bottomButtonBG.x += 56f;
            bttomButtonHarvestNow.x += 56f;
            resourceCountRect.x += 56f;
            Widgets.DrawTextureFitted(bottomButtonBG, Images.TEX_ICON_EARTH, 1f);

            bottomButtonBG.x += 28f;
            bttomButtonHarvestNow.x += 28f;
            resourceCountRect.x += 28f;
            Widgets.DrawTextureFitted(bottomButtonBG, Images.TEX_BG_ICON, 1f);
            Widgets.DrawTextureFitted(bttomButtonHarvestNow, TexButton.ShowZones, 1f);
            Widgets.DrawTextureFitted(resourceCountRect, Widgets.GetIconFor(ThingDefOf.RawBerries), 1f);
            Widgets.DrawTextureFitted(bttomButtonHarvestNow, Images.TEX_HARVEST_PLANT, 1f);
            UIHelper.HighlightRect(bttomButtonHarvestNow);
            if (Widgets.ButtonInvisible(bttomButtonHarvestNow))
            {
                FloatMenuHelper.SimpleConfirmFloatMenu("WPP_BottomBar_HarvestDestinationNowWorld".Translate(), () => DesignationHelper.TryDesignationAll(dc, null, false, false));
            }
            UIHelper.ToolTipRect(bottomButtonBG, "WPP_BottomBar_HarvestDestinationNowWorld".Translate());

            bottomButtonBG.x += 28f;
            bttomButtonHarvestNow.x += 28f;
            resourceCountRect.x += 28f;
            Widgets.DrawTextureFitted(bottomButtonBG, Images.TEX_BG_ICON, 1f);
            Widgets.DrawTextureFitted(resourceCountRect, Widgets.GetIconFor(ThingDefOf.RawBerries), 1f);
            Widgets.DrawTextureFitted(bttomButtonHarvestNow, Images.TEX_HARVEST_PLANT, 1f);
            UIHelper.HighlightRect(bttomButtonHarvestNow);
            if (Widgets.ButtonInvisible(bttomButtonHarvestNow))
            {
                FloatMenuHelper.SimpleConfirmFloatMenu("WPP_BottomBar_HarvestIgnoreAreaWorld".Translate(), () => DesignationHelper.TryDesignationAll(dc, null, true, false));
            }
            UIHelper.ToolTipRect(bottomButtonBG, "WPP_BottomBar_HarvestIgnoreAreaWorld".Translate());

            bottomButtonBG.x += 28f;
            bttomButtonHarvestNow.x += 28f;
            resourceCountRect.x += 28f;
            Widgets.DrawTextureFitted(bottomButtonBG, Images.TEX_BG_ICON, 1f);
            Widgets.DrawTextureFitted(bttomButtonHarvestNow, TexButton.ShowZones, 1f);
            Widgets.DrawTextureFitted(bttomButtonHarvestNow, Images.TEX_HARVEST_PLANT, 1f);
            UIHelper.HighlightRect(bttomButtonHarvestNow);
            if (Widgets.ButtonInvisible(bttomButtonHarvestNow))
            {
                FloatMenuHelper.SimpleConfirmFloatMenu("WPP_BottomBar_HarvestIgnoreResourceCountWorld".Translate(), () => DesignationHelper.TryDesignationAll(dc, null, false, true));
            }
            UIHelper.ToolTipRect(bottomButtonBG, "WPP_BottomBar_HarvestIgnoreResourceCountWorld".Translate());

            bottomButtonBG.x += 28f;
            bttomButtonHarvestNow.x += 28f;
            resourceCountRect.x += 28f;
            Widgets.DrawTextureFitted(bottomButtonBG, Images.TEX_BG_ICON, 1f);
            Widgets.DrawTextureFitted(bttomButtonHarvestNow, Images.TEX_HARVEST_PLANT, 1f);
            UIHelper.HighlightRect(bttomButtonHarvestNow);
            if (Widgets.ButtonInvisible(bttomButtonHarvestNow))
            {
                FloatMenuHelper.SimpleConfirmFloatMenu("WPP_BottomBar_HarvestIgnoreBothWorld".Translate(), () => DesignationHelper.TryDesignationAll(dc, null, true, true));
            }
            UIHelper.ToolTipRect(bottomButtonBG, "WPP_BottomBar_HarvestIgnoreBothWorld".Translate());

            bottomButtonBG.x += 28f;
            bttomButtonHarvestNow.x += 28f;
            resourceCountRect.x += 28f;
            Widgets.DrawTextureFitted(bottomButtonBG, Images.TEX_BG_ICON, 1f);
            Widgets.DrawTextureFitted(bttomButtonHarvestNow, CompTransporter.CancelLoadCommandTex, 1f);
            UIHelper.HighlightRect(bttomButtonHarvestNow);
            if (Widgets.ButtonInvisible(bttomButtonHarvestNow))
            {
                FloatMenuHelper.SimpleConfirmFloatMenu("WPP_BottomBar_CancelHarvestDesignationWorld".Translate(), () => DesignationHelper.TryCancelHarvestDesignationAll(null));
            }
            UIHelper.ToolTipRect(bottomButtonBG, "WPP_BottomBar_CancelHarvestDesignationWorld".Translate());

            if (anyMapRenderedNow)
            {
                bottomButtonBG.x += 56f;
                bttomButtonHarvestNow.x += 56f;
                resourceCountRect.x += 56f;
                Widgets.DrawTextureFitted(bottomButtonBG, SettleUtility.SettleCommandTex, 1f);

                bottomButtonBG.x += 28f;
                bttomButtonHarvestNow.x += 28f;
                resourceCountRect.x += 28f;
                Widgets.DrawTextureFitted(bottomButtonBG, Images.TEX_BG_ICON, 1f);
                Widgets.DrawTextureFitted(bttomButtonHarvestNow, TexButton.ShowZones, 1f);
                Widgets.DrawTextureFitted(resourceCountRect, Widgets.GetIconFor(ThingDefOf.RawBerries), 1f);
                Widgets.DrawTextureFitted(bttomButtonHarvestNow, Images.TEX_HARVEST_PLANT, 1f);
                UIHelper.HighlightRect(bttomButtonHarvestNow);
                if (Widgets.ButtonInvisible(bttomButtonHarvestNow))
                {
                    FloatMenuHelper.SimpleConfirmFloatMenu("WPP_BottomBar_HarvestDestinationNowMap".Translate(), () => DesignationHelper.TryDesignationAll(dc, map, false, false));
                }
                UIHelper.ToolTipRect(bottomButtonBG, "WPP_BottomBar_HarvestDestinationNowMap".Translate());

                bottomButtonBG.x += 28f;
                bttomButtonHarvestNow.x += 28f;
                resourceCountRect.x += 28f;
                Widgets.DrawTextureFitted(bottomButtonBG, Images.TEX_BG_ICON, 1f);
                Widgets.DrawTextureFitted(resourceCountRect, Widgets.GetIconFor(ThingDefOf.RawBerries), 1f);
                Widgets.DrawTextureFitted(bttomButtonHarvestNow, Images.TEX_HARVEST_PLANT, 1f);
                UIHelper.HighlightRect(bttomButtonHarvestNow);
                if (Widgets.ButtonInvisible(bttomButtonHarvestNow))
                {
                    FloatMenuHelper.SimpleConfirmFloatMenu("WPP_BottomBar_HarvestIgnoreAreaMap".Translate(), () => DesignationHelper.TryDesignationAll(dc, map, true, false));
                }
                UIHelper.ToolTipRect(bottomButtonBG, "WPP_BottomBar_HarvestIgnoreAreaMap".Translate());

                bottomButtonBG.x += 28f;
                bttomButtonHarvestNow.x += 28f;
                resourceCountRect.x += 28f;
                Widgets.DrawTextureFitted(bottomButtonBG, Images.TEX_BG_ICON, 1f);
                Widgets.DrawTextureFitted(bttomButtonHarvestNow, TexButton.ShowZones, 1f);
                Widgets.DrawTextureFitted(bttomButtonHarvestNow, Images.TEX_HARVEST_PLANT, 1f);
                UIHelper.HighlightRect(bttomButtonHarvestNow);
                if (Widgets.ButtonInvisible(bttomButtonHarvestNow))
                {
                    FloatMenuHelper.SimpleConfirmFloatMenu("WPP_BottomBar_HarvestIgnoreResourceCountMap".Translate(), () => DesignationHelper.TryDesignationAll(dc, map, false, true));
                }
                UIHelper.ToolTipRect(bottomButtonBG, "WPP_BottomBar_HarvestIgnoreResourceCountMap".Translate());

                bottomButtonBG.x += 28f;
                bttomButtonHarvestNow.x += 28f;
                resourceCountRect.x += 28f;
                Widgets.DrawTextureFitted(bottomButtonBG, Images.TEX_BG_ICON, 1f);
                Widgets.DrawTextureFitted(bttomButtonHarvestNow, Images.TEX_HARVEST_PLANT, 1f);
                UIHelper.HighlightRect(bttomButtonHarvestNow);
                if (Widgets.ButtonInvisible(bttomButtonHarvestNow))
                {
                    FloatMenuHelper.SimpleConfirmFloatMenu("WPP_BottomBar_HarvestIgnoreBothMap".Translate(), () => DesignationHelper.TryDesignationAll(dc, map, true, true));
                }
                UIHelper.ToolTipRect(bottomButtonBG, "WPP_BottomBar_HarvestIgnoreBothMap".Translate());

                bottomButtonBG.x += 28f;
                bttomButtonHarvestNow.x += 28f;
                resourceCountRect.x += 28f;
                Widgets.DrawTextureFitted(bottomButtonBG, Images.TEX_BG_ICON, 1f);
                Widgets.DrawTextureFitted(bttomButtonHarvestNow, CompTransporter.CancelLoadCommandTex, 1f);
                UIHelper.HighlightRect(bttomButtonHarvestNow);
                if (Widgets.ButtonInvisible(bttomButtonHarvestNow))
                {
                    FloatMenuHelper.SimpleConfirmFloatMenu("WPP_BottomBar_CancelHarvestDesignationMap".Translate(), () => DesignationHelper.TryCancelHarvestDesignationAll(map));
                }
                UIHelper.ToolTipRect(bottomButtonBG, "WPP_BottomBar_CancelHarvestDesignationMap".Translate());
            }

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
            this.m_Opened = true;
        }

        public override void PreClose()
        {
            this.m_Opened = false;
        }

        public override Vector2 InitialSize => new Vector2(900f,600f);
    }
}
