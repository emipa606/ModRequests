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

        private void _FillTab()
        {
            TakeOffComp comp = SelPawn.GetComp<TakeOffComp>();
            if (comp == null) return;
            lsw.ListRect = new Rect(5f, 25f, this.size.x-5f, this.size.y-25f);
            lsw.BeginWithScroll();
            bool usePeronal=comp.UsePersonalSettings;
            lsw.CheckBoxSized("usePersonalSettings", ref usePeronal);
            comp.UsePersonalSettings = usePeronal;
            if (usePeronal)
            {
                float l1 = 0.03f;
                float l1w = 0.97f;
                float l2 = 0.06f;
                float l2w = 0.94f;
                float l3 = 0.09f;

                TakeOffData data = comp.personalData;
                if (data.indoorLayer == LayerPriority.Null)
                {
                    data.indoorLayer = TakeOffSettings.data.indoorLayer;
                    data.privateRoomLayer = TakeOffSettings.data.privateRoomLayer;
                    data.beltAsLayer = TakeOffSettings.data.beltAsLayer;
                    data.useTakeOffDrafted = TakeOffSettings.data.useTakeOffDrafted;
                    data.hideHatsInBed = TakeOffSettings.data.hideHatsInBed;
                }
                if (data.hotLayer == LayerPriority.Null) //制作時期が違うので一応分ける
                {
                    data.hotLayer = TakeOffSettings.data.hotLayer;
                    data.coldLayer = TakeOffSettings.data.coldLayer;
                    data.useTemp = TakeOffSettings.data.useTemp;
                    data.useTempHotWhileIndoor = TakeOffSettings.data.useTempHotWhileIndoor;
                    data.useTempHotWhileOutdoor = TakeOffSettings.data.useTempHotWhileOutdoor;
                    data.useHotTempDefault = TakeOffSettings.data.useHotTempDefault;
                    data.hotTemp = TakeOffSettings.data.hotTemp;
                    data.useTempColdWhileIndoor = TakeOffSettings.data.useTempColdWhileIndoor;
                    data.useTempColdWhileOutdoor = TakeOffSettings.data.useTempColdWhileOutdoor;
                    data.useColdTempDefault = TakeOffSettings.data.useColdTempDefault;
                    data.coldTemp = TakeOffSettings.data.coldTemp;
                }


                lsw.CheckBoxSized("useTakeOffDrafted", ref data.useTakeOffDrafted);
                if (!TakeOffSettings.LegacyMode)
                {
                    lsw.CheckBoxSized("dontBeNaked", ref data.dontBeNaked, "dontBeNakedTooltip");
                }

                lsw.GapLine();
                lsw.Label("ShowLayerDiscription", "ShowLayerDiscriptionTooltip");
                lsw.LabelDouble("ApparelLayersIndoor", data.indoorLayer.ToString());
                WrapSlider(ref data.indoorLayer, (int)LayerPriority.Naked, (int)LayerPriority.Shell);
                if (TakeOffSettings.usePrivateRoom)
                {
                    lsw.LabelDouble("ApparelLayersPrivateRoom", data.privateRoomLayer.ToString());
                    WrapSlider(ref data.privateRoomLayer, (int)LayerPriority.Naked, (int)LayerPriority.Shell);
                    lsw.useTranslate = false;
                    lsw.CheckBoxSized("TOC.ApplyBarracks".TranslateW("Apply to barracks."), ref data.applyBarracks, null, l1, l1w);
                    lsw.useTranslate = true;
                }
                lsw.LabelDouble("ApparelLayersBeltAs", data.beltAsLayer.ToString(), "ApparelLayersBeltAsTooltip");
                WrapSlider(ref data.beltAsLayer, (int)LayerPriority.Naked, (int)LayerPriority.Shell);

                lsw.useTranslate = false;
                lsw.LabelDouble("TOC.SleepingLayer".TranslateW("Sleeping layer."), (lsw.translateName + data.sleepingLayer.ToString()).Translate(), "TOC.SleepingLayerTooltip".TranslateW("Select the layer to display during sleep. Apparels are usually not visible in the bed.(For some MODs)"));
                lsw.useTranslate = true;
                WrapSlider(ref data.sleepingLayer, (int)LayerPriority.Naked, (int)LayerPriority.Shell);

                lsw.useTranslate = false;
                lsw.LabelSized("TOC.OnlyUseInBed".TranslateW("Use only in bed."), null, l1, l1w);
                lsw.CheckBoxLabeledSized("", ref data.onlyUseInBed, 0.7f, 0.3f);
                lsw.useTranslate = true;

                lsw.GapLine();
                lsw.CheckBoxSized("useTakeOffHatsIndoor", ref data.useTakeOffHatsIndoor);
                lsw.CheckBoxSized("hideHatsInBed", ref data.hideHatsInBed);
                lsw.CheckBoxSized("useHatsAs", ref data.useHatsAsLayer, "useHatsAsTooltip");
                if (data.useHatsAsLayer)
                {
                    lsw.LabelDouble("HatsLayer", data.hatsLayer.ToString(), null, l1);
                    WrapSlider(ref data.hatsLayer, (int)LayerPriority.Naked, (int)LayerPriority.Other, l1, l1w);
                }

                lsw.GapLine();
                lsw.CheckBoxSized("useTemp", ref data.useTemp, "useTempTooltip");
                if (data.useTemp)
                {
                    lsw.CheckBoxSized("UseHotIndoor", ref data.useTempHotWhileIndoor, null, l1, l1w);
                    lsw.CheckBoxSized("UseHotOutdoor", ref data.useTempHotWhileOutdoor, null, l1, l1w);
                    if (data.useTempHotWhileIndoor || data.useTempHotWhileOutdoor)
                    {
                        lsw.LabelDouble("HotLayer", data.hotLayer.ToString(), null, l2);
                        WrapSlider(ref data.hotLayer, (int)LayerPriority.Naked, (int)LayerPriority.Shell, l2, l2w);
                        lsw.CheckBoxSized("UseHotDefault", ref data.useHotTempDefault, null, l2, l2w);
                        if (!data.useHotTempDefault)
                        {
                            float tmpf = data.hotTemp;
                            lsw.InputWithUpDown("HotTemp", ref tmpf, -99, 99, 26, "HotTempTooltip", labelLeftMag: l3, defaultButtonLeftMag: 0.83f, defaultButtonWidth: 0.17f, upDownButtonLeftMag: 0.52f, upDownButtonWidth: 0.08f, inputLeftMag: 0.68f, inputWidthMag: 0.15f);
                            data.hotTemp = Mathf.RoundToInt(tmpf);
                        }
                    }
                    lsw.Gap(10);
                    lsw.CheckBoxSized("UseColdIndoor", ref data.useTempColdWhileIndoor, null, l1, l1w);
                    lsw.CheckBoxSized("UseColdOutdoor", ref data.useTempColdWhileOutdoor, null, l1, l1w);
                    if (data.useTempColdWhileIndoor || data.useTempColdWhileOutdoor)
                    {
                        lsw.LabelDouble("ColdLayer", data.coldLayer.ToString(), null, l2);
                        WrapSlider(ref data.coldLayer, (int)LayerPriority.Naked, (int)LayerPriority.Shell, l2, l2w);
                        lsw.CheckBoxSized("UseColdDefault", ref data.useColdTempDefault, null, l2, l2w);
                        if (!data.useColdTempDefault)
                        {
                            float tmpf = data.coldTemp;
                            lsw.InputWithUpDown("ColdTemp", ref tmpf, -99, 99, 16, "ColdTempTooltip", labelLeftMag: l3, defaultButtonLeftMag: 0.83f, defaultButtonWidth: 0.17f, upDownButtonLeftMag: 0.52f, upDownButtonWidth: 0.08f, inputLeftMag: 0.68f, inputWidthMag: 0.15f);
                            data.coldTemp = Mathf.RoundToInt(tmpf);
                        }
                    }
                }
                lsw.GapLine();


                float u = lsw.CurHeight;
                lsw.Gap(20);
                float h = lsw.CurHeight-u;
                if (Widgets.ButtonText(new Rect(lsw.viewRect.x,u,150,h),lsw.trans("Discard")))
                {
                    comp.UsePersonalSettings = false;
                    comp.personalData = null;
                }

                lsw.Gap();
            }
            lsw.EndWithScroll();
            comp.nowPersonalSettings = comp.CalcSettingToULong();

            if (comp.ChkPersonalSettingsUpdate)
            {
                if (TakeOffSettings.ResolveLegacy3)
                {
                    comp.Update();
                    comp.tempExecute = true;
                }
                else /*if(TakeOffSettings.ResolveLegacy || TakeOffSettings.ResolveLegacy2 || TakeOffSettings.ResolveTest)*/
                {
                    comp.tempExecute = true;
                    comp.UpdateShowLayerLevel();
                    //comp.Pawn.Drawer.renderer.graphics.ResolveApparelGraphics();
                    //comp.Pawn.Drawer.renderer.graphics.ResolveAllGraphics();

                    //comp.Pawn.Drawer.renderer.graphics.SetApparelGraphicsDirty();
                    ////GlobalTextureAtlasManager.TryMarkPawnFrameSetDirty(comp.Pawn);
                    //comp.Pawn.apparel.Notify_ApparelChanged();
                    //PortraitsCache.SetDirty(comp.Pawn);
                    RenderUtil.NotifyApparelChanged(comp);

                }
            }

        }
        private void WrapSlider(ref LayerPriority value, int min, int max, float leftMag = 0f, float widthMag = 1f)
        {
            int tmp = (int)value;
            lsw.Slider(ref tmp, min, max, leftMag: leftMag, widthMag: widthMag);
            value = (LayerPriority)tmp;
        }
    }
}
