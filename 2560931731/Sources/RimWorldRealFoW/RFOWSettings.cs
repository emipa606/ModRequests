using System;
using System.Collections.Generic;
using RimWorldRealFoW;
using UnityEngine;
using Verse;

namespace RimWorldRealFoW
{
    public class RFOWSettings : ModSettings
    {
        public static Vector2 scrollPosition = Vector2.zero;

        public static void DoSettingsWindowContents(Rect rect)
        {
            //Listing_Standard row = new Listing_Standard(GameFont.Small);
            //row.ColumnWidth = rect.width;
          //  row.Begin(rect);
            rect.yMin += 15f;
            rect.yMax -= 15f;

            var defaultColumnWidth = (rect.width - 50);
            Listing_Standard row = new Listing_Standard() { ColumnWidth = defaultColumnWidth };


            var outRect = new Rect(rect.x, rect.y, rect.width, rect.height);
            var scrollRect = new Rect(0f, 0f, rect.width - 16f, rect.height * 2f);
            Widgets.BeginScrollView(outRect, ref scrollPosition, scrollRect, true);
            
            row.Begin(scrollRect);

            if (row.ButtonTextLabeled("fogAlphaSetting_title".Translate(), ("fogAlphaSetting_" + RFOWSettings.fogAlpha).Translate()))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (object obj in Enum.GetValues(typeof(RFOWSettings.FogAlpha)))
                {
                    RFOWSettings.FogAlpha localValue3 = (RFOWSettings.FogAlpha)obj;
                    RFOWSettings.FogAlpha localValue = localValue3;
                    list.Add(new FloatMenuOption(("fogAlphaSetting_" + localValue).Translate(), delegate ()
                    {
                        RFOWSettings.fogAlpha = localValue;
                        RFOWSettings.ApplySettings();
                    }, MenuOptionPriority.Default, null, null, 0f, null, null));
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }

            Text.Font = GameFont.Tiny;
            row.Label("fogAlphaSetting_desc".Translate(), -1f, null);
            Text.Font = GameFont.Small;

            if (row.ButtonTextLabeled("fogFadeSpeedSetting_title".Translate(), ("fogFadeSpeedSetting_" + RFOWSettings.fogFadeSpeed).Translate()))
            {
                List<FloatMenuOption> list2 = new List<FloatMenuOption>();
                foreach (object obj2 in Enum.GetValues(typeof(RFOWSettings.FogFadeSpeedEnum)))
                {
                    RFOWSettings.FogFadeSpeedEnum localValue2 = (RFOWSettings.FogFadeSpeedEnum)obj2;
                    RFOWSettings.FogFadeSpeedEnum localValue = localValue2;
                    list2.Add(new FloatMenuOption(("fogFadeSpeedSetting_" + localValue).Translate(), delegate ()
                    {
                        RFOWSettings.fogFadeSpeed = localValue;
                        RFOWSettings.ApplySettings();
                    }, MenuOptionPriority.Default, null, null, 0f, null, null));
                }
                Find.WindowStack.Add(new FloatMenu(list2));
            }

            Text.Font = GameFont.Tiny;
            row.Label("fogFadeSpeedSetting_desc".Translate(), -1f, null);
            Text.Font = GameFont.Small;
            AddGap(row);
            row.Label("baseViewRange".Translate() +": " + baseViewRange.ToString(), -1f, "baseViewRange".Translate());
            baseViewRange = (int)row.Slider((float)RFOWSettings.baseViewRange, 10f, 100);
            row.Label("baseHearingRange".Translate()+": " + Math.Round(baseHearingRange, 1).ToString(), -1f, null);
            baseHearingRange = row.Slider(RFOWSettings.baseHearingRange, 0, 30);
            AddGap(row);
            row.Label("buildingVisionMod".Translate() +": " + Math.Round(buildingVisionModifier, 2).ToString(), -1f, "buildingVisionModDesc".Translate());
            buildingVisionModifier = row.Slider(buildingVisionModifier, 0.2f, 2);
            row.Label("animalVisionModDesc".Translate() + ": " + Math.Round(animalVisionModifier, 2).ToString(), -1f, null);
            animalVisionModifier = row.Slider(animalVisionModifier, 0.2f, 2);
            row.Label("turretVisionModDesc".Translate() + ": " + Math.Round(turretVisionModifier, 2).ToString(), -1f, null);
            turretVisionModifier = row.Slider(turretVisionModifier, 0.2f, 2);
            AddGap(row);
            row.CheckboxLabeled("allyGiveVision".Translate(), ref RFOWSettings.allyGiveVision, "allyGiveVision".Translate());
            row.CheckboxLabeled("prisonerGiveVision".Translate(), ref RFOWSettings.prisonerGiveVision, "prisonerGiveVision".Translate());
            row.CheckboxLabeled("mapRevealAtStart".Translate(), ref RFOWSettings.mapRevealAtStart, "mapRevealAtStart".Translate());
          
            row.CheckboxLabeled("wildLifeTabVisible".Translate(), ref RFOWSettings.wildLifeTabVisible, "wildLifeTabVisibleDesc".Translate());
            row.CheckboxLabeled("NeedWatcher".Translate(), ref RFOWSettings.needWatcher, "NeedWatcherDesc".Translate());
       
            AddGap(row);
            row.CheckboxLabeled("hideEventNegative".Translate(), ref RFOWSettings.hideEventNegative, "hideEventNegative".Translate());
            row.CheckboxLabeled("hideEventNeutral".Translate(), ref RFOWSettings.hideEventNeutral, "hideEventNeutral".Translate());
            row.CheckboxLabeled("hideEventPositive".Translate(), ref RFOWSettings.hideEventPositive, "hideEventPositive".Translate());     
            row.CheckboxLabeled("hideThreatBig".Translate(), ref RFOWSettings.hideThreatBig, "hideThreatBig".Translate());
            row.CheckboxLabeled("hideThreatSmall".Translate(), ref RFOWSettings.hideThreatSmall, "hideThreatSmall".Translate());
            AddGap(row);
            row.CheckboxLabeled("censorMode".Translate(), ref RFOWSettings.censorMode, "censorMode".Translate());
            row.CheckboxLabeled("hideSpeakBubble".Translate(), ref RFOWSettings.hideSpeakBubble, "hideSpeakBubbleDesc".Translate());
            row.CheckboxLabeled("aiSmart".Translate(), ref RFOWSettings.aiSmart, "aiSmartDesc".Translate());


            row.End();
            Widgets.EndScrollView();
        }
        public static void AddGap(Listing_Standard listing_Standard, float value = 12f)
        {
            listing_Standard.Gap(value);
            listing_Standard.GapLine(value);
        }
        public static void ApplySettings()
        {
            SectionLayer_FoVLayer.prefFadeSpeedMult = (int)RFOWSettings.fogFadeSpeed;
            SectionLayer_FoVLayer.prefEnableFade = (RFOWSettings.fogFadeSpeed != RFOWSettings.FogFadeSpeedEnum.Disabled);
            SectionLayer_FoVLayer.prefFogAlpha = (byte)RFOWSettings.fogAlpha;
            if (Current.ProgramState == ProgramState.Playing)
            {
                foreach (Map map in Find.Maps)
                {
                    if (map.mapDrawer != null)
                    {
                        map.mapDrawer.RegenerateEverythingNow();
                    }
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<RFOWSettings.FogFadeSpeedEnum>(ref RFOWSettings.fogFadeSpeed, "fogFadeSpeed", RFOWSettings.FogFadeSpeedEnum.Medium, false);
            Scribe_Values.Look<RFOWSettings.FogAlpha>(ref RFOWSettings.fogAlpha, "fogAlpha", RFOWSettings.FogAlpha.Medium, false);
            Scribe_Values.Look<int>(ref RFOWSettings.baseViewRange, "baseViewRange", 60, false);
            Scribe_Values.Look<float>(ref RFOWSettings.buildingVisionModifier, "buildingVisionMod", 1, false);
            Scribe_Values.Look<float>(ref RFOWSettings.animalVisionModifier, "animalVisionMod", 0.5f, false);
            Scribe_Values.Look<float>(ref RFOWSettings.turretVisionModifier, "turretVisionMod", 0.7f, false);
            Scribe_Values.Look<float>(ref RFOWSettings.baseHearingRange, "baseHearingRange", 10, false);
            Scribe_Values.Look<bool>(ref RFOWSettings.wildLifeTabVisible, "wildLifeTabVisible", true, false);
            Scribe_Values.Look<bool>(ref RFOWSettings.prisonerGiveVision, "prisonerGiveVision", false, false);
            Scribe_Values.Look<bool>(ref RFOWSettings.mapRevealAtStart, "mapRevealAtStart", false, false);
            Scribe_Values.Look<bool>(ref RFOWSettings.allyGiveVision, "allyGiveVision", false, false);
            Scribe_Values.Look<bool>(ref RFOWSettings.needWatcher, "needWatcher", true, false);
            Scribe_Values.Look<bool>(ref RFOWSettings.needMemoryStorage, "needMemoryStorage", true, false);
            Scribe_Values.Look<bool>(ref RFOWSettings.hideEventNegative, "hideEventNegative", false, false);
            Scribe_Values.Look<bool>(ref RFOWSettings.hideEventNeutral, "hideEventNeutral", false, false);
            Scribe_Values.Look<bool>(ref RFOWSettings.hideEventPositive, "hideEventPositive", false, false);
            Scribe_Values.Look<bool>(ref RFOWSettings.hideThreatBig, "hideThreatBig", false, false);
            Scribe_Values.Look<bool>(ref RFOWSettings.hideThreatSmall, "hideThreatSmall", false, false);
            Scribe_Values.Look<bool>(ref RFOWSettings.censorMode, "censorMode", false, false);
            Scribe_Values.Look<bool>(ref RFOWSettings.hideSpeakBubble, "hideSpeakBubble", false, false);
            Scribe_Values.Look<bool>(ref RFOWSettings.aiSmart, "aiSmart", false, false);

            RFOWSettings.ApplySettings();
        }

        public static RFOWSettings.FogFadeSpeedEnum fogFadeSpeed = RFOWSettings.FogFadeSpeedEnum.Medium;

        public static RFOWSettings.FogAlpha fogAlpha = RFOWSettings.FogAlpha.Medium;

        public static int baseViewRange = 60;
        public static float baseHearingRange = 10;

        public static float buildingVisionModifier = 1;

        public static float turretVisionModifier = 0.7f;

        public static float animalVisionModifier = 0.5f;

        public static bool hideSpeakBubble = false;
    
        public static bool aiSmart = false;
        public static bool censorMode = false;
        public static bool needWatcher = true;
        public static bool hideThreatBig = false;
        public static bool hideThreatSmall = false;
        public static bool hideEventPositive = false;
        public static bool hideEventNegative = false;
        public static bool hideEventNeutral = false;
        public static bool prisonerGiveVision = false;
        public static bool allyGiveVision = false;
        public static bool mapRevealAtStart = false;
        public static bool wildLifeTabVisible = true;
        public static bool needMemoryStorage = true;
        public enum FogFadeSpeedEnum
        {
            Slow = 5,
            Medium = 20,
            Fast = 40,
            Disabled = 100
        }

        // Token: 0x0200002D RID: 45
        public enum FogAlpha
        {
            Black = 255,
            NearlyBlack = 210,
            VeryVeryVeryDark = 180,
            VeryVeryDark = 150,
            VeryDark = 120,
            Dark = 100,
            Medium = 80,
            Light = 60
        }
    }
}
