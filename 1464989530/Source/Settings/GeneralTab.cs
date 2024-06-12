using System.Diagnostics;
using UnityEngine;
using Verse;

namespace NightVision
{
    public class GeneralTab
    {
        private bool _askToConfirmReset;
        private Stopwatch confirmTimer = new Stopwatch();

        public void Clear()
        {
            _askToConfirmReset = false;
            confirmTimer.Reset();
        }

        public void DrawTab(Rect inRect)
        {
            TextAnchor anchor = Text.Anchor;
            float rowHeight = Constants.GEN_ROW_HEIGHT;
            var cache = Settings.Cache;

            var rowRect = new Rect(
                inRect.width * 0.05f,
                inRect.height * 0.05f,
                inRect.width * 0.9f,
                rowHeight
            );

            Text.Anchor = TextAnchor.MiddleLeft;

            Widgets.Label(rowRect, "NVVanillaMultiExp".Translate());
            rowRect.y += rowHeight + Constants.ROW_GAP;
            Widgets.DrawLineHorizontal(rowRect.x + 24f, rowRect.y, rowRect.width - 48f);
            rowRect.y += Constants.ROW_GAP;

            //Night Vision Settings

            Widgets.Label(
                rowRect,
                "NightVisionNVSettingDesc".Translate()
                            .ToLower()
                            .CapitalizeFirst()
            );

            rowRect.y += rowHeight + Constants.ROW_GAP;

            cache.NVZeroCache = Widgets.HorizontalSlider(
                rowRect,
                 cache.NVZeroCache.GetValueOrDefault(),
                 cache.MinCache.GetValueOrDefault(),
                cache.MaxCache.GetValueOrDefault(),
                true,
                string.Format(
                    Str.ZeroMultiLabel,
                    cache.NVZeroCache
                ),
                string.Format(
                    Str.XLabel,
                    cache.MinCache
                ),
                string.Format(
                    Str.XLabel,
                    cache.MaxCache
                ),
                1
            );

            SettingsHelpers.DrawIndicator(
                rowRect,
                Constants.DEFAULT_ZERO_LIGHT_MULTIPLIER,
                Constants.NVDefaultOffsets[0],
                cache.MinCache.GetValueOrDefault(),
                cache.MaxCache.GetValueOrDefault(),
                IndicatorTex.DefIndicator
            );

            rowRect.y += rowHeight * 1.5f;

            cache.NVFullCache = Widgets.HorizontalSlider(
                rowRect,
                (float)cache.NVFullCache.GetValueOrDefault(),
                (float)cache.MinCache.GetValueOrDefault(),
                (float)cache.MaxCache.GetValueOrDefault(),
                true,
                string.Format(
                    Str.FullMultiLabel,
                    cache.NVFullCache
                ),
                string.Format(
                    Str.XLabel,
                    cache.MinCache
                ),
                string.Format(
                    Str.XLabel,
                    cache.MaxCache
                ),
                1
            );

            SettingsHelpers.DrawIndicator(
                rowRect,
                Constants.DEFAULT_FULL_LIGHT_MULTIPLIER,
                Constants.NVDefaultOffsets[1],
                (float)cache.MinCache.GetValueOrDefault(),
                (float)cache.MaxCache.GetValueOrDefault(),
                IndicatorTex.DefIndicator
            );

            rowRect.y += rowHeight * 2f;
            Widgets.DrawLineHorizontal(rowRect.x + 24f, rowRect.y, rowRect.width - 48f);
            rowRect.y += Constants.ROW_GAP;

            // Photosensitivity settings

            Widgets.Label(
                rowRect,
                "NightVisionPSSettingsDesc".Translate()
                            .ToLower()
                            .CapitalizeFirst()
            );

            rowRect.y += rowHeight * 1.5f;

            cache.PSZeroCache = Widgets.HorizontalSlider(
                rowRect,
                (float)cache.PSZeroCache,
                (float)cache.MinCache,
                (float)cache.MaxCache,
                true,
                string.Format(
                    Str.ZeroMultiLabel,
                    cache.PSZeroCache
                ),
                string.Format(
                    Str.XLabel,
                    cache.MinCache
                ),
                string.Format(
                    Str.XLabel,
                    cache.MaxCache
                ),
                1
            );

            SettingsHelpers.DrawIndicator(
                rowRect,
                Constants.DEFAULT_ZERO_LIGHT_MULTIPLIER,
                Constants.PSDefaultOffsets[0],
                (float)cache.MinCache,
                (float)cache.MaxCache,
                IndicatorTex.DefIndicator
            );

            rowRect.y += rowHeight * 1.5f;

            cache.PSFullCache = Widgets.HorizontalSlider(
                rowRect,
                (float)cache.PSFullCache.GetValueOrDefault(),
                (float)cache.MinCache.GetValueOrDefault(),
                (float)cache.MaxCache.GetValueOrDefault(),
                true,
                string.Format(
                    Str.FullMultiLabel,
                    cache.PSFullCache
                ),
                string.Format(
                    Str.XLabel,
                    cache.MinCache
                ),
                string.Format(
                    Str.XLabel,
                    cache.MaxCache
                ),
                1
            );

            SettingsHelpers.DrawIndicator(
                rowRect,
                Constants.DEFAULT_FULL_LIGHT_MULTIPLIER,
                Constants.PSDefaultOffsets[1],
                (float)cache.MinCache,
                (float)cache.MaxCache,
                IndicatorTex.DefIndicator
            );

            rowRect.y += rowHeight * 2f;
            Widgets.DrawLineHorizontal(rowRect.x + 24f, rowRect.y, rowRect.width - 48f);
            rowRect.y += Constants.ROW_GAP;


            Widgets.CheckboxLabeled(rowRect, "NightVisionFlareRaidEnabled".Translate(), ref NVGameComponent.FlareRaidIsEnabled);
            rowRect.y += rowHeight * 2f;
            Widgets.DrawLineHorizontal(rowRect.x + 24f, rowRect.y, rowRect.width - 48f);
            rowRect.y += Constants.ROW_GAP;



            if (_askToConfirmReset)
            {
                if (!confirmTimer.IsRunning)
                {
                    confirmTimer.Start();
                }

                Color color = GUI.color;
                GUI.color = Color.red;

                if (Widgets.ButtonText(rowRect, "NVConfirmReset".Translate()) && confirmTimer.ElapsedMilliseconds > 500)
                {
                    Settings.Store.ResetAllSettings();
                    confirmTimer.Reset();
                }

                GUI.color = color;

                if (confirmTimer.ElapsedMilliseconds > 5000)
                {
                    _askToConfirmReset = false;
                    confirmTimer.Reset();
                }
            }
            else
            {
                _askToConfirmReset = Widgets.ButtonText(rowRect, "NVReset".Translate());
            }

            Text.Anchor = anchor;
        }
    }
}