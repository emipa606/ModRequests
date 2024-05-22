using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using RimWorld;
using Verse.Sound;
using AlienRace;

namespace TakeOffIndoor
{
    public class ITab_Pawn_TakeOffIndoor : ITab
    {
        ListingStandardWrapper lsw=new ListingStandardWrapper("TakeOffIndoor");

        public ITab_Pawn_TakeOffIndoor()
        {
            this.size = new Vector2(460f, 450f);
            this.labelKey = "TakeOffIndoor.TabTakeOffIndoor";
            this.tutorTag = "TakeOffIndoor";
        }

        private TakeOffComp TakeOffComp
        {
            get
            {
                if(base.SelPawn == null)
                {
                    return null;
                }
                Corpse corpse = base.SelThing as Corpse;
                if (corpse != null)
                {
                    return null;
                }
                return base.SelPawn.GetComp<TakeOffComp>();
            }
        }
        public override bool IsVisible
        {
            get
            {
                if (TakeOffSettings.DontUsePersonalSettings)
                {
                    return false;
                }

                Corpse corpse = base.SelThing as Corpse;
                if (corpse != null)
                {
                    return false;
                }
                if (base.SelPawn == null)
                {
                    return false;
                }
                Pawn pawn = base.SelPawn;

                return (pawn.RaceProps.ToolUser || pawn.inventory.innerContainer.Count > 0) && (TakeOffComp != null) && pawn.IsColonist;
            }
        }
        protected override void FillTab()
        {
            _FillTab();
        }

