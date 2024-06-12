// Nightvision NightVision SettingsHelpers.cs
// 
// 03 08 2018
// 
// 16 10 2018

using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace NightVision
{
    public static class SettingsHelpers
    {
        private static Dictionary<Def, string> _tipStringHolder;

        public static Dictionary<Def, string> TipStringHolder
        {
            get
                => _tipStringHolder
                   ?? (_tipStringHolder = new Dictionary<Def, string>());
            set => _tipStringHolder = value;
        }

        public static float ModToMultiPercent(
            float mod,
            bool isZeroLight
        )
        {
            return (float)Math.Round(
                100f
                * (mod
                   + (isZeroLight
                               ? Constants.DEFAULT_ZERO_LIGHT_MULTIPLIER
                               : Constants.DEFAULT_FULL_LIGHT_MULTIPLIER))
            );
        }

        public static float MultiPercentToMod(
            float multipercent,
            bool isZeroLight
        )
        {
            return (float)Math.Round(multipercent / 100f, Constants.NUMBER_OF_DIGITS)
                   - (isZeroLight
                               ? Constants.DEFAULT_ZERO_LIGHT_MULTIPLIER
                               : Constants.DEFAULT_FULL_LIGHT_MULTIPLIER);
        }

        public static void DrawIndicator(
            Rect rowRect,
            float baseVal,
            float modVal,
            float min,
            float max,
            Texture2D indicator
        )
        {
            var posOfDefault = rowRect.x
                                 + 6f
                                 + (rowRect.width - 12f)
                                 * ((baseVal + modVal) * 100 - min)
                                 / (max - min);

            rowRect.position = new Vector2(
                posOfDefault - 0.5f * Constants.INDICATOR_SIZE,
                rowRect.y + rowRect.height * 0.95f
            );

            rowRect.width = Constants.INDICATOR_SIZE;
            rowRect.height = Constants.INDICATOR_SIZE;
            Widgets.DrawTextureFitted(rowRect, indicator, 1);
        }

        private static void DrawIndicators(
            Rect zeroRect,
            Rect fullRect,
            LightModifiersBase lightModifiers,
            float minZero,
            float maxZero,
            float minFull,
            float maxFull
        )
        {
            //int eyeCount = lightModifiers is Race_LightModifiers rlm ? rlm.EyeCount : 1; 
            //Draw indicators on zero light rect
            DrawIndicator(
                zeroRect,
                Constants.DEFAULT_ZERO_LIGHT_MULTIPLIER,
                LightModifiersBase.PSLightModifiers[0],
                minZero,
                maxZero,
                IndicatorTex.PsIndicator
            );

            DrawIndicator(
                zeroRect,
                Constants.DEFAULT_ZERO_LIGHT_MULTIPLIER,
                LightModifiersBase.NVLightModifiers[0],
                minZero,
                maxZero,
                IndicatorTex.NvIndicator
            );

            DrawIndicator(
                zeroRect,
                Constants.DEFAULT_ZERO_LIGHT_MULTIPLIER,
                lightModifiers.DefaultOffsets[0],
                minZero,
                maxZero,
                IndicatorTex.DefIndicator
            );

            //Draw indicators on full light rect
            DrawIndicator(
                fullRect,
                Constants.DEFAULT_FULL_LIGHT_MULTIPLIER,
                LightModifiersBase.PSLightModifiers[1],
                minFull,
                maxFull,
                IndicatorTex.PsIndicator
            );

            DrawIndicator(
                fullRect,
                Constants.DEFAULT_FULL_LIGHT_MULTIPLIER,
                LightModifiersBase.NVLightModifiers[1],
                minFull,
                maxFull,
                IndicatorTex.NvIndicator
            );

            DrawIndicator(
                fullRect,
                Constants.DEFAULT_FULL_LIGHT_MULTIPLIER,
                lightModifiers.DefaultOffsets[1],
                minFull,
                maxFull,
                IndicatorTex.DefIndicator
            );
        }

        public static string GetTipString(
            Def def,
            LightModifiersBase lightModifiers
        )
        {
            if (TipStringHolder == null)
            {
                TipStringHolder = new Dictionary<Def, string>();
            }

            if (TipStringHolder.TryGetValue(def, out var tip))
            {
                return tip;
            }

            var result = def.description ?? def.LabelCap;

            if (lightModifiers is Hediff_LightModifiers hediffMods)
            {
                if (hediffMods.AffectsEye)
                {
                    result += "\n" + "NVHediffQualifier".Translate();
                }

                if (hediffMods.AutoAssigned)
                {
                    result += "\n" + "NVHediffAutoAssigned".Translate(hediffMods.DefaultSetting.ToString().Translate());
                }
                else if (Math.Abs(lightModifiers.DefaultOffsets[0]) > Constants.NV_EPSILON
                         || Math.Abs(lightModifiers.DefaultOffsets[1]) > Constants.NV_EPSILON)
                {
                    result += "\n"
                              + "NVLoadedFromFile".Translate(
                                  hediffMods.DefaultSetting.ToString().Translate(),
                                  DefaultValuesLine(hediffMods.DefaultOffsets, false)
                              );
                }
            }
            else if (lightModifiers is Race_LightModifiers rlm
                     && (Math.Abs(lightModifiers.DefaultOffsets[0]) > Constants.NV_EPSILON
                         || Math.Abs(lightModifiers.DefaultOffsets[1]) > Constants.NV_EPSILON))
            {
                result += "\n"
                          + "NVLoadedFromFile".Translate(
                              Race_LightModifiers.GetSetting(rlm).ToString().Translate(),
                              DefaultValuesLine(rlm.DefaultOffsets, true)
                          );
            }

            TipStringHolder[def] = result;

            return result;
        }

        public static string DefaultValuesLine(float[] offsets, bool addBaseValues)
        {
            var zeroValue = offsets[0];
            var fullValue = offsets[1];
            if (addBaseValues)
            {
                zeroValue += Constants.DEFAULT_ZERO_LIGHT_MULTIPLIER;
                fullValue += Constants.DEFAULT_FULL_LIGHT_MULTIPLIER;
            }

            return $"\n  0% lit: {zeroValue.ToStringSignedWholePercent()}\n  100% lit: {fullValue.ToStringSignedWholePercent()}";
        }

        public static void DrawLightModifiersHeader(
            ref Rect inRect,
            string label,
            string tooltip
        )
        {
            var headerRect = new Rect(inRect.x + 6f, inRect.y, inRect.width - 12f, inRect.height * 0.1f);

            var labelwidth = headerRect.width * 0.3f;
            var labelRect = new Rect(headerRect.x, headerRect.y, labelwidth, headerRect.height);
            var xSettings = headerRect.x + labelwidth + 1f + headerRect.width * 0.05f;

            var settingsRect = new Rect(
                xSettings,
                headerRect.y,
                headerRect.xMax - xSettings,
                headerRect.height
            );

            var font = Text.Font;
            Text.Font = GameFont.Medium;
            Widgets.Label(labelRect, label);
            TooltipHandler.TipRegion(labelRect, new TipSignal(tooltip));
            Widgets.Label(settingsRect.TopPart(0.6f), "NVGlowModOptions".Translate());
            var num = settingsRect.width / 3;
            settingsRect = settingsRect.BottomPart(0.4f);
            Text.Font = GameFont.Tiny;
            settingsRect.width = num;

            Widgets.Label(
                settingsRect,
                new GUIContent(
                    "default".Translate().ToLower().CapitalizeFirst(),
                    IndicatorTex.DefIndicator
                )
            );

            settingsRect.x += num;

            Widgets.Label(
                settingsRect,
                new GUIContent(
                    "NVNightVision".Translate().ToLower().CapitalizeFirst(),
                    IndicatorTex.NvIndicator
                )
            );

            settingsRect.x += num;

            Widgets.Label(
                settingsRect,
                new GUIContent(
                    "NVPhotosensitivity".Translate().ToLower().CapitalizeFirst(),
                    IndicatorTex.PsIndicator
                )
            );

            Text.Font = font;

            Widgets.DrawLineHorizontal(
                headerRect.x + 10f,
                headerRect.yMax + Constants.ROW_GAP / 4 - 0.5f,
                headerRect.width - 20f
            );

            inRect.yMin += headerRect.height + Constants.ROW_GAP / 2;
        }

        /// <summary>
        ///     0 if LightModifiers is unchanged, 1 if LightModifiers changed to custom, -1 if LightModifiers changed from custom
        /// </summary>
        /// <param name="rowRect"></param>
        /// <param name="num">the y-coord of the rect: is increased if rect needs more space</param>
        /// <param name="def"></param>
        /// <param name="lightMods"></param>
        /// <param name="isRace"></param>
        public static int DrawLightModifiersRow(
            Def def,
            LightModifiersBase lightMods,
            Rect rowRect,
            ref float num,
            bool isRace
        )
        {
            var result = 0;
            var labelWidth = rowRect.width * 0.3f;
            var labelRect = new Rect(rowRect.x, rowRect.y, labelWidth, rowRect.height);

            var buttonWidth = rowRect.width * 0.14f;
            var buttonGap = rowRect.width * 0.025f;
            var currentX = rowRect.x + labelWidth + buttonGap;

            Widgets.DrawAltRect(labelRect.ContractedBy(2f));
            Widgets.Label(labelRect, def.LabelCap);

            TooltipHandler.TipRegion(
                labelRect,
                new TipSignal(
                    GetTipString(def, lightMods),
                    labelRect.GetHashCode()
                )
            );

            Widgets.DrawLineVertical(currentX, rowRect.y, rowRect.height);

            //LightModifiers.Options =  enum: default = 0; nightvis = 1; photosens = 2; custom = 3
            for (var i = 0; i < 4; i++)
            {
                var iOption = (VisionType)Enum.ToObject(typeof(VisionType), i);
                currentX += buttonGap;
                var buttonRect = new Rect(currentX, rowRect.y + 6f, buttonWidth, rowRect.height - 12f);

                if (iOption == lightMods.Setting)
                {
                    var color = GUI.color;
                    GUI.color = Color.yellow;
                    Widgets.DrawBox(buttonRect.ExpandedBy(2f));
                    GUI.color = color;
                    Widgets.DrawAtlas(buttonRect, Widgets.ButtonSubtleAtlas);
                    Widgets.Label(buttonRect, iOption.ToString().Translate());
                }
                else
                {
                    var changesetting =
                                Widgets.ButtonText(buttonRect, iOption.ToString().Translate());

                    if (changesetting)
                    {
                        if (lightMods.IsCustom())
                        {
                            result = -1;
                        }

                        lightMods.ChangeSetting(iOption);

                        if (lightMods.IsCustom())
                        {
                            result = 1;
                        }
                    }
                }

                currentX += buttonWidth;
            }

            if (!lightMods.IsCustom())
            {
                return result;
            }

            num += Constants.ROW_HEIGHT + Constants.ROW_GAP;

            var topRect = new Rect(
                labelRect.xMax + 2 * buttonGap,
                num,
                rowRect.width - labelRect.width - 60f,
                Constants.ROW_HEIGHT / 2
            );

            var bottomRect = new Rect(
                labelRect.xMax + 2 * buttonGap,
                topRect.yMax + Constants.ROW_GAP * 2,
                rowRect.width - labelRect.width - 60f,
                Constants.ROW_HEIGHT / 2
            );

            var explanationRect = new Rect(
                labelRect.xMax - labelRect.width * 0.95f,
                num,
                labelRect.width * 0.9f,
                Constants.ROW_HEIGHT
            );

            Widgets.DrawLineVertical(
                labelRect.xMax + buttonGap,
                rowRect.y + rowRect.height,
                Constants.ROW_HEIGHT * 2 - Constants.ROW_GAP * 0.5f
            );


            var zeroModAsPercent = (float)Math.Round(
                (lightMods.Offsets[0]
               + (isRace ? Constants.DEFAULT_ZERO_LIGHT_MULTIPLIER : 0))
              * 100
            );

            var fullModAsPercent = (float)Math.Round(
                (lightMods.Offsets[1]
               + (isRace ? Constants.DEFAULT_FULL_LIGHT_MULTIPLIER : 0))
              * 100
            );

            var cache = Settings.Cache;

            if (isRace)
            {
                var font = Text.Font;
                Text.Font = GameFont.Tiny;

                Widgets.Label(
                    explanationRect,
                    "NVMoveWorkSpeedMultipliers".Translate("").Trim().CapitalizeFirst()
                  + "\n"
                  + "NVRaceQualifier"
                        .Translate()
                        .CapitalizeFirst()
                );

                Text.Font = font;
                float min;
                float max;

                if (((Race_LightModifiers)lightMods).CanCheat)
                {
                    min = (float)Math.Round(Storage.LowestCap * 100);
                    max = (float)Math.Round(Storage.HighestCap * 100);
                }
                else
                {
                    min = (float)cache.MinCache.GetValueOrDefault();
                    max = (float)cache.MaxCache.GetValueOrDefault();
                }

                zeroModAsPercent = Widgets.HorizontalSlider(
                    topRect,
                    zeroModAsPercent,
                    min,
                    max,
                    true,
                    string.Format(
                        Str.ZeroMultiLabel,
                        zeroModAsPercent
                    ),
                    string.Format(Str.XLabel, min),
                    string.Format(Str.XLabel, max),
                    1
                );

                fullModAsPercent = Widgets.HorizontalSlider(
                    bottomRect,
                    fullModAsPercent,
                    min,
                    max,
                    true,
                    string.Format(
                        Str.FullMultiLabel,
                        fullModAsPercent
                    ),
                    string.Format(Str.XLabel, min),
                    string.Format(Str.XLabel, max),
                    1
                );

                DrawIndicators(topRect, bottomRect, lightMods, min, max, min, max);
            }
            else
            {
                var font = Text.Font;
                Text.Font = GameFont.Tiny;

                Widgets.Label(
                    explanationRect,
                    "NVMoveWorkSpeedModifiers".Translate(def.LabelCap).CapitalizeFirst()
                );

                Text.Font = font;

                zeroModAsPercent = Widgets.HorizontalSlider(
                    topRect,
                    zeroModAsPercent,
                    (float)cache.MinCache.GetValueOrDefault() - 80,
                    (float)cache.MaxCache.GetValueOrDefault() - 80,
                    true,
                    string.Format(Str.ZeroLabel, zeroModAsPercent),
                    string.Format(
                        Str.Alabel,
                        cache.MinCache - 80
                    ),
                    string.Format(
                        Str.Alabel,
                        cache.MaxCache - 80
                    ),
                    1
                );


                fullModAsPercent = Widgets.HorizontalSlider(
                    bottomRect,
                    fullModAsPercent,
                    (float)cache.MinCache - 100,
                    (float)cache.MaxCache - 100,
                    true,
                    string.Format(Str.FullLabel, fullModAsPercent),
                    string.Format(
                        Str.Alabel,
                        cache.MinCache - 100
                    ),
                    string.Format(
                        Str.Alabel,
                        cache.MaxCache - 100
                    ),
                    1
                );

                DrawIndicators(
                    topRect,
                    bottomRect,
                    lightMods,
                    (float)cache.MinCache,
                    (float)cache.MaxCache,
                    (float)cache.MinCache,
                    (float)cache.MaxCache
                );
            }

            if (!Mathf.Approximately(zeroModAsPercent / 100, lightMods.Offsets[0]))
            {
                lightMods[0] = zeroModAsPercent / 100
                             - (isRace ? Constants.DEFAULT_ZERO_LIGHT_MULTIPLIER : 0);
            }

            if (!Mathf.Approximately(fullModAsPercent / 100, lightMods.Offsets[1]))
            {
                lightMods[1] = fullModAsPercent / 100
                             - (isRace ? Constants.DEFAULT_FULL_LIGHT_MULTIPLIER : 0);
            }

            num += Constants.ROW_HEIGHT * 0.9f /*+ rowGap*/;

            return result;
        }
    }
}