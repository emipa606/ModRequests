using System.Linq;
using UnityEngine;
using Verse;

namespace NightVision
{
    public class HediffTab
    {
        private Vector2 _hediffScrollPosition = Vector2.zero;
        private int? _numberOfCustomHediffs;


        public void Clear()
        {
            _hediffScrollPosition = Vector2.zero;
            _numberOfCustomHediffs = null;
        }


        public void DrawTab(Rect inRect)
        {
            var cache = Settings.Cache;
            var store = Settings.Store;
            int hediffcount = cache.GetAllHediffs.Count;

            if (_numberOfCustomHediffs == null)
            {
                _numberOfCustomHediffs =
                            store.HediffLightMods.Count(hlm => hlm.Value.IntSetting == VisionType.NVCustom);
            }

            inRect = inRect.AtZero();

            SettingsHelpers.DrawLightModifiersHeader(
                ref inRect,
                "NVHediffs".Translate(),
                "NVHediffNote".Translate() + " " + "NVHediffNoteCont".Translate()
            );

            float num = inRect.y + 3f;

            var viewRect = new Rect(
                inRect.x,
                inRect.y,
                inRect.width * 0.9f,
                hediffcount
                * (Constants.ROW_HEIGHT + Constants.ROW_GAP)
                + (float)_numberOfCustomHediffs * 100f
            );

            var rowRect = new Rect(inRect.x + 6f, num, inRect.width - 12f, Constants.ROW_HEIGHT);
            Widgets.BeginScrollView(inRect, ref _hediffScrollPosition, viewRect);

            for (var i = 0; i < hediffcount; i++)
            {
                HediffDef hediffdef = cache.GetAllHediffs[i];
                rowRect.y = num;

                if (store.HediffLightMods.TryGetValue(hediffdef, out Hediff_LightModifiers hediffmods))
                {
                    _numberOfCustomHediffs +=
                                SettingsHelpers.DrawLightModifiersRow(
                                    hediffdef,
                                    hediffmods,
                                    rowRect,
                                    ref num,
                                    false
                                );
                }
                else
                {
                    Hediff_LightModifiers temp = store.AllEyeHediffs.Contains(hediffdef)
                                ? new Hediff_LightModifiers { AffectsEye = true }
                                : new Hediff_LightModifiers();

                    _numberOfCustomHediffs +=
                                SettingsHelpers.DrawLightModifiersRow(
                                    hediffdef,
                                    temp,
                                    rowRect,
                                    ref num,
                                    false
                                );

                    if (temp.IntSetting != VisionType.NVNone)
                    {
                        temp.InitialiseNewFromSettings(hediffdef);
                        store.HediffLightMods[hediffdef] = temp;
                    }
                }

                num += Constants.ROW_HEIGHT + Constants.ROW_GAP;

                if (i < hediffcount)
                {
                    Widgets.DrawLineHorizontal(
                        rowRect.x + 6f,
                        num - (Constants.ROW_GAP / 2 - 0.5f),
                        rowRect.width - 12f
                    );
                }
            }

            Widgets.EndScrollView();
        }
    }
}