        public static void FillingPersonalSettings(ListingStandardWrapper lsw,ref TakeOffData nowData)
        {
            float l1 = 0.03f;
            float l1w = 0.97f;
            float l2 = 0.06f;
            float l2w = 0.94f;
            float l3 = 0.09f;

            if (nowData.indoorLayer == LayerPriority.Null)
            {
                nowData.indoorLayer = TakeOffSettings.data.indoorLayer;
                nowData.privateRoomLayer = TakeOffSettings.data.privateRoomLayer;
                nowData.beltAsLayer = TakeOffSettings.data.beltAsLayer;
                nowData.useTakeOffDrafted = TakeOffSettings.data.useTakeOffDrafted;
                nowData.hideHatsInBed = TakeOffSettings.data.hideHatsInBed;
            }
            if (nowData.hotLayer == LayerPriority.Null) //制作時期が違うので一応分ける
            {
                nowData.hotLayer = TakeOffSettings.data.hotLayer;
                nowData.coldLayer = TakeOffSettings.data.coldLayer;
                nowData.useTemp = TakeOffSettings.data.useTemp;
                nowData.useTempHotWhileIndoor = TakeOffSettings.data.useTempHotWhileIndoor;
                nowData.useTempHotWhileOutdoor = TakeOffSettings.data.useTempHotWhileOutdoor;
                nowData.useHotTempDefault = TakeOffSettings.data.useHotTempDefault;
                nowData.hotTemp = TakeOffSettings.data.hotTemp;
                nowData.useTempColdWhileIndoor = TakeOffSettings.data.useTempColdWhileIndoor;
                nowData.useTempColdWhileOutdoor = TakeOffSettings.data.useTempColdWhileOutdoor;
                nowData.useColdTempDefault = TakeOffSettings.data.useColdTempDefault;
                nowData.coldTemp = TakeOffSettings.data.coldTemp;
            }


            lsw.CheckBoxSized("useTakeOffDrafted", ref nowData.useTakeOffDrafted);
            if (!TakeOffSettings.LegacyMode)
            {
                lsw.CheckBoxSized("dontBeNaked", ref nowData.dontBeNaked, "dontBeNakedTooltip");
            }

            lsw.GapLine();
            lsw.Label("ShowLayerDiscription", "ShowLayerDiscriptionTooltip");
            lsw.LabelDouble("ApparelLayersIndoor", nowData.indoorLayer.Translate());
            WrapSlider(lsw,ref nowData.indoorLayer, (int)LayerPriority.Naked, (int)LayerPriority.Shell);
            if (TakeOffSettings.usePrivateRoom)
            {
                lsw.LabelDouble("ApparelLayersPrivateRoom", nowData.privateRoomLayer.Translate());
                WrapSlider(lsw,ref nowData.privateRoomLayer, (int)LayerPriority.Naked, (int)LayerPriority.Shell);
                lsw.useTranslate = false;
                lsw.CheckBoxSized("TOC.ApplyBarracks".TranslateW("Apply to barracks."), ref nowData.applyBarracks, null, l1, l1w);
                lsw.useTranslate = true;
            }
            lsw.LabelDouble("ApparelLayersBeltAs", nowData.beltAsLayer.Translate(), "ApparelLayersBeltAsTooltip");
            WrapSlider(lsw, ref nowData.beltAsLayer, (int)LayerPriority.Naked, (int)LayerPriority.Shell);

            lsw.useTranslate = false;
            lsw.LabelDouble("TOC.SleepingLayer".TranslateW("Sleeping layer."), nowData.sleepingLayer.Translate(), "TOC.SleepingLayerTooltip".TranslateW("Select the layer to display during sleep. Apparels are usually not visible in the bed.(For some MODs)"));
            lsw.useTranslate = true;
            WrapSlider(lsw, ref nowData.sleepingLayer, (int)LayerPriority.Naked, (int)LayerPriority.Shell);

            lsw.useTranslate = false;
            lsw.AllocOneCulumn();
            lsw.LabelSized("TOC.OnlyUseInBed".TranslateW("Use only in bed."), null, l1, l1w);
            lsw.CheckBoxLabeledSized("", ref nowData.onlyUseInBed, 0.7f, 0.3f);
            lsw.useTranslate = true;

            lsw.GapLine();
            lsw.CheckBoxSized("hideHatsInBed", ref nowData.hideHatsInBed);
            lsw.CheckBoxSized("useTakeOffHatsIndoor", ref nowData.useTakeOffHatsIndoor);
            lsw.CheckBoxSized("useHatsAs", ref nowData.useHatsAsLayer, "useHatsAsTooltip");
            if (nowData.useHatsAsLayer)
            {
                lsw.LabelDouble("HatsLayer", nowData.hatsLayer.ToString(), null, l1);
                WrapSlider(lsw, ref nowData.hatsLayer, (int)LayerPriority.Naked, (int)LayerPriority.Other, l1, l1w);
            }

            lsw.GapLine();
            lsw.CheckBoxSized("useTemp", ref nowData.useTemp, "useTempTooltip");
            if (nowData.useTemp)
            {
                lsw.CheckBoxSized("UseHotIndoor", ref nowData.useTempHotWhileIndoor, null, l1, l1w);
                lsw.CheckBoxSized("UseHotOutdoor", ref nowData.useTempHotWhileOutdoor, null, l1, l1w);
                if (nowData.useTempHotWhileIndoor || nowData.useTempHotWhileOutdoor)
                {
                    lsw.LabelDouble("HotLayer", nowData.hotLayer.Translate(), null, l2);
                    WrapSlider(lsw, ref nowData.hotLayer, (int)LayerPriority.Naked, (int)LayerPriority.Shell, l2, l2w);
                    lsw.CheckBoxSized("UseHotDefault", ref nowData.useHotTempDefault, null, l2, l2w);
                    if (!nowData.useHotTempDefault)
                    {
                        float tmpf = nowData.hotTemp;
                        lsw.InputWithUpDown("HotTemp", ref tmpf, -99, 99, 26, "HotTempTooltip", labelLeftMag: l3, defaultButtonLeftMag: 0.83f, defaultButtonWidth: 0.17f, upDownButtonLeftMag: 0.52f, upDownButtonWidth: 0.08f, inputLeftMag: 0.68f, inputWidthMag: 0.15f);
                        nowData.hotTemp = Mathf.RoundToInt(tmpf);
                    }
                }
                lsw.Gap(10);
                lsw.CheckBoxSized("UseColdIndoor", ref nowData.useTempColdWhileIndoor, null, l1, l1w);
                lsw.CheckBoxSized("UseColdOutdoor", ref nowData.useTempColdWhileOutdoor, null, l1, l1w);
                if (nowData.useTempColdWhileIndoor || nowData.useTempColdWhileOutdoor)
                {
                    lsw.LabelDouble("ColdLayer", nowData.coldLayer.Translate(), null, l2);
                    WrapSlider(lsw, ref nowData.coldLayer, (int)LayerPriority.Naked, (int)LayerPriority.Shell, l2, l2w);
                    lsw.CheckBoxSized("UseColdDefault", ref nowData.useColdTempDefault, null, l2, l2w);
                    if (!nowData.useColdTempDefault)
                    {
                        float tmpf = nowData.coldTemp;
                        lsw.InputWithUpDown("ColdTemp", ref tmpf, -99, 99, 16, "ColdTempTooltip", labelLeftMag: l3, defaultButtonLeftMag: 0.83f, defaultButtonWidth: 0.17f, upDownButtonLeftMag: 0.52f, upDownButtonWidth: 0.08f, inputLeftMag: 0.68f, inputWidthMag: 0.15f);
                        nowData.coldTemp = Mathf.RoundToInt(tmpf);
                    }
                }
            }
        }

