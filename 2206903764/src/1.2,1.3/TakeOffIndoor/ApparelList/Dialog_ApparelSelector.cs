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
    public struct ArrayPos //配列番号としての座標用
    {
        public int x;
        public int y;

        public ArrayPos(int _x, int _y)
        {
            x = _x;
            y = _y;
        }
        public ArrayPos(Vector2Int vec)
        {
            x = vec.x;
            y = vec.y;
        }

        public static ArrayPos operator +(ArrayPos posA, ArrayPos posB)
        {
            return new ArrayPos(posA.x + posB.x, posA.y + posB.y);
        }
        public static ArrayPos operator -(ArrayPos posA, ArrayPos posB)
        {
            return new ArrayPos(posA.x - posB.x, posA.y - posB.y);
        }
        public static ArrayPos operator *(ArrayPos pos, int num)
        {
            return new ArrayPos(pos.x * num, pos.y * num);
        }
        public static ArrayPos operator /(ArrayPos pos, int num)
        {
            return new ArrayPos(pos.x / num, pos.y / num);
        }

        public override string ToString()
        {
            return "(" + x + "," + y + ")";
        }

        public override bool Equals(object obj)
        {
            ArrayPos pos2 = (ArrayPos)obj;
            return this.x == pos2.x && this.y == pos2.y;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

    }

    public class Dialog_ApaprelSelector : Window
    {

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(480, 400);
            }
        }

        public Dialog_ApaprelSelector()
        {
            this.doCloseX = true;
            this.onlyOneOfTypeAllowed = true;
            this.absorbInputAroundWindow = true;
            this.closeOnCancel = true;
            this.doCloseButton = true;

        }


        static ListingStandardWrapper ww = new ListingStandardWrapper("");

        const float IconSize = 50;
        const float IconGap = 54;
        const float Dif = 2;
        const int xMax = 8;

        static Vector2 offset = new Vector2(0, 0);

        static bool first = true;
        static bool firstCat = true;
        static bool firstGrp = true;
        static List<string> groupMenuList = new List<string>();
        static List<string> categoryMenuList = new List<string>();

        static FloatMenuW fmwCat = new FloatMenuW();
        static FloatMenuW fmwGrp = new FloatMenuW();
        static FloatMenuW fmwSet = new FloatMenuW();

        static ThingCategoryDef nowCat = null;
        static ApparelGroup nowGrp = null;

        static List<ThingCategoryDef> categoryList = new List<ThingCategoryDef>();
        static List<ApparelListRecord> activeList = new List<ApparelListRecord>();
        public override void DoWindowContents(Rect inRect)
        {

            if (first)
            {
                activeList = ApparelList.list;

                for(int i = -2; i <= 2; i++)
                {
                    VisibleMode visible = Util.ParseEnum<VisibleMode>(i);
                    fmwSet.AddMenu(("TOC." + visible.ToString()).TranslateW(visible.ToString()), i);
                    fmwSet.SetAction(SetDisplayingCommand);
                }

                first = false;
            }
            TextAnchor anc = Text.Anchor;
            GameFont font = Text.Font;

            //Text.Font = GameFont.Tiny;
            //Widgets.Label(new Rect(0, 0, 200, 20), "IconSelector".TranslateW("Icon Selector."));
            //Text.Font = font;

            Text.Anchor = TextAnchor.MiddleRight;

            

            Text.Anchor = anc;

            DrawDiscription(new Rect(0, 0, 24, 24));
            if (Widgets.ButtonText(new Rect(50, 0, 100, 24), "TOC.bulkChange".TranslateW("Bulk change")))
            {
                fmwSet.Show();
            }
            ww.Tooltip(new Rect(50, 0, 100, 24), "TOC.bulkChangeTooltip".TranslateW("Change the settings of the displayed apparels at once. "));


            if (Widgets.ButtonText(new Rect(inRect.width-200,0,100,24), "TOC.filter".TranslateW("Filter")))
            {
                if (firstCat)
                {
                    MakeCategoryMenuList();
                    fmwCat.AddMenu("None".TranslateW("None"), -1);
                    for (int i = 0; i < categoryMenuList.Count; i++)
                    {
                        fmwCat.AddMenu(categoryMenuList[i], i);
                    }
                    fmwCat.SetAction(CategoryFilterlingCommand);
                    firstCat = false;
                }
                fmwCat.Show();
            }
            if (Widgets.ButtonText(new Rect(inRect.width - 100, 0, 100, 24), "TOC.filter".TranslateW("Filter")+"2"))
            {
                if (firstGrp)
                {
                    MakeGroupMenuList();
                    fmwGrp.AddMenu("None".TranslateW("None"), -1);
                    for (int i = 0; i < groupMenuList.Count; i++)
                    {
                        fmwGrp.AddMenu(groupMenuList[i], i);
                    }
                    fmwGrp.SetAction(GroupFilterlingCommand);
                    firstGrp = false;
                }
                fmwGrp.Show();
            }

            ww.ListRect = new Rect(inRect.x,inRect.y+24f,inRect.width,inRect.height-72f);

            ww.BeginWithScroll();
            int x = 0,y = 0;

            ApparelList.MakeTexListOnce();

            foreach(ApparelListRecord alr in activeList)
            {
                string tt = alr.thing.label + "("+("TOC."+alr.Visible.ToString()).TranslateW(alr.Visible.ToString()) +")"+
                    "\n" + alr.thing.defName;

                ww.Tooltip(DrawTex(alr, x, y), tt);
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
                    int num = pos.y * xMax + pos.x;
                    if (Util.HasGraphic(activeList[num].thing))
                    {
                        if (Util.GetLeftClick())
                        {
                            ApplyChange(activeList[num]);
                        }
                        if (Util.GetRightClick())
                        {
                            ApplyChange(activeList[num], false);
                        }
                    }
                }
            }

            ww.EndWithScroll();
        }


        void MakeGroupMenuList()
        {
            foreach(ApparelGroup ag in ApparelGroup.groupList)
            {
                groupMenuList.Add(ag.ToString(",","",false,true));
            }
        }
        void MakeCategoryMenuList()
        {
            ThingCategoryDef apparelCategory = DefDatabase<ThingCategoryDef>.AllDefsListForReading.Where(x => x.defName == "Apparel").FirstOrDefault();

            if (apparelCategory != null)
            {
                foreach (ThingCategoryDef tcd in apparelCategory.ThisAndChildCategoryDefs)
                {
                    categoryList.Add(tcd);
                    categoryMenuList.Add(GetCategoryStringFull(tcd));
                }
            }
        }


        void GroupFilterlingCommand(int num)
        {
            debug.WriteLine("gfc:" + num);
            if (num >= 0)
            {
                nowGrp = ApparelGroup.groupList.ToArray()[num];
            }
            else
            {
                nowGrp = null;
            }
            Filterling();
            
        }
        void CategoryFilterlingCommand(int num)
        {
            debug.WriteLine("cfc:"+num);
            if (num >= 0)
            {
                nowCat = categoryList[num];
            }
            else
            {
                nowCat = null;
            }
            Filterling();
        }
        void SetDisplayingCommand(int num)
        {
            VisibleMode visible = Util.ParseEnum<VisibleMode>(num);
            foreach(ApparelListRecord alr in activeList)
            {
                if (Util.HasGraphic(alr.thing))
                {
                    alr.Visible = visible;
                }
            }
        }

        void Filterling()
        {
            activeList = ApparelList.list.ListFullCopy();

            for(int i = activeList.Count - 1; i >= 0; i--)
            {
                if (nowCat != null)
                {
                    if (!activeList[i].thing.thingCategories.Contains(nowCat))
                    {
                        activeList.RemoveAt(i);
                        continue;
                    }
                }

                if (nowGrp != null)
                {
                    if (activeList[i].group.hashKey != nowGrp.hashKey)
                    {
                        activeList.RemoveAt(i);
                        continue;
                    }
                }
            }
        }

        string GetCategoryStringFull(ThingCategoryDef def,string delimiter = "/")
        {
            ThingCategoryDef now = def;
            string ret = "";

            ret = now.label;

            now = now.parent;
            while (now != null && now.defName!="Root")
            {
                ret = now.label + delimiter + ret;
                now = now.parent;
            } 
            return ret;
        }

        private void DrawDiscription(Rect rect)
        {
            if (ListingStandardWrapper.TooltipS(rect, "TOC.selectorInformation".TranslateW("If you left-click the icon, it will change from \"Forced display only when not drafted\"→\"Forced display\"→\"None\".\nRight - click to change \"Show only when drafting\"→\"Hide\"→\"None\".\nThe icon is gray for apparel that has no wearing image.")))
            {
                Widgets.DrawBoxSolid(rect, Color.gray);
            }

            Widgets.DrawTextureFitted(rect, TakeOffMain.infoTex, 1f);
        }

        private Rect DrawTex(ApparelListRecord alr, int x,int y)
        {
            Rect rect = new Rect(x * IconGap + offset.x, y * IconGap + offset.y, IconSize, IconSize);

            switch (alr.Visible)
            {
                case VisibleMode.Never:
                    Widgets.DrawBoxSolid(rect, new Color32(170,64,64,255));
                    break;
                case VisibleMode.Invisible:
                    Widgets.DrawBoxSolid(rect, new Color32(255, 128, 128, 255));
                    break;
                case VisibleMode.Visible:
                    Widgets.DrawBoxSolid(rect, new Color32(128, 170, 255, 255));
                    break;
                case VisibleMode.Force:
                    Widgets.DrawBoxSolid(rect, new Color32(64, 128, 255, 255));
                    break;
                default:
                    break;
            }

            ww.DrawTexture(rect, alr.tex);

            if (!Util.HasGraphic(alr.thing))
            {
                Widgets.DrawBoxSolid(rect, new Color32(128, 128, 128, 128));
            }
            return rect;
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

        void ApplyChange(ApparelListRecord alr, bool left=true)
        {
            if (left)
            {
                if (alr.Visible <= 0)
                {
                    alr.Visible = VisibleMode.Visible;
                }
                else
                {
                    if (alr.Visible == VisibleMode.Visible)
                    {
                        alr.Visible = VisibleMode.Force;
                    }
                    else
                    {
                        alr.Visible = VisibleMode.None;
                    }
                }
            }
            else
            {
                if (alr.Visible >= 0)
                {
                    alr.Visible = VisibleMode.Invisible;
                }
                else
                {
                    if (alr.Visible == VisibleMode.Invisible)
                    {
                        alr.Visible = VisibleMode.Never;
                    }
                    else
                    {
                        alr.Visible = VisibleMode.None;
                    }
                }
            }
        }

        public override void PostClose()
        {
            TakeOffSettings.useInvisibleCount++;
            base.PostClose();
            UI.UnfocusCurrentControl();
        }

    }
}
