using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using FloatMenuWrapper;

namespace TakeOffIndoor
{
    public class Dialog_IconSelector : Window
    {

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(480, 400);
            }
        }

        //public override bool IsDebug
        //{
        //    get
        //    {
        //        return true;
        //    }
        //}

        public Dialog_IconSelector()
        {
            this.doCloseX = true;
            this.onlyOneOfTypeAllowed = true;
            this.absorbInputAroundWindow = true;
            this.closeOnCancel = true;

            foreach (ThingDef apThing in DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.IsApparel))
            {
                if (RPGSlotMain.CheckApparel(targetSD.raceDef, apThing, targetSD.tree.gender))
                {
                    if (targetRec == targetSD.GetSlotDataRecord(apThing))
                    {
                        apThingList.Add(apThing);
                        SlotDataRecord.MakeIcon(apThing, out Texture iconTex, out Texture iconInvalidTex);
                        iconTexList.Add(iconTex);
                        invalidTexList.Add(iconInvalidTex);
                        originalTexList.Add(apThing.graphic.MatAt(apThing.defaultPlacingRot, null).mainTexture);
                    }
                }
            }
        }

        List<ThingDef> apThingList = new List<ThingDef>();
        List<Texture> iconTexList = new List<Texture>();
        List<Texture> invalidTexList = new List<Texture>();
        List<Texture> originalTexList = new List<Texture>();

        static WidgetsWrapper ww = new WidgetsWrapper("");

        const float IconSize = 50;
        const float IconGap = 54;
        const float Dif = 2;
        const int xMax = 8;

        static Vector2 offset = new Vector2(0, 0);

        public static bool applyAllRaces = false;

        public override void DoWindowContents(Rect inRect)
        {
            TextAnchor anc = Text.Anchor;
            GameFont font = Text.Font;

            //Text.Font = GameFont.Tiny;
            //Widgets.Label(new Rect(0, 0, 200, 20), "IconSelector".TranslateW("Icon Selector."));
            //Text.Font = font;

            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(new Rect(inRect.width - 166, 0, 124, 24), "ShowOriginalColor".TranslateW("Show original color"));
            Widgets.Checkbox(inRect.width - 40, 0, ref Util.showOriginalColor);

            Widgets.Label(new Rect(inRect.width - 336, 0, 124, 24), "ApplyAllRaces".TranslateW("Apply all races"));
            Widgets.Checkbox(inRect.width - 210, 0, ref applyAllRaces);

            Text.Anchor = anc;

            ww.ListRect = new Rect(inRect.x,inRect.y+15f,inRect.width,inRect.height-15f);

            ww.BeginWithScroll();
            int x = 0,y = 0;

            List<Texture> usingList;
            if (Util.showOriginalColor)
            {
                usingList = originalTexList;
            }
            else
            {
                usingList = iconTexList;
            }

            for (int i=0;i<usingList.Count;i++)
            {
                string tt = apThingList[i].label;
                if (Util.ShowDefName)
                {
                    tt += "\n" + apThingList[i].defName;
                }
                ww.Tooltip(DrawTex(usingList[i], x, y), tt);
                x++;
                if (x == xMax)
                {
                    x = 0;y++;
                }
            }

            x--;
            if (x < 0)
            {
                x = xMax - 1;
                y--;
            }

            ArrayPos? mousePos = GetMousePosition(xMax, y+1);

            if (mousePos != null)
            {
                ArrayPos pos = (ArrayPos)mousePos;
                if(!((pos.y == y )&& (pos.x > x))) 
                {
                    DrawFrame((ArrayPos)mousePos, Color.white);
                    if (Util.GetLeftClick())
                    {
                        int num = pos.y * xMax + pos.x;

                        ApplyChange(num);

                        this.Close();
                    }
                }
            }

            ww.EndWithScroll();
        }

        private Rect DrawTex(Texture tex,int x,int y)
        {
            Rect rect = new Rect(x * IconGap + offset.x, y * IconGap + offset.y, IconSize, IconSize);
            //Widgets.DrawTextureFitted(rect, tex, 1f);
            ww.DrawTexture(rect, tex);
            return rect;
        }

        public void DrawFrame(SlotDataRecord record, Color color)
        {
            DrawFrame(record.ArrayPos, color);
        }

        public void DrawFrame(ArrayPos pos, Color color)
        {
            Color tmpc = GUI.color;
            GUI.color = color;

            Rect rect = new Rect(pos.x * IconGap + offset.x, pos.y * IconGap + offset.y, IconSize, IconSize);

            Widgets.DrawBox(rect, 2);
            GUI.color = tmpc;
        }


        public ArrayPos? GetMousePosition(int xmax, int ymax)
        {
            for (int x = 0; x < xmax; x++)
            {
                for (int y = 0; y < ymax; y++)
                {
                    if (Mouse.IsOver(new Rect(x * IconGap + offset.x - Dif, y * IconGap + offset.y - Dif, IconGap, IconGap)))
                    {
                        return new ArrayPos(x, y);
                    }
                }
            }
            return null;
        }

        void ApplyChange(int num)
        {
            if (applyAllRaces)
            {
                ApplyAllRaces(num);
            }
            else
            {
                ApplySingle(targetRec, num);
                if (Dialog_SlotPositionAdjuster.applyOtherGender)
                {
                    SlotData sd = SlotData.GetSlotData(targetRec.parent.raceDef, Util.ReverseGender(targetRec.parent.tree.gender), false);
                    if (sd != null)
                    {
                        if(RPGSlotMain.CheckApparel(sd.raceDef, apThingList[num],sd.tree.gender)
                            && sd.apparelGroupHashToSlotDataDic.ContainsKey(targetRec.node.group.hashKey))
                        {
                            ApplySingle(sd.apparelGroupHashToSlotDataDic[targetRec.node.group.hashKey], num);
                        }
                    }
                }
            }
        }

        void ApplySingle(SlotDataRecord rec, int num)
        {
            //debug.WriteLine("applysingle");
            rec.exampleThing = apThingList[num];
            rec.iconTex = iconTexList[num];
            rec.iconInvalidTex = invalidTexList[num];
            IconOverwrite.ChkAndUpdateOverwrite(rec);
        }
        void ApplyAllRaces(int num)
        {
            //debug.WriteLine("applyall");
            ThingDef ap = apThingList[num];
            foreach (SlotData sd in SlotData.raceSlotDic.Values)
            {
                //debug.WriteLine("aprace:"+sd.raceDef.defName);
                string key = ApparelGroupTree.apparelToApparelGroupDic[ap.defName].hashKey;
                if (RPGSlotMain.CheckApparel(sd.raceDef, ap, sd.tree.gender) //着れるか
                   && sd.apparelGroupHashToSlotDataDic.ContainsKey(key)) //キーが有るか 
                {
                    ApplySingle(sd.apparelGroupHashToSlotDataDic[key], num);
                }                
            }
        }
        public override void PostClose()
        {
            base.PostClose();
            UI.UnfocusCurrentControl();
        }

    }
}