        static FloatMenuWrapper.FloatMenuW fmwPreset = new FloatMenuWrapper.FloatMenuW();

        void SetPresetCommand(int num)
        {
            TakeOffComp comp = SelPawn.GetComp<TakeOffComp>();
            if (num >= 0)
            {
                comp.targetPresetOverride = TakeOffSettings.presets[num].name;
            }
            else
            {
                comp.targetPresetOverride = "None";
            }

            comp.presetEvaluated = false;
            //comp.nowPreset = TOCPreset.GetName(comp.targetPresetOverride);
        }
        private void _FillTab()
        {
            TakeOffComp comp = SelPawn.GetComp<TakeOffComp>();
            if (comp == null) return;
            lsw.ListRect = new Rect(5f, 12f, this.size.x - 10f, this.size.y - 14f);
            lsw.BeginWithScroll();

            bool usePeronal=comp.UsePersonalSettings;


            bool presetFlg = false;

            if (TakeOffSettings.usePreset && usePeronal == false)
            {
                if (comp.nowPreset != null)
                {
                    lsw.Label(("TOC.NowPreset").TranslateW("Now preset") + ":");
                    if (lsw.ButtonTextSized(comp.nowPreset.name, 0.3f, 0.5f))
                    {
                        if (TakeOffSettings.presets.Count > 0)
                        {
                            fmwPreset.ResetList();
                            fmwPreset.AddMenu("None", -1);
                            for (int i = 0; i < TakeOffSettings.presets.Count; i++)
                            {
                                fmwPreset.AddMenu(TakeOffSettings.presets[i].name, i);
                            }
                            fmwPreset.SetAction(SetPresetCommand);
                            fmwPreset.Show();
                        }
                    }
                    if (lsw.ButtonTextSized("Reset".Translate(), 0.805f, 0.195f))
                    {
                        comp.targetPresetOverride = "";
                        //comp.nowPreset = TOCPreset.GetMatchData(SelPawn);
                        comp.presetEvaluated = false;
                    }
                    lsw.Tooltip("TOC.PresetButtonTooltip".TranslateW(@"The preset is automatically selected by the filter. \nTo specify manually, select from the button. \nPress Reset to return to automatic selection. \nIf select other than ""None"", the settings of the currently selected preset will be displayed."));

                }
                presetFlg = true;
            }
            else
            {
                lsw.Gap();
            }

            lsw.CheckBoxSized("usePersonalSettings", ref usePeronal);
            comp.UsePersonalSettings = usePeronal;


            lsw.GapLine();
            //if (usePeronal)
            //{

            FillingPersonalSettings(lsw,ref comp.data);


            if (usePeronal)
            {
                lsw.GapLine();
                lsw.AllocOneCulumn();

                if (lsw.ButtonTextSized(lsw.trans("Discard"),0,0.3f))
                {
                    comp.UsePersonalSettings = false;
                    comp.personalData = null;
                }
            }
            //}
            lsw.EndWithScroll();
            comp.nowPersonalSettings = comp.CalcSettingToULong();

            if (comp.ChkPersonalSettingsUpdate)
            {
                //if (TakeOffSettings.ResolveLegacy3)
                //{
                //    comp.Update();
                //    comp.tempExecute = true;
                //}
                //else /*if(TakeOffSettings.ResolveLegacy || TakeOffSettings.ResolveLegacy2 || TakeOffSettings.ResolveTest)*/
                //{
                    comp.tempExecute = true;
                    comp.UpdateShowLayerLevel();
                    //comp.Pawn.Drawer.renderer.graphics.ResolveApparelGraphics();
                    //comp.Pawn.Drawer.renderer.graphics.ResolveAllGraphics();

                    //comp.Pawn.Drawer.renderer.graphics.SetApparelGraphicsDirty();
                    ////GlobalTextureAtlasManager.TryMarkPawnFrameSetDirty(comp.Pawn);
                    //comp.Pawn.apparel.Notify_ApparelChanged();
                    //PortraitsCache.SetDirty(comp.Pawn);
                    RenderUtil.NotifyApparelChanged(comp);

                    if (presetFlg)
                    {
                        TakeOffSettings.Save();
                    }
                //}
            }

        }
        private static void WrapSlider(ListingStandardWrapper lsw, ref LayerPriority value, int min, int max, float leftMag = 0f, float widthMag = 1f)
        {
            int tmp = (int)value;
            lsw.Slider(ref tmp, min, max, leftMag: leftMag, widthMag: widthMag);
            value = (LayerPriority)tmp;
        }
    }
}
