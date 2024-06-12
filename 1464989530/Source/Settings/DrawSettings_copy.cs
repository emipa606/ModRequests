using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace NightVision
{
    public static class DrawSettings
    {
        private static List<Pawn> _allPawns;
        private static Vector2    _apparelScrollPosition = Vector2.zero;
        private static Vector2    _debugScrollPos        = Vector2.zero;
        private static Vector2    _hediffScrollPosition  = Vector2.zero;
        private static float      _maxY;
        private static int?       _numberOfCustomHediffs;
        private static int?       _numberOfCustomRaces;
        private static Vector2    _raceScrollPosition = Vector2.zero;
        private static bool _askToConfirmReset;
        public static Stopwatch confirmTimer = new Stopwatch();
        #if DEBUG
        public static bool LogPawnComps;
        #endif
        public static void Clear()
        {
            DrawSettings._apparelScrollPosition = Vector2.zero;
            DrawSettings._debugScrollPos        = Vector2.zero;
            DrawSettings._hediffScrollPosition  = Vector2.zero;
            DrawSettings._raceScrollPosition    = Vector2.zero;
            DrawSettings._allPawns              = null;
            DrawSettings._maxY                  = -1;
            DrawSettings._numberOfCustomHediffs = null;
            DrawSettings._numberOfCustomRaces   = null;
            DrawSettings._askToConfirmReset = false;
            DrawSettings.confirmTimer.Reset();
        }

        public static void GeneralTab(
                        Rect inRect
                    )
        {
            //TODO GeneralTab: Add  thought settings?
            TextAnchor anchor    = Text.Anchor;
            float      rowHeight = Constants.GenRowHeight;

            var rowRect = new Rect(
                                   inRect.width  * 0.05f,
                                   inRect.height * 0.05f,
                                   inRect.width  * 0.9f,
                                   rowHeight
                                  );

            Text.Anchor = TextAnchor.MiddleLeft;

            Widgets.Label(rowRect, "NVVanillaMultiExp".Translate());
            rowRect.y += rowHeight + Constants.RowGap;
            Widgets.DrawLineHorizontal(rowRect.x + 24f, rowRect.y, rowRect.width - 48f);
            rowRect.y += Constants.RowGap;

            //Night Vision Settings

            Widgets.Label(
                          rowRect,
                          "NVMoveWorkSpeedMultipliers"
                                      .Translate(VisionType.NVNightVision.ToString().Translate())
                                      .ToLower()
                                      .CapitalizeFirst()
                         );

            rowRect.y += rowHeight + Constants.RowGap;

            SettingsCache.NVZeroCache = Widgets.HorizontalSlider(
                                                                 rowRect,
                                                                 (float) SettingsCache.NVZeroCache,
                                                                 (float) SettingsCache.MinCache,
                                                                 (float) SettingsCache.MaxCache,
                                                                 true,
                                                                 string.Format(
                                                                               Constants.ZeroMultiLabel,
                                                                               SettingsCache.NVZeroCache
                                                                              ),
                                                                 string.Format(
                                                                               Constants.XLabel,
                                                                               SettingsCache.MinCache
                                                                              ),
                                                                 string.Format(
                                                                               Constants.XLabel,
                                                                               SettingsCache.MaxCache
                                                                              ),
                                                                 1
                                                                );

            SettingsHelpers.DrawIndicator(
                                          rowRect,
                                          Constants.DefaultZeroLightMultiplier,
                                          Constants.NVDefaultOffsets[0],
                                          (float) SettingsCache.MinCache,
                                          (float) SettingsCache.MaxCache,
                                          IndicatorTex.DefIndicator
                                         );

            rowRect.y += rowHeight * 1.5f;

            SettingsCache.NVFullCache = Widgets.HorizontalSlider(
                                                                 rowRect,
                                                                 (float) SettingsCache.NVFullCache,
                                                                 (float) SettingsCache.MinCache,
                                                                 (float) SettingsCache.MaxCache,
                                                                 true,
                                                                 string.Format(
                                                                               Constants.FullMultiLabel,
                                                                               SettingsCache.NVFullCache
                                                                              ),
                                                                 string.Format(
                                                                               Constants.XLabel,
                                                                               SettingsCache.MinCache
                                                                              ),
                                                                 string.Format(
                                                                               Constants.XLabel,
                                                                               SettingsCache.MaxCache
                                                                              ),
                                                                 1
                                                                );

            SettingsHelpers.DrawIndicator(
                                          rowRect,
                                          Constants.DefaultFullLightMultiplier,
                                          Constants.NVDefaultOffsets[1],
                                          (float) SettingsCache.MinCache,
                                          (float) SettingsCache.MaxCache,
                                          IndicatorTex.DefIndicator
                                         );

            rowRect.y += rowHeight * 2f;
            Widgets.DrawLineHorizontal(rowRect.x + 24f, rowRect.y, rowRect.width - 48f);
            rowRect.y += Constants.RowGap;

            //Photosensitivity settings

            Widgets.Label(
                          rowRect,
                          "NVMoveWorkSpeedMultipliers"
                                      .Translate(VisionType.NVPhotosensitivity.ToString().Translate())
                                      .ToLower()
                                      .CapitalizeFirst()
                         );

            rowRect.y += rowHeight * 1.5f;

            SettingsCache.PSZeroCache = Widgets.HorizontalSlider(
                                                                 rowRect,
                                                                 (float) SettingsCache.PSZeroCache,
                                                                 (float) SettingsCache.MinCache,
                                                                 (float) SettingsCache.MaxCache,
                                                                 true,
                                                                 string.Format(
                                                                               Constants.ZeroMultiLabel,
                                                                               SettingsCache.PSZeroCache
                                                                              ),
                                                                 string.Format(
                                                                               Constants.XLabel,
                                                                               SettingsCache.MinCache
                                                                              ),
                                                                 string.Format(
                                                                               Constants.XLabel,
                                                                               SettingsCache.MaxCache
                                                                              ),
                                                                 1
                                                                );

            SettingsHelpers.DrawIndicator(
                                          rowRect,
                                          Constants.DefaultZeroLightMultiplier,
                                          Constants.PSDefaultOffsets[0],
                                          (float) SettingsCache.MinCache,
                                          (float) SettingsCache.MaxCache,
                                          IndicatorTex.DefIndicator
                                         );

            rowRect.y += rowHeight * 1.5f;

            SettingsCache.PSFullCache = Widgets.HorizontalSlider(
                                                                 rowRect,
                                                                 (float) SettingsCache.PSFullCache,
                                                                 (float) SettingsCache.MinCache,
                                                                 (float) SettingsCache.MaxCache,
                                                                 true,
                                                                 string.Format(
                                                                               Constants.FullMultiLabel,
                                                                               SettingsCache.PSFullCache
                                                                              ),
                                                                 string.Format(
                                                                               Constants.XLabel,
                                                                               SettingsCache.MinCache
                                                                              ),
                                                                 string.Format(
                                                                               Constants.XLabel,
                                                                               SettingsCache.MaxCache
                                                                              ),
                                                                 1
                                                                );

            SettingsHelpers.DrawIndicator(
                                          rowRect,
                                          Constants.DefaultFullLightMultiplier,
                                          Constants.PSDefaultOffsets[1],
                                          (float) SettingsCache.MinCache,
                                          (float) SettingsCache.MaxCache,
                                          IndicatorTex.DefIndicator
                                         );

            rowRect.y += rowHeight * 2f;
            Widgets.DrawLineHorizontal(rowRect.x + 24f, rowRect.y, rowRect.width - 48f);
            rowRect.y += Constants.RowGap;
            

            if (Settings.CEDetected)
            {
                Widgets.CheckboxLabeled(rowRect, "EnableNVForCE".Translate(), ref Storage.NVEnabledForCE);
                rowRect.y += rowHeight * 2f;
                Widgets.DrawLineHorizontal(rowRect.x + 24f, rowRect.y, rowRect.width - 48f);
                rowRect.y += Constants.RowGap;
            }


                
            if (DrawSettings._askToConfirmReset)
            {
                if (!DrawSettings.confirmTimer.IsRunning)
                {
                    DrawSettings.confirmTimer.Start();
                }
                Color color = GUI.color;
                GUI.color = Color.red;
                if (Widgets.ButtonText(rowRect, "NVConfirmReset"))
                {
                    Log.Message("NightVision.DrawSettings.GeneralTab: NVConfirm");
                    
                    Storage.ResetAllSettings();
                    DrawSettings.confirmTimer.Reset();
                }
                GUI.color = color;

                if (DrawSettings.confirmTimer.ElapsedMilliseconds > 5000)
                {
                    DrawSettings._askToConfirmReset = false;
                    DrawSettings.confirmTimer.Reset();
                }
            }
            else
            {
                DrawSettings._askToConfirmReset = Widgets.ButtonText(rowRect, "NVReset");

            }

            Text.Anchor = anchor;
        }

        public static void RaceTab(
                        Rect inRect
                    )
        {
            int raceCount = Storage.RaceLightMods.Count;

            if (DrawSettings._numberOfCustomRaces == null)
            {
                DrawSettings._numberOfCustomRaces =
                            Storage.RaceLightMods.Count(rlm => rlm.Value.IntSetting == VisionType.NVCustom);
            }

            inRect = inRect.AtZero();
            SettingsHelpers.DrawLightModifiersHeader(ref inRect, "NVRaces".Translate(), "NVRaceNote".Translate());

            //#region Tweaks

            //Constants.RowHeight = Widgets.HorizontalSlider(new Rect(inRect.x, inRect.y, inRect.width, Constants.RowHeight),
            //    Constants.RowHeight,
            //    -100f,
            //    +100f,
            //    true,
            //    $"Tweak RowHeight: {Constants.RowHeight}");
            //inRect.y += 50f;


            //#endregion
            float num = inRect.y + 3f;

            var viewRect = new Rect(
                                    inRect.x,
                                    inRect.y,
                                    inRect.width * 0.9f,
                                    raceCount
                                    * (Constants.RowHeight + Constants.RowGap)
                                    + (float) DrawSettings._numberOfCustomRaces * 100f
                                   );

            var rowRect = new Rect(inRect.x + 6f, num, inRect.width - 12f, Constants.RowHeight);
            Widgets.BeginScrollView(inRect, ref DrawSettings._raceScrollPosition, viewRect);
            var count = 0;

            foreach (KeyValuePair<ThingDef, Race_LightModifiers> kvp in Storage.RaceLightMods)
            {
                Color givenColor = GUI.color;
                rowRect.y = num;

                if (!kvp.Value.ShouldShowInSettings)
                {
                    if (!Prefs.DevMode)
                    {
                        continue;
                    }

                    GUI.color = Color.grey;
                    Widgets.Label(rowRect.TopPartPixels(20f), "NVDevModeOnly".Translate());
                    rowRect.y += 20;
                    num       += 20;
                }

                DrawSettings._numberOfCustomRaces +=
                            SettingsHelpers.DrawLightModifiersRow(kvp.Key, kvp.Value, rowRect, ref num, true);

                if (!kvp.Value.ShouldShowInSettings)
                {
                    GUI.color = givenColor;
                }

                count++;
                num += Constants.RowHeight + Constants.RowGap;

                if (count < raceCount)
                {
                    Widgets.DrawLineHorizontal(rowRect.x + 6f, num - 5.5f, rowRect.width - 12f);
                }
            }

            Widgets.EndScrollView();
        }

        public static void ApparelTab(
                        Rect inRect
                    )
        {
            Text.Anchor = TextAnchor.LowerCenter;
            int  apparelCount = SettingsCache.GetAllHeadgear.Count;
            var  headerRect   = new Rect(24f, 0f, inRect.width - 64f, 36f);
            Rect leftRect     = headerRect.LeftPart(0.4f);
            Rect midRect      = headerRect.RightPart(0.6f).LeftHalf().RightPart(0.8f);
            Rect rightRect    = headerRect.RightPart(0.6f).RightHalf().LeftPart(0.8f);
            Widgets.Label(leftRect,  "NVApparel".Translate());
            Widgets.Label(midRect,   "NVNullPS".Translate());
            Widgets.Label(rightRect, "NVGiveNV".Translate());

            Widgets.DrawLineHorizontal(headerRect.x + 12f, headerRect.yMax + 4f, headerRect.xMax - 64f);

            Text.Anchor = TextAnchor.MiddleCenter;
            var viewRect   = new Rect(32f, 48f, inRect.width - 64f, apparelCount * 48f);
            var scrollRect = new Rect(12f, 48f, inRect.width - 12f, inRect.height - 48f);

            var   checkboxSize = 20f;
            float leftBoxX     = midRect.center.x   + checkboxSize;
            float rightBoxX    = rightRect.center.x + checkboxSize;
            var   leftBox      = new Rect(leftBoxX,  0f, checkboxSize, checkboxSize);
            var   rightBox     = new Rect(rightBoxX, 0f, checkboxSize, checkboxSize);

            var num = 48f;
            Widgets.BeginScrollView(scrollRect, ref DrawSettings._apparelScrollPosition, viewRect);

            foreach (ThingDef appareldef in SettingsCache.GetAllHeadgear)
            {
                var rowRect = new Rect(scrollRect.x + 12f, num, scrollRect.width - 24f, 40);
                Widgets.DrawAltRect(rowRect);

                var  locGUIContent = new GUIContent(appareldef.LabelCap, appareldef.uiIcon);
                Rect apparelRect   = rowRect.LeftPart(0.4f);
                Widgets.Label(apparelRect, locGUIContent);

                TooltipHandler.TipRegion(
                                         apparelRect,
                                         new TipSignal(appareldef.DescriptionDetailed /*, apparelRect.GetHashCode()*/)
                                        );

                var leftBoxPos  = new Vector2(leftBoxX,  rowRect.center.y - checkboxSize / 2);
                var rightBoxPos = new Vector2(rightBoxX, rowRect.center.y - checkboxSize / 2);
                leftBox.y  = leftBoxPos.y;
                rightBox.y = leftBoxPos.y;
                TooltipHandler.TipRegion(leftBox,  new TipSignal("PSApparelExplained".Translate()));
                TooltipHandler.TipRegion(rightBox, new TipSignal("NVApparelExplained".Translate()));

                if (Storage.NVApparel.TryGetValue(appareldef, out ApparelVisionSetting apparelSetting))
                {
                    Widgets.Checkbox(leftBoxPos,  ref apparelSetting.NullifiesPS, checkboxSize);
                    Widgets.Checkbox(rightBoxPos, ref apparelSetting.GrantsNV,    checkboxSize);

                    if (!apparelSetting.Equals(Storage.NVApparel[appareldef]))
                    {
                        if (apparelSetting.IsRedundant())
                        {
                            Storage.NVApparel.Remove(appareldef);
                        }
                        else
                        {
                            Storage.NVApparel[appareldef] = apparelSetting;
                        }
                    }
                }
                else
                {
                    var nullPs = false;
                    var giveNV = false;
                    Widgets.Checkbox(leftBoxPos,  ref nullPs, checkboxSize);
                    Widgets.Checkbox(rightBoxPos, ref giveNV, checkboxSize);

                    if (nullPs || giveNV)
                    {
                        if (appareldef.GetCompProperties<CompProperties_NightVisionApparel>() is
                                    CompProperties_NightVisionApparel compprops)
                        {
                            apparelSetting = compprops.AppVisionSetting;
                        }
                        else
                        {
                            apparelSetting =
                                        ApparelVisionSetting.CreateNewApparelVisionSetting(appareldef);
                        }

                        apparelSetting.NullifiesPS    = nullPs;
                        apparelSetting.GrantsNV       = giveNV;
                        Storage.NVApparel[appareldef] = apparelSetting;
                    }
                }

                num += 48f;
            }

            Widgets.EndScrollView();
            Text.Anchor = TextAnchor.UpperLeft;
        }

        public static void HediffTab(
                        Rect inRect
                    )
        {
            int hediffcount = SettingsCache.GetAllHediffs.Count;

            if (DrawSettings._numberOfCustomHediffs == null)
            {
                DrawSettings._numberOfCustomHediffs =
                            Storage.HediffLightMods.Count(hlm => hlm.Value.IntSetting == VisionType.NVCustom);
            }

            inRect = inRect.AtZero();

            SettingsHelpers.DrawLightModifiersHeader(
                                                     ref inRect,
                                                     "NVHediffs".Translate(),
                                                     "NVHediffNote".Translate() + "NVHediffNoteCont".Translate()
                                                    );

            float num = inRect.y + 3f;

            var viewRect = new Rect(
                                    inRect.x,
                                    inRect.y,
                                    inRect.width * 0.9f,
                                    hediffcount
                                    * (Constants.RowHeight + Constants.RowGap)
                                    + (float) DrawSettings._numberOfCustomHediffs * 100f
                                   );

            var rowRect = new Rect(inRect.x + 6f, num, inRect.width - 12f, Constants.RowHeight);
            Widgets.BeginScrollView(inRect, ref DrawSettings._hediffScrollPosition, viewRect);

            for (var i = 0; i < hediffcount; i++)
            {
                HediffDef hediffdef = SettingsCache.GetAllHediffs[i];
                rowRect.y = num;

                if (Storage.HediffLightMods.TryGetValue(hediffdef, out Hediff_LightModifiers hediffmods))
                {
                    DrawSettings._numberOfCustomHediffs +=
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
                    Hediff_LightModifiers temp = Storage.AllEyeHediffs.Contains(hediffdef)
                                ? new Hediff_LightModifiers {AffectsEye = true}
                                : new Hediff_LightModifiers();

                    DrawSettings._numberOfCustomHediffs +=
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
                        Storage.HediffLightMods[hediffdef] = temp;
                    }
                }

                num += Constants.RowHeight + Constants.RowGap;

                if (i < hediffcount)
                {
                    Widgets.DrawLineHorizontal(
                                               rowRect.x     + 6f,
                                               num           - (Constants.RowGap / 2 - 0.5f),
                                               rowRect.width - 12f
                                              );
                }
            }

            Widgets.EndScrollView();
        }

        public static void DebugTab(
                        Rect inRect
                    )
        {
            bool playing = Current.ProgramState == ProgramState.Playing;
            inRect    = inRect.AtZero();
            Text.Font = GameFont.Tiny;
            float listingY = default;
#if DEBUG
                        var listing = new Listing_Standard();
                        listing.Begin(inRect);
                        listing.CheckboxLabeled("NVDebugLogComps".Translate(), ref LogPawnComps);
                        listing.GapLine();
                        listing.LabelDouble("NVDebugAverageTime".Translate(),
                            (NVHarmonyPatcher.TotalGlFactorNanoSec / NVHarmonyPatcher.TotalTicks).ToString("00 ns/tick"));
                        listing.Label($"1 tick = {1000000000 / 60:00} ns");
                        listing.GapLine();
                        listingY = listing.CurHeight;
                        listing.End();
#endif

            var rowRect = new Rect(
                                   inRect.width * 0.05f,
                                   inRect.height * 0.05f + listingY,
                                   inRect.width * 0.9f,
                                   Constants.RowHeight
                                  );

            Text.Anchor = TextAnchor.MiddleLeft;
            //Multiplier Limits
            Widgets.CheckboxLabeled(rowRect, "NVCustomCapsEnabled".Translate(), ref Storage.CustomCapsEnabled);

            if (Storage.CustomCapsEnabled)
            {
                rowRect.y += Constants.RowHeight;
                Text.Font =  GameFont.Tiny;
                Widgets.Label(rowRect, "NVCapsExp".Translate());
                //Text.Font =  GameFont.Small;
                rowRect.y += Constants.RowHeight /*+ Constants.RowGap*/;

                SettingsCache.MinCache = Widgets.HorizontalSlider(
                                                                  rowRect,
                                                                  (float) SettingsCache.MinCache,
                                                                  1f,
                                                                  100f,
                                                                  true,
                                                                  "NVSettingsMinCapLabel".Translate(
                                                                                                    SettingsCache
                                                                                                                .MinCache
                                                                                                   ),
                                                                  "1%",
                                                                  "100%",
                                                                  1
                                                                 );

                SettingsHelpers.DrawIndicator(
                                              rowRect.TopPart(0.9f),
                                              Constants.DefaultMinCap,
                                              0f,
                                              1f,
                                              100f,
                                              IndicatorTex.DefIndicator
                                             );

                rowRect.y += Constants.RowHeight;

                SettingsCache.MaxCache = Widgets.HorizontalSlider(
                                                                  rowRect,
                                                                  (float) SettingsCache.MaxCache,
                                                                  100f,
                                                                  200f,
                                                                  true,
                                                                  "NVSettingsMaxCapLabel".Translate(
                                                                                                    SettingsCache
                                                                                                                .MaxCache
                                                                                                   ),
                                                                  "100%",
                                                                  "200%",
                                                                  1
                                                                 );

                SettingsHelpers.DrawIndicator(
                                              rowRect.TopPart(0.9f),
                                              Constants.DefaultMaxCap,
                                              0f,
                                              100f,
                                              200f,
                                              IndicatorTex.DefIndicator
                                             );
            }

            rowRect.y += Constants.RowHeight;
            Widgets.DrawLineHorizontal(rowRect.x + 24f, rowRect.y, rowRect.width - 48f);
            float endY = rowRect.yMax;

            if (playing)
            {
                if (DrawSettings._allPawns == null)
                {
                    DrawSettings._allPawns = PawnsFinder.AllMaps_Spawned.Where(pwn => pwn.RaceProps.Humanlike).ToList();
                }

                float height;

                if (Math.Abs(DrawSettings._maxY) < 0.001)
                {
                    height = 25 * DrawSettings._allPawns.Count
                             + 200
                             * DrawSettings._allPawns
                                           .FindAll(
                                                    pawn => pawn.GetComp<Comp_NightVision>()
                                                            != null
                                                   )
                                           .Count;
                }
                else
                {
                    height = DrawSettings._maxY;
                }

                var remainRect = new Rect(
                                          inRect.x,
                                          endY + 5f,
                                          inRect.width,
                                          inRect.yMax - endY + 5f
                                         );

                var viewRect = new Rect(remainRect.x + 6f, remainRect.y, remainRect.width - 12f, height);

                rowRect = new Rect(
                                   remainRect.x     + 10f,
                                   remainRect.y     + 3f,
                                   remainRect.width - 20f,
                                   Constants.RowHeight / 2
                                  );

                Widgets.BeginScrollView(remainRect, ref DrawSettings._debugScrollPos, viewRect);
                Text.Font = GameFont.Tiny;

                foreach (Pawn pawn in DrawSettings._allPawns)
                {
                    Widgets.Label(rowRect.LeftPart(0.1f), pawn.Name.ToStringShort);

                    if (pawn.GetComp<Comp_NightVision>() is Comp_NightVision comp)
                    {
                        Rect rightRect = rowRect.RightPart(0.8f);

                        Widgets.Label(
                                      rightRect,
                                      $"Number of eyes: {comp.NumberOfRemainingEyes}/{comp.RaceSightParts.Count}   Natural modifier: {comp.NaturalLightModifiers.IntSetting}"
                                     );

                        rightRect.y += Constants.RowHeight / 2;

                        Widgets.Label(
                                      rightRect,
                                      $"0% light modifier: {comp.ZeroLightModifier}   100% light modifier: {comp.FullLightModifier}"
                                     );

                        rightRect.y += Constants.RowHeight / 2;

                        Widgets.Label(
                                      rightRect,
                                      $"Psych: {comp.PsychDark}      Apparel: NightVis: {comp.ApparelGrantsNV}   Anti-brightness: {comp.ApparelNullsPS}"
                                     );

                        rightRect.y += Constants.RowHeight / 2;

                        Widgets.Label(
                                      rightRect,
                                      $"Health (pre-cap) [0%,100%] : NightV mod: [{comp.NvhediffMods[0]}, {comp.NvhediffMods[1]}]   PhotoS mod: [{comp.PshediffMods[0]}, {comp.PshediffMods[1]}]   Custom: [{comp.HediffMods[0]},{comp.HediffMods[1]}]"
                                     );

                        rightRect.y += Constants.RowHeight / 2;
                        Widgets.Label(rightRect, "Body Parts & their hediffs");

                        foreach (KeyValuePair<string, List<HediffDef>> hedifflist in comp.PawnsNVHediffs
                                    )
                        {
                            rightRect.y += Constants.RowHeight / 2;

                            Widgets.Label(
                                          rightRect,
                                          $"  Body part: {hedifflist.Key} has hediffs: "
                                         );

                            foreach (HediffDef hediff in hedifflist.Value)
                            {
                                if (Storage.HediffLightMods.TryGetValue(
                                                                        hediff,
                                                                        out Hediff_LightModifiers value
                                                                       ))
                                {
                                    rightRect.y += Constants.RowHeight / 2;

                                    Widgets.Label(
                                                  rightRect,
                                                  $"    {hediff.LabelCap}: current setting = {value.IntSetting}"
                                                 );
                                }
                                else
                                {
                                    rightRect.y += Constants.RowHeight / 2;

                                    Widgets.Label(
                                                  rightRect,
                                                  $"    {hediff.LabelCap}: No Setting"
                                                 );
                                }
                            }
                        }

                        rightRect.y += Constants.RowHeight / 2;
                        Widgets.Label(rightRect, "Eye covering or nightvis Apparel:");

                        foreach (Apparel apparel in comp.PawnsNVApparel)
                        {
                            if (Storage.NVApparel.TryGetValue(
                                                              apparel.def,
                                                              out ApparelVisionSetting appSet
                                                             ))
                            {
                                rightRect.y += Constants.RowHeight / 2;

                                Widgets.Label(
                                              rightRect,
                                              $"  {apparel.LabelCap}: nightvis: {appSet.GrantsNV}  anti-bright: {appSet.NullifiesPS}  - def file setting: NV:{appSet.CompGrantsNV} A-B:{appSet.CompNullifiesPS}"
                                             );
                            }
                            else
                            {
                                rightRect.y += Constants.RowHeight / 2;
                                Widgets.Label(rightRect, $"  {apparel.LabelCap}: No Setting");
                            }
                        }

                        rightRect.y += Constants.RowHeight / 2;
                        rowRect.y   =  rightRect.y;
                    }
                    else
                    {
                        Widgets.Label(rowRect.RightHalf(), "No Night Vision component found.");
                    }

                    rowRect.y += Constants.RowHeight / 2;
                    Widgets.DrawLineHorizontal(rowRect.x + 10f, rowRect.y, rowRect.width - 20f);
                    rowRect.y += Constants.RowHeight / 2;
                }

                if (Math.Abs(DrawSettings._maxY) < 0.001)
                {
                    DrawSettings._maxY = rowRect.yMax;
                }

                Widgets.EndScrollView();
            }
        }
    }
}