using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace NightVision
{
    public class RaceTab
    {
        private int? _numberOfCustomRaces;
        private Vector2 _raceScrollPosition = Vector2.zero;

        public void Clear()
        {
            _numberOfCustomRaces = null;
            _raceScrollPosition = Vector2.zero;
        }


        public void DrawTab(Rect inRect)
        {
            var raceCount = Settings.Store.RaceLightMods.Count;

            if (_numberOfCustomRaces == null)
            {
                _numberOfCustomRaces =
                    Settings.Store.RaceLightMods.Count(rlm => rlm.Value.IntSetting == VisionType.NVCustom);
            }

            inRect = inRect.AtZero();
            SettingsHelpers.DrawLightModifiersHeader(ref inRect, "NVRaces".Translate(), "NVRaceNote".Translate());

            var heightMarker = inRect.y + 3f;

            var viewRect = new Rect(
                inRect.x,
                inRect.y,
                inRect.width * 0.9f,
                raceCount
                * (Constants.ROW_HEIGHT + Constants.ROW_GAP)
                + (float)_numberOfCustomRaces * 100f
            );

            var rowRect = new Rect(inRect.x + 6f, heightMarker, inRect.width - 12f, Constants.ROW_HEIGHT);
            Widgets.BeginScrollView(inRect, ref _raceScrollPosition, viewRect);
            var count = 0;

            foreach (KeyValuePair<ThingDef, Race_LightModifiers> kvp in Settings.Store.RaceLightMods)
            {
                var givenColor = GUI.color;
                rowRect.y = heightMarker;

                if (!kvp.Value.ShouldShowInSettings)
                {
                    if (!Prefs.DevMode)
                    {
                        continue;
                    }

                    GUI.color = Color.grey;
                    Widgets.Label(rowRect.TopPartPixels(20f), "NVDevModeOnly".Translate());
                    rowRect.y += 20;
                    heightMarker += 20;
                }

                _numberOfCustomRaces +=
                            SettingsHelpers.DrawLightModifiersRow(kvp.Key, kvp.Value, rowRect, ref heightMarker, true);

                if (!kvp.Value.ShouldShowInSettings)
                {
                    GUI.color = givenColor;
                }

                count++;
                heightMarker += Constants.ROW_HEIGHT + Constants.ROW_GAP;

                if (count < raceCount)
                {
                    Widgets.DrawLineHorizontal(rowRect.x + 6f, heightMarker - 5.5f, rowRect.width - 12f);
                }
            }

            Widgets.EndScrollView();
        }
    }
}