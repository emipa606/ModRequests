using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace TakeOffIndoor
{
    public class TakeOffMod : Mod
    {
        ListingStandardWrapper lsw = new ListingStandardWrapper("TakeOffIndoor");

        TakeOffSettings settings;
        public TakeOffMod(ModContentPack content) :base(content)
        {
            settings = GetSettings<TakeOffSettings>();
            Util.mod = this;
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            inRect.y -= 20f;
            inRect.height += 20f;
            lsw.ListRect = new Rect(inRect);

            TextAnchor anc = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(new Rect(inRect.xMax - 320f, inRect.yMin - 20f, 150f, 24f),"ver"+ TakeOffWorldComponent.nowVerdate);
            Text.Anchor = anc;
            if (Widgets.ButtonText(new Rect(inRect.xMax - 170f, inRect.yMin -20f, 150f, 24f), "ChangeLog".TranslateW("Change log.")))
            {
                UpdateNotice.Notice(TakeOffWorldComponent.nowVerdate, 0);
            }

            float l1 = 0.05f;
            float l1w = 0.95f;
            float l2 = 0.1f;
            float l2w = 0.9f;
            float l3 = 0.15f;

            lsw.useTranslate = true;

            lsw.BeginWithScroll();
            lsw.CheckBoxSized("useTakeOff", ref TakeOffSettings.useTakeOff);
            if (TakeOffSettings.useTakeOff)
            {
                lsw.useTranslate = false;
                lsw.CheckBoxSized("TOC.dontUsePersonalSettings".TranslateW("Don't use personal settings."), ref TakeOffSettings.dontUsePersonalSettings, "TOC.dontUsePersonalSettingsTooltip".TranslateW("Don't use personal settings and hide personal settings tab in Pawn."));
                lsw.useTranslate = true;
                TakeOffData data = TakeOffSettings.data;
                if (data == null)
                {
                    debug.WriteLine("data is null");
                }
                lsw.CheckBoxSized("useTakeOffDrafted", ref data.useTakeOffDrafted);
                if (!TakeOffSettings.LegacyMode)
                {
                    lsw.CheckBoxSized("dontBeNaked", ref data.dontBeNaked, "dontBeNakedTooltip");
                }
                lsw.useTranslate = false;
                lsw.Label(" ");
                lsw.CheckBoxLabeledSized("TOC.useInvisible".TranslateW("Use apparel indivisually visible settings."), ref TakeOffSettings.useInvisible, 0, 0.7f, "TOC.useInvisibleTooltip".TranslateW("Select Hide for each apparel, Show only during conscription, Forced show only when not drafted, Forced show."));
                if (TakeOffSettings.useInvisible)
                {
                    if (lsw.ButtonTextSized("TOC.openSelector".TranslateW("Open selector"), 0.75f, 0.25f))
                    {
                        Find.WindowStack.Add(new Dialog_ApaprelSelector());
                    }
                }

                lsw.Label(" ");
                lsw.CheckBoxLabeledSized("TOC.usePreset".TranslateW("Use preset."), ref TakeOffSettings.usePreset, 0, 0.7f, "TOC.usePresetTooltip".TranslateW("Create presets for settings, filter by race or traits for automatic settings."));
                if (TakeOffSettings.usePreset)
                {
                    if (lsw.ButtonTextSized("TOC.openSelector".TranslateW("Open selector"), 0.75f, 0.25f))
                    {
                        Find.WindowStack.Add(new Dialog_Presets());
                    }
                }
                lsw.useTranslate = true;


                lsw.GapLine();
                lsw.Label("ShowLayerDiscription","ShowLayerDiscriptionTooltip");
                lsw.LabelDouble("ApparelLayersIndoor", data.indoorLayer.Translate());
                WrapSlider(ref data.indoorLayer, (int)LayerPriority.Naked, (int)LayerPriority.Shell);

                lsw.useTranslate = false;
                lsw.AllocOneCulumn();
                lsw.CheckBoxLabeledSized("TOC.UsePrivateRoom".TranslateW("Use private room layer."), ref TakeOffSettings.usePrivateRoom,0.8f);
                lsw.useTranslate = true;
                if (TakeOffSettings.usePrivateRoom)
                {
                    lsw.LabelDouble("ApparelLayersPrivateRoom", data.privateRoomLayer.Translate(), null, l1);
                    WrapSlider(ref data.privateRoomLayer, (int)LayerPriority.Naked, (int)LayerPriority.Shell,l1,l1w);


                    lsw.useTranslate = false;
                    lsw.CheckBoxSized("TOC.ApplyBarracks".TranslateW("Apply to barracks."), ref data.applyBarracks, null,l2,l2w);
                    lsw.useTranslate = true;

                    //lsw.InputLabeledSized("RoomCheckInterval", ref TakeOffSettings.roomCheckInterval, 60, 100000, 0, 0.7f, 0.2f);
                    string rci = TakeOffSettings.roomCheckInterval.ToString();
                    lsw.AllocOneCulumn();
                    lsw.LabelSized("RoomCheckInterval", null,l2);
                    lsw.useTranslate = false;
                    lsw.AllocOneCulumn();
                    lsw.RadioButtonLabeledHorizontal(ref rci, "60", 0.1f, 0.15f);
                    lsw.RadioButtonLabeledHorizontal(ref rci, "90", 0.25f,0.15f);
                    lsw.RadioButtonLabeledHorizontal(ref rci, "120", 0.4f, 0.15f);
                    lsw.RadioButtonLabeledHorizontal(ref rci, "180", 0.55f, 0.15f);
                    lsw.RadioButtonLabeledHorizontal(ref rci, "600", 0.7f, 0.15f);
                    lsw.RadioButtonLabeledHorizontal(ref rci, "6000", 0.85f, 0.15f);
                    lsw.useTranslate = true;

                    TakeOffSettings.roomCheckInterval = Int32.Parse(rci);
                    if (TakeOffSettings.roomCheckInterval > 6000)   TakeOffSettings.roomCheckInterval = 6000;


                    lsw.Gap(3);

                    //lsw.AllocOneCulumn();
                    //if (lsw.ButtonTextSized("SetToMax"))
                    //{
                    //    TakeOffSettings.roomCheckInterval = 100000000;
                    //    //TakeOffSettings.updateTicks = 600;
                    //}
                }


                lsw.LabelDouble("ApparelLayersBeltAs", data.beltAsLayer.Translate(), "ApparelLayersBeltAsTooltip");
                WrapSlider(ref data.beltAsLayer, (int)LayerPriority.Naked, (int)LayerPriority.Shell);

                lsw.useTranslate = false;
                lsw.LabelDouble("TOC.SleepingLayer".TranslateW("Sleeping layer."), data.sleepingLayer.Translate(), "TOC.SleepingLayerTooltip".TranslateW("Select the layer to display during sleep. Apparels are usually not visible in the bed."));
                lsw.useTranslate = true;
                WrapSlider(ref data.sleepingLayer, (int)LayerPriority.Naked, (int)LayerPriority.Shell);
                lsw.useTranslate = false;
                lsw.AllocOneCulumn();
                lsw.LabelSized("TOC.OnlyUseInBed".TranslateW("Use only in bed."), null, l1, l1w);
                lsw.Gap();
                lsw.CheckBoxLabeledSized("", ref data.onlyUseInBed,0.7f,0.3f);
                lsw.useTranslate = true;

                lsw.GapLine();
                lsw.CheckBoxSized("hideHatsInBed", ref data.hideHatsInBed);
                lsw.CheckBoxSized("useTakeOffHatsIndoor", ref data.useTakeOffHatsIndoor);
                lsw.CheckBoxSized("useHatsAs", ref data.useHatsAsLayer, "useHatsAsTooltip");
                if (data.useHatsAsLayer)
                {
                    lsw.LabelDouble("HatsLayer", data.hatsLayer.Translate(), null,l1);
                    WrapSlider(ref data.hatsLayer, (int)LayerPriority.Naked, (int)LayerPriority.Other, l1, l1w);
                }
                lsw.GapLine();
                lsw.CheckBoxSized("useTemp", ref data.useTemp, "useTempTooltip");
                if (data.useTemp)
                {
                    lsw.CheckBoxSized("UseHotIndoor", ref data.useTempHotWhileIndoor,null, l1, l1w);
                    lsw.CheckBoxSized("UseHotOutdoor", ref data.useTempHotWhileOutdoor, null, l1, l1w);
                    if (data.useTempHotWhileIndoor || data.useTempHotWhileOutdoor)
                    {
                        lsw.LabelDouble("HotLayer", data.hotLayer.Translate(), null,l2);
                        WrapSlider(ref data.hotLayer, (int)LayerPriority.Naked, (int)LayerPriority.Shell, l2,l2w);
                        lsw.CheckBoxSized("UseHotDefault", ref data.useHotTempDefault,null, l2, l2w);
                        if (!data.useHotTempDefault)
                        {
                            float tmpf = data.hotTemp;
                            lsw.InputWithUpDown("HotTemp", ref tmpf, -99, 99,26, "HotTempTooltip",labelLeftMag: l3, defaultButtonLeftMag: 0.83f, defaultButtonWidth: 0.17f, upDownButtonLeftMag: 0.52f, upDownButtonWidth: 0.08f, inputLeftMag: 0.68f, inputWidthMag: 0.15f);
                            data.hotTemp = Mathf.RoundToInt(tmpf);
                        }
                    }
                    lsw.Gap(10);
                    lsw.CheckBoxSized("UseColdIndoor", ref data.useTempColdWhileIndoor, null, l1, l1w);
                    lsw.CheckBoxSized("UseColdOutdoor", ref data.useTempColdWhileOutdoor, null, l1, l1w);
                    if (data.useTempColdWhileIndoor || data.useTempColdWhileOutdoor)
                    {
                        lsw.LabelDouble("ColdLayer", data.coldLayer.Translate(), null, l2);
                        WrapSlider(ref data.coldLayer, (int)LayerPriority.Naked, (int)LayerPriority.Shell,l2, l2w);
                        lsw.CheckBoxSized("UseColdDefault", ref data.useColdTempDefault,null, l2, l2w);
                        if (!data.useColdTempDefault)
                        {
                            float tmpf = data.coldTemp;
                            lsw.InputWithUpDown("ColdTemp", ref tmpf, -99, 99, 16, "ColdTempTooltip", labelLeftMag: l3, defaultButtonLeftMag: 0.83f, defaultButtonWidth: 0.17f, upDownButtonLeftMag: 0.52f, upDownButtonWidth: 0.08f, inputLeftMag: 0.68f, inputWidthMag: 0.15f);
                            data.coldTemp = Mathf.RoundToInt(tmpf);
                        }
                    }
                }

                if (!TakeOffSettings.LegacyMode)
                {
                    lsw.GapLine();
                    lsw.CheckBoxSized("DontApplyPortrait", ref TakeOffSettings.dontApplyPortrait);
                    lsw.CheckBoxSized("HatsOnlyOnMap", ref TakeOffSettings.hatsOnlyOnMap, "HatsOnlyOnMapTooltip");

                    TakeOffSettings.portraitOption = TakeOffSettings.dontApplyPortrait || TakeOffSettings.hatsOnlyOnMap;
                }

                //listingStandard.GapLine();
                //lsw.CheckBox("UseRemoveAnimal", ref TakeOffSettings.useRemoveAnimal,null,false);
                //lsw.CheckBox("UseRemoveMechanoid", ref TakeOffSettings.useRemoveMech, null, false);

                lsw.GapLine();

                if (RenderUtil.ver == 12)
                {
                    lsw.Label("ResolveGraphicsMode", "ResolveGraphicsModeTooltip");
                    float u = lsw.CurHeight;
                    lsw.Gap(24);
                    float d = lsw.CurHeight;
                    float radwid = lsw.viewRect.width / (debug.deb ? 7 : 6);
                    Rect radrect = new Rect(lsw.ListRect.x, u, radwid, d - u);
                    WrapRadioButton(radrect, ref TakeOffSettings.graphicsMode, ResolveGraphicsMode.Normal);
                    radrect.x += radwid;
                    WrapRadioButton(radrect, ref TakeOffSettings.graphicsMode, ResolveGraphicsMode.AlwaysAll);
                    radrect.x += radwid;
                    WrapRadioButton(radrect, ref TakeOffSettings.graphicsMode, ResolveGraphicsMode.AlwaysApparel);
                    radrect.x += radwid;
                    WrapRadioButton(radrect, ref TakeOffSettings.graphicsMode, ResolveGraphicsMode.Legacy);
                    radrect.x += radwid;
                    WrapRadioButton(radrect, ref TakeOffSettings.graphicsMode, ResolveGraphicsMode.Legacy2);
                    radrect.x += radwid;
                    WrapRadioButton(radrect, ref TakeOffSettings.graphicsMode, ResolveGraphicsMode.Legacy3);
                    if (debug.deb)
                    {
                        radrect.x += radwid;
                        WrapRadioButton(radrect, ref TakeOffSettings.graphicsMode, ResolveGraphicsMode.Test);
                    }

                }
                else
                {
                }

                lsw.CheckBoxSized("DuplicateMessage", ref TakeOffSettings.dontShowDuplicateWaring, "DuplicateMessageTooltip");

                lsw.useTranslate = false;
                if (ModUtil.ExistHAR)
                {
                    lsw.CheckBoxSized("TOC.PatchCanDrawAddons".TranslateW("Patch HAR drawing.(\"reboot required\")"), ref TakeOffSettings.patchCanDrawAddon,
                        tip: "TOC.PatchCanDrawAddonsTooltip".TranslateW("When turned on, fixes an issue where HAR races ears would not display properly when wearing a hat. "));
                }

                lsw.CheckBoxSized("TOC.ShowLatestUpdates".TranslateW("Show latest updates"), ref TakeOffSettings.showLatestUpdates) ;


                lsw.GapLine();

                lsw.useTranslate = true;

                lsw.AllocOneCulumn();
                if (lsw.ButtonTextSized("SetToDefault"))
                {
                    TakeOffSettings.SetToDefault();
                }
            }

            //if (Event.current.type == EventType.Layout)
            //{
            //    scrollViewHeight = lsw.CurHeight + 30f;
            //}

            lsw.EndWithScroll();
            base.DoSettingsWindowContents(inRect);

            //ulong tmp = TakeOffSettings.CalcSettingToULong();
            //if(TakeOffSettings.settingsBK != tmp)
            //{
            //    TakeOffSettings.settingToggle = !TakeOffSettings.settingToggle;
            //    TakeOffSettings.settingsBK = tmp;
            //}

            ulong bk = TakeOffSettings.settingsBK;
            TakeOffSettings.settingsBK = TakeOffSettings.CalcSettingToULong();
            if (bk != TakeOffSettings.settingsBK)
            {
                Harmony_Render.NotifySettingsChanged();
            }

            TakeOffSettings.portraitOption = TakeOffSettings.dontApplyPortrait || TakeOffSettings.hatsOnlyOnMap;
        }

        private void WrapRadioButton(Rect rect,ref ResolveGraphicsMode setting,ResolveGraphicsMode target)
        {
            if (Widgets.RadioButton(rect.x, rect.y-2, setting == target))
            {
                setting = target;
            }
            rect.x += 26;
            rect.width -= 26;
            Widgets.Label(rect, target.ToString());
        }

        public override string SettingsCategory()
        {
            return "Take off coat indoor";
        }

        private void WrapSlider(ref LayerPriority value,int min,int max,float leftMag=0f,float widthMag=1f)
        {
            int tmp = (int)value;
            lsw.Slider(ref tmp, min, max, leftMag:leftMag,widthMag:widthMag) ;
            value = (LayerPriority)tmp;
        }

    }
}