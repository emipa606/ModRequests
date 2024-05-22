using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using FloatMenuWrapper;
using AlienRace;
using System.Text.RegularExpressions;

namespace TakeOffIndoor
{
    public class Dialog_Presets: Window
    {
        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(680, 600);
            }
        }

        public Dialog_Presets()
        {
            this.doCloseX = true;
            this.onlyOneOfTypeAllowed = true;
            this.absorbInputAroundWindow = true;
            this.closeOnCancel = true;
            this.doCloseButton = true;
        }

        static ListingStandardWrapper ww = new ListingStandardWrapper("TakeOffIndoor");

        const float IconSize = 50;
        const float IconGap = 54;
        const float Dif = 2;
        const int xMax = 8;

        static Vector2 offset = new Vector2(0, 0);

        static bool first = true;

        static FloatMenuW fmwPresets = new FloatMenuW();

        static TOCPreset nowPreset;

        public override void DoWindowContents(Rect inRect)
        {

            if (first)
            {
                first = false;
                if (TakeOffSettings.presets == null)
                {
                    TakeOffSettings.presets = new List<TOCPreset>();
                }
                else
                {
                    if (TakeOffSettings.presets.Count > 0)
                    {
                        nowPreset = TakeOffSettings.presets[0];
                    }
                }
                if (nowPreset == null)
                {
                    nowPreset = new TOCPreset();
                    TakeOffSettings.presets.Add(nowPreset);
                }

            }

            DrawDiscription(new Rect(0, 0, 24, 24));
            //設定名＆設定選択
            TakeOffData nowData = nowPreset.data;

            Widgets.Label(new Rect(0,24,150,24), "TOC.PresetName".TranslateW("Preset name") + ":");
            nowPreset.name = Widgets.TextField(new Rect(125, 24, inRect.width - 200, 24), nowPreset.name);

            if (Widgets.ButtonText(new Rect(inRect.width-24, 24, 24, 24), "▼"))
            {
                fmwPresets.ResetList();
                fmwPresets.AddMenu("New".Translate(), -1);
                int cnt = 0;
                foreach(TOCPreset set in TakeOffSettings.presets)
                {
                    fmwPresets.AddMenu(set.name, cnt++);
                }
                fmwPresets.SetAction(PresetCommand);
                fmwPresets.Show();
            }

            inRect.y -= 20f;
            inRect.height += 20f;
            ww.ListRect = new Rect(inRect.x, inRect.y + 44f, inRect.width, inRect.height - 98f);
            ww.canvasRect.height += 10f;
            ww.BeginWithScroll();

            //フィルター
            ww.ListSeparater("TOC.Filters".TranslateW("Filters"));
            ww.Gap(6);
            if (nowPreset.filters.Count == 0)
            {
                nowPreset.filters.Add(new RuleFilter(RuleType.Trait, ""));
            }

            for (int i=0;i< nowPreset.filters.Count;i++)
            {
                RuleFilter filter = nowPreset.filters[i];

                ww.AllocOneCulumn();
                {
                    string tmpLogic = filter.LogicString;
                    ww.RadioButtonLabeledHorizontal(ref tmpLogic,"And",0,0.1f);
                    ww.RadioButtonLabeledHorizontal(ref tmpLogic, "Or", 0.1f, 0.8f);
                    ww.RadioButtonLabeledHorizontal(ref tmpLogic, "Not", 0.18f, 0.1f);
                    filter.LogicString = tmpLogic;
                }

                ww.BackupGUI();
                Text.Anchor = TextAnchor.MiddleRight;
                ww.LabelSized("TOC.Type".TranslateW("Value type") + ":", null, 0.3f, 0.1f);
                ww.RecoverGUI();
                if (ww.ButtonTextSized(filter.TypeName.Translate(), 0.4f, 0.1f))
                {
                    filter.ShowTypeMenu();
                }
                Text.Anchor = TextAnchor.MiddleRight;
                ww.LabelSized("Value".Translate() + ":", null, 0.52f, 0.08f);
                ww.RecoverGUI();
                if (ww.ButtonTextSized(filter.Label, 0.6f, 0.245f))
                {
                    filter.ShowValueMenu();
                }

                if (i > 0)
                {
                    if (ww.ButtonTextSized("△", 0.85f, 0.045f))
                    {
                        RuleFilter tmp = nowPreset.filters[i];
                        nowPreset.filters[i]= nowPreset.filters[i-1];
                        nowPreset.filters[i - 1] = tmp;
                    }
                }
                if(i< nowPreset.filters.Count-1)
                {
                    if (ww.ButtonTextSized("▽", 0.9f, 0.045f))
                    {
                        RuleFilter tmp = nowPreset.filters[i];
                        nowPreset.filters[i] = nowPreset.filters[i + 1];
                        nowPreset.filters[i + 1] = tmp;
                    }
                }

                if (ww.ButtonTextSized("－", 0.95f, 0.045f))
                {
                    nowPreset.filters.RemoveAt(i);
                }
            }
            ww.AllocOneCulumn();
            if(ww.ButtonTextSized("＋", 0f, 0.04f))
            {
                nowPreset.filters.Add(new RuleFilter(RuleType.Trait,""));
            }

            ww.GapLine();


            //個別の設定
            ITab_Pawn_TakeOffIndoor.FillingPersonalSettings(ww, ref nowData);

            ww.Gap(10);
            ww.EndWithScroll();

            TakeOffSettings.presetUpdateCount++; //とりあえずこれで更新を取得
        }

        private void DrawDiscription(Rect rect)
        {
            if (ListingStandardWrapper.TooltipS(rect, "TOC.presetInfomation".TranslateW(@"Create personalized presets.\nYou can select a preset from the Pawn tab window.\nIf you have set a filter, it will automatically be set to the Pawn that matches the filters.\nFilters logic are processed from top to bottom. (Example: From the top ""And A,Or B,And C,Not D"" is processed as ""(((A or B) And C) Nand D)"").")))
            {
                Widgets.DrawBoxSolid(rect, Color.gray);
            }

            Widgets.DrawTextureFitted(rect, TakeOffMain.infoTex, 1f);
        }

        void PresetCommand(int num)
        {
            if (num < 0)
            {
                TOCPreset np = new TOCPreset();
                TakeOffSettings.presets.Add(np);
                nowPreset = np;
            }
            else
            {
                nowPreset = TakeOffSettings.presets[num];
            }
        }

        public override void PostClose()
        {
            base.PostClose();
            UI.UnfocusCurrentControl();
        }
    }
}
