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
        public static ThingCategoryDef NoCategory=new ThingCategoryDef();
        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(800, 600);
            }
        }

        public Dialog_ApaprelSelector()
        {
            this.doCloseX = true;
            this.onlyOneOfTypeAllowed = true;
            this.absorbInputAroundWindow = true;
            this.closeOnCancel = true;
            this.doCloseButton = true;
            this.resizeable = true;
            this.draggable = true;
        }


        static ListingStandardWrapper ww = new ListingStandardWrapper("");

        const float IconGap = 48;

        static Vector2 offset = new Vector2(0, 0);

        static bool first = true;
        static bool firstCat = true;
        static bool firstGrp = true;
        static List<string> groupMenuList = new List<string>();
        static List<string> categoryMenuList = new List<string>();
        static Dictionary<int,string> layerMenuDic = new Dictionary<int, string>();

        static FloatMenuW fmwCatFilt = new FloatMenuW();
        static FloatMenuW fmwGrpFilt = new FloatMenuW();
        static FloatMenuW fmwLayFilt = new FloatMenuW();
        static FloatMenuW fmwModFilt = new FloatMenuW();
        static FloatMenuW fmwBulkMod = new FloatMenuW();
        static FloatMenuW fmwBulkLay = new FloatMenuW();
        static FloatMenuW fmwLay = new FloatMenuW();
        static FloatMenuW fmwMod = new FloatMenuW();

        static ThingCategoryDef nowCat = null;
        static ApparelGroup nowGrp = null;
        static LayerPriority nowLay = LayerPriority.Null;
        static int nowModeDrafted = -1;
        static int nowModeNormal = -1;

        static List<ThingCategoryDef> categoryList = new List<ThingCategoryDef>();
        static List<ApparelListRecord> activeList = new List<ApparelListRecord>();

        static float l1 = 0.3f;
        static float l2 = 0.6f;

        Color lineGray = new Color(0.4f, 0.4f, 0.4f);
        Color lineGray2 = new Color(0.3f, 0.3f, 0.3f);

        //alrをリスト化→lp辞書作成→lpがデフォルト以外が設定されたらcaldに追加、デフォルト値を用意してデフォルトなら辞書から削除
        //そのためのメニューリストを作成してボタンに設定する
        //左側をレイヤー設定、右側を強制表示設定にする
        public override void DoWindowContents(Rect inRect)
        {
            if (first)
            {
                activeList = ApparelList.list;

                NoCategory.defName = "NoCategory";
                NoCategory.label = "TOC.NoCategory".TranslateW("No Category");

                layerMenuDic.Add((int)LayerPriority.Underwear, LayerPriority.Underwear.Translate());
                layerMenuDic.Add((int)LayerPriority.OnSkin, LayerPriority.OnSkin.Translate());
                layerMenuDic.Add((int)LayerPriority.Middle, LayerPriority.Middle.Translate());
                layerMenuDic.Add((int)LayerPriority.Shell, LayerPriority.Shell.Translate());
                layerMenuDic.Add((int)LayerPriority.Headgear, LayerPriority.Headgear.Translate());
                fmwLay.SetAction(LayerCommand);
                foreach(ApparelListRecord alr in ApparelList.list)
                {
                    if (!CustomApparelLayerDictionary.DefsLayerDic.ContainsKey(alr.thing))
                    {
                        CustomApparelLayerDictionary.DefsLayerDic[alr.thing] = TakeOffComp.CalcLayerPriorityDefault(alr.thing.apparel.LastLayer);
                    }
                }

                fmwMod.AddMenu("TOC.None".TranslateW("None"), (int)VisibleModeSingle.None);
                fmwMod.AddMenu("TOC.Invisible".TranslateW("Invisible"), (int)VisibleModeSingle.Invisible);
                fmwMod.AddMenu("TOC.Force".TranslateW("Force visible"), (int)VisibleModeSingle.Force);
                fmwMod.SetAction(ModeCommand);

                fmwBulkLay.AddMenu("default".Translate(), -1);
                fmwLayFilt.AddMenu("None".Translate(), -1);
                foreach (KeyValuePair<int,string> kvp in layerMenuDic)
                {
                    fmwBulkLay.AddMenu(kvp.Value, kvp.Key);
                    fmwLayFilt.AddMenu(kvp.Value, kvp.Key);
                }
                fmwBulkLay.SetAction(BulkLayerCommand);
                fmwLayFilt.SetAction(LayerFilterCommand);

                for (int i = 0; i <= 2; i++)
                {
                    for(int j = 0; j <= 2; j++)
                    {
                        VisibleMode vm = (VisibleMode)(i * 10 + j);
                        fmwBulkMod.AddMenu(vm.GetLabel(),(int)vm);
                    }
                }
                for (int i = 0; i <= 2; i++)
                {
                    VisibleMode vm = (VisibleMode)(i * 10);
                    fmwBulkMod.AddMenu(vm.GetLabel(true), -(i+1) * 10);
                }
                for (int i = 0; i <= 2; i++)
                {
                    VisibleMode vm = (VisibleMode)(i);
                    fmwBulkMod.AddMenu( vm.GetLabel(false) , -(i+1));
                }

                fmwBulkMod.SetAction(BulkModeCommand);

                fmwModFilt.AddMenu("None".Translate(), -1);
                for (int i = 0; i <= 2; i++)
                {
                    VisibleModeSingle vms = (VisibleModeSingle)(i);
                    fmwModFilt.AddMenu(vms.GetLabel(), i);
                }
                fmwModFilt.SetAction(ModeFilterCommand);


                first = false;
            }
            TextAnchor anc = Text.Anchor;
            GameFont font = Text.Font;

            //Text.Font = GameFont.Tiny;
            //Widgets.Label(new Rect(0, 0, 200, 20), "IconSelector".TranslateW("Icon Selector."));
            //Text.Font = font;

            Text.Anchor = TextAnchor.MiddleRight;

            Text.Anchor = anc;

            ww.BackupGUI();


            inRect.y -= 20f;
            inRect.height += 20f;

            ww.ListRect = new Rect(inRect.x, inRect.y + 56f, inRect.width, inRect.height - 113f);
            ww.canvasRect.height += 20f; //別ウィンドウの場合ここ調整しないと下部が埋もれる謎
            //ww.viewRect.height -= 24f;
            float x0 = ww.viewRect.xMin;
            float x1 = ww.viewRect.xMin + IconGap;
            float x2 = (int)(ww.viewRect.xMin + IconGap + (ww.viewRect.width - IconGap) * l1);
            float x3 = (int)(ww.viewRect.xMin + IconGap + (ww.viewRect.width - IconGap) * l2);
            float x4 = ww.viewRect.xMin + ww.viewRect.width;
            float y1 = 0f;
            float y2 = 26f;
            float y3 = 48f;


            //1段目 infoとフィルター
            DrawDiscription(new Rect(0, 0, 24, 24));

            if (Widgets.ButtonImageWithBG(new Rect(x2 - 48, y1, 24, 24), TakeOffMain.filterIcon))
            {
                if (firstCat)
                {
                    MakeCategoryMenuList();
                    fmwCatFilt.AddMenu("None".TranslateW("None"), -1);
                    fmwCatFilt.AddMenu(NoCategory.label, -2);
                    for (int i = 0; i < categoryMenuList.Count; i++)
                    {
                        fmwCatFilt.AddMenu(categoryMenuList[i], i);
                    }
                    fmwCatFilt.SetAction(CategoryFilterlingCommand);
                    firstCat = false;
                }
                fmwCatFilt.Show();
            }
            if (Widgets.ButtonImageWithBG(new Rect(x2-25, y1, 24, 24), TakeOffMain.filterIcon))
            {
                if (firstGrp)
                {
                    MakeGroupMenuList();
                    fmwGrpFilt.AddMenu("None".TranslateW("None"), -1);
                    for (int i = 0; i < groupMenuList.Count; i++)
                    {
                        fmwGrpFilt.AddMenu(groupMenuList[i], i);
                    }
                    fmwGrpFilt.SetAction(GroupFilterlingCommand);
                    firstGrp = false;
                }
                fmwGrpFilt.Show();
            }
            //レイヤー
            ww.Tooltip(new Rect(x2 + 3 , y1, 100, 24), "TOC.bulkChangeTooltip".TranslateW("Change the settings of the displayed apparels at once. "));
            if (Widgets.ButtonText(new Rect(x2+1, y1, 100, 24), "TOC.bulkChange".TranslateW("Bulk change")))
            {
                fmwBulkLay.Show();
            }

            if (Widgets.ButtonImageWithBG(new Rect(x3 - 25, y1, 24, 24), TakeOffMain.filterIcon))
            {
                fmwLayFilt.Show();
            }


            //強制
            ww.Tooltip(new Rect(x3 + 1, y1, 100, 24), "TOC.bulkChangeTooltip".TranslateW("Change the settings of the displayed apparels at once. "));
            if (Widgets.ButtonText(new Rect(x3 + 4, y1, 100, 24), "TOC.bulkChange".TranslateW("Bulk change")))
            {
                fmwBulkMod.Show();
            }

            //2段目 見出し行

            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(new Rect(x0, y2, x2, y3-y2+24f), ThingCategoryDefOf.Apparel.label);

            Widgets.Label(new Rect(x2 + 2, y2, x3 - x2 - 2, 24), "TOC.Layer".TranslateW("Layer"));

            Widgets.Label(new Rect(x3 + 2, y2, x4 - x3 - 2, 24), "TOC.ForceMenu".TranslateW("Force Display"));

            Widgets.Label(new Rect(x3 + 2, y3, (x4 - x3) / 2 - 2, 24), "TOC.Drafted".TranslateW("Drafted"));
            Widgets.Label(new Rect(x3 + (x4 - x3) / 2, y3, (x4 - x3) / 2 - 2, 24), "TOC.NotDrafted".TranslateW("Not drafted"));
            if (Widgets.ButtonImageWithBG(new Rect(x3 + (x4 - x3) / 2 - 21, y3, 20, 20), TakeOffMain.filterIcon))
            {
                fmwModFilt.SetParam(true);
                fmwModFilt.Show();
            }
            if (Widgets.ButtonImageWithBG(new Rect(x4 - 21, y3, 20, 20), TakeOffMain.filterIcon))
            {
                fmwModFilt.SetParam(false);
                fmwModFilt.Show();
            }

            //線
            Widgets.DrawLine(new Vector2(x3, y3 ), new Vector2(x4, y3 ), lineGray2, 0.5f);

            Widgets.DrawLine(new Vector2(x2 , y2+2), new Vector2(x2 , inRect.height - 57f), lineGray, 1);
            Widgets.DrawLine(new Vector2(x3 , y2+2), new Vector2(x3 , inRect.height - 57f), lineGray, 1);

            Widgets.DrawLine(new Vector2(x0, y3 + 21), new Vector2(x4 - x0, y3 + 21), lineGray, 1);

            Widgets.DrawLine(new Vector2((x3+x4)/2, y3 + 2), new Vector2((x3 + x4) / 2, inRect.height - 57f), lineGray2, 0.5f);

            ww.RecoverGUI();


            ww.BeginWithScroll();

            ApparelList.MakeTexListOnce();

            foreach (ApparelListRecord alr in activeList)
            {

                DrawRow(alr);
            }


            //int x = 0, y = 0;


            //foreach (ApparelListRecord alr in activeList)
            //{
            //    string tt = alr.thing.label + "(" + ("TOC." + alr.Visible.ToString()).TranslateW(alr.Visible.ToString()) + ")" +
            //        "\n" + alr.thing.defName;

            //    ww.Tooltip(DrawTex(alr, x, y), tt);
            //    x++;
            //    if (x == xMax)
            //    {
            //        x = 0; y++;
            //    }
            //}

            //x--;
            //if (x < 0)
            //{
            //    x = xMax - 1;
            //    y--;
            //}

            //ArrayPos? mousePos = GetMousePosition(xMax, y + 1);

            //if (mousePos != null)
            //{
            //    ArrayPos pos = (ArrayPos)mousePos;
            //    if (!((pos.y == y) && (pos.x > x)))
            //    {
            //        DrawFrame((ArrayPos)mousePos, Color.white);
            //        int num = pos.y * xMax + pos.x;
            //        if (Util.HasGraphic(activeList[num].thing))
            //        {
            //            if (Util.GetLeftClick())
            //            {
            //                ApplyChange(activeList[num]);
            //            }
            //            if (Util.GetRightClick())
            //            {
            //                ApplyChange(activeList[num], false);
            //            }
            //        }
            //    }
            //}

            ww.EndWithScroll();
        }


        void MakeGroupMenuList()
        {
            foreach (ApparelGroup ag in ApparelGroup.groupList)
            {
                groupMenuList.Add(ag.ToString(",", "", false, true));
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
            debug.WriteLine("cfc:" + num);
            if (num >= 0)
            {
                nowCat = categoryList[num];
            }
            else
            {
                if (num == -2)
                {
                    nowCat = NoCategory;
                }
                else
                {
                    nowCat = null;
                }
            }
            Filterling();
        }
        void ModeFilterCommand(int num)
        {
            if (fmwModFilt.GetParam<bool>())
            {
                nowModeDrafted = num;
            }
            else
            {
                nowModeNormal = num;
            }
            Filterling();
        }
        void LayerFilterCommand(int num)
        {
            if (num < 0)
            {
                nowLay = LayerPriority.Null;
            }
            else
            {
                nowLay = (LayerPriority)num;
            }
            Filterling();
        }
        void BulkModeCommand(int num)
        {

            VisibleMode visible=VisibleMode.None;
            VisibleModeSingle vms=VisibleModeSingle.None;
            bool drafted = false;
            if (num >= 0)
            {
                visible = Util.ParseEnum<VisibleMode>(num);
            }
            else
            {
                if (num <= -10)
                {
                    drafted = true;
                    vms = Util.ParseEnum<VisibleModeSingle>(((-num)/10)-1);
                }
                else
                {
                    vms = Util.ParseEnum<VisibleModeSingle>(((-num) % 10) - 1);
                }
            }
            foreach (ApparelListRecord alr in activeList)
            {
                if (Util.HasGraphic(alr.thing))
                {
                    if (num >= 0)
                    {
                        CustomApparelLayerDictionary.SetVisible(alr.thing, visible);
                    }
                    else
                    {
                        CustomApparelLayerDictionary.SetVisible(alr.thing, vms, drafted);
                    }
                }
            }
        }
        void BulkLayerCommand(int num)
        {
            foreach (ApparelListRecord alr in activeList)
            {
                if (Util.HasGraphic(alr.thing))
                {
                    if (num < 0)
                    {
                        CustomApparelLayerDictionary.DefsLayerDic[alr.thing] = TakeOffComp.CalcLayerPriorityDefault(alr.thing.apparel.LastLayer);
                    }
                    else
                    {
                        CustomApparelLayerDictionary.DefsLayerDic[alr.thing] = Util.ParseEnum<LayerPriority>(num);
                    }
                }
            }
        }
        void LayerCommand(int num)
        {
            ApparelListRecord alr = (ApparelListRecord)fmwLay.GetObject;
            CustomApparelLayerDictionary.DefsLayerDic[alr.thing] = Util.ParseEnum<LayerPriority>(num);
        }
        void ModeCommand(int num)
        {
            ApparelListRecord alr = (ApparelListRecord)fmwMod.GetObject;
            CustomApparelLayerDictionary.SetVisible(alr.thing, (VisibleModeSingle)num, fmwMod.GetParam<bool>());
        }

        void Filterling()
        {
            activeList = ApparelList.list.ListFullCopy();

            for (int i = activeList.Count - 1; i >= 0; i--)
            {
                if (nowCat != null)
                {
                    if (activeList[i] != null && activeList[i].thing != null)
                    {

                        debug.WriteLine("no category");
                        if ((nowCat != NoCategory) && (activeList[i].thing.thingCategories == null || !activeList[i].thing.thingCategories.Contains(nowCat)) 
                            || ((nowCat==NoCategory) && activeList[i].thing.thingCategories != null))
                        {
                            activeList.RemoveAt(i);
                            continue;
                        }
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

                if (nowLay != LayerPriority.Null)
                {
                    if (CustomApparelLayerDictionary.DefsLayerDic.TryGetValue(activeList[i].thing,out LayerPriority lp))
                    {
                        if (lp != nowLay)
                        {
                            activeList.RemoveAt(i);
                            continue;
                        }
                    }
                }

                if (nowModeDrafted >=0)
                {
                    VisibleMode vm;
                    if (!CustomApparelLayerDictionary.DefsModeDic.TryGetValue(activeList[i].thing, out vm))
                    {
                        vm = VisibleMode.None;
                    }
                    if ((int)vm.GetDrafted() != nowModeDrafted)
                    {
                        activeList.RemoveAt(i);
                        continue;
                    }
                }
                if (nowModeNormal >= 0)
                {
                    VisibleMode vm;
                    if (!CustomApparelLayerDictionary.DefsModeDic.TryGetValue(activeList[i].thing, out vm))
                    {
                        vm = VisibleMode.None;
                    }
                    if ((int)vm.GetNormal() != nowModeNormal)
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

        private void DrawRow(ApparelListRecord alr)
        {
            bool graphicFlg = Util.HasGraphic(alr.thing);

            if (!graphicFlg)
            {
                return;
            }

            ww.AllocOneCulumn((int)IconGap/2);
            Rect rect = new Rect(ww.viewRect.xMin, ww.top, IconGap, IconGap);
            float top = ww.top;
            float bottom = top + IconGap;


            //switch (alr.Visible)
            //{
            //    case VisibleMode.Never:
            //        Widgets.DrawBoxSolid(rect, new Color32(170, 64, 64, 255));
            //        break;
            //    case VisibleMode.Invisible:
            //        Widgets.DrawBoxSolid(rect, new Color32(255, 128, 128, 255));
            //        break;
            //    case VisibleMode.Visible:
            //        Widgets.DrawBoxSolid(rect, new Color32(128, 170, 255, 255));
            //        break;
            //    case VisibleMode.Force:
            //        Widgets.DrawBoxSolid(rect, new Color32(64, 128, 255, 255));
            //        break;
            //    default:
            //        break;
            //}

            //string tt = alr.thing.label + "(" + ("TOC." + alr.Visible.ToString()).TranslateW(alr.Visible.ToString()) + ")" + "\n" + alr.thing.defName;

            if (!graphicFlg)
            {
                Widgets.DrawBoxSolid(rect, new Color32(128, 128, 128, 128));
            }

            ww.LabelSized(alr.thing.LabelCap, null, 0f, l1, leftOffset: IconGap);


            int lmd = (int)CustomApparelLayerDictionary.DefsLayerDic[alr.thing];
            int deflm = (int)TakeOffComp.CalcLayerPriorityDefault(alr.thing.apparel.LastLayer);
            string nowLayStr = layerMenuDic[lmd];
            if (lmd == deflm) nowLayStr += "(" + "default".Translate() + ")";

            ww.AllocOneCulumn((int)IconGap / 2);

            ww.LabelSized(alr.thing.defName, null, 0f, l1, leftOffset: IconGap);


            if (graphicFlg)
            {
                Rect layRect = ww.PrevColRect(l1, l2 - l1, IconGap);
                layRect.width -= 2f;
                if (Widgets.ButtonText(layRect, nowLayStr))
                {
                    fmwLay.ResetList();
                    fmwLay.SetObject(alr);
                    foreach (KeyValuePair<int, string> kvp in layerMenuDic)
                    {
                        if (kvp.Key == deflm)
                        {
                            fmwLay.AddMenu(layerMenuDic[kvp.Key] + "(" + "default".Translate() + ")", kvp.Key);
                        }
                        else
                        {
                            fmwLay.AddMenu(layerMenuDic[kvp.Key], kvp.Key);
                        }
                    }
                    fmwLay.Show();
                }
                layRect = ww.PrevColRect(l1, l2 - l1, IconGap);

                if (ww.ButtonTextSized(CustomApparelLayerDictionary.GetMode( alr.thing,true).GetLabel(), l2, (1f-l2)/2, leftOffset: IconGap))
                {
                    fmwMod.SetObject(alr);
                    fmwMod.SetParam(true);
                    fmwMod.Show();
                }
                if (ww.ButtonTextSized(CustomApparelLayerDictionary.GetMode(alr.thing, false).GetLabel(), l2 + ((1f - l2) / 2), (1f - l2) / 2, leftOffset: IconGap))
                {
                    fmwMod.SetObject(alr);
                    fmwMod.SetParam(false);
                    fmwMod.Show();
                }
            }

            ww.DrawTexture(rect, alr.tex);

            GUI.color = lineGray;
            ww.GapLine(3);

            ww.RecoverGUI();

        }

        //private string LayerPriorityToString(LayerPriority lp)
        //{
        //    if (lp == LayerPriority.Headgear)
        //    {
        //        return "Headgear.label".Translate();
        //    }
        //    string str = lp.ToString();
        //    if ((int)lp>=2 && (int)lp <= 6)
        //    {
        //        return ("TakeOffIndoor." + str).TranslateW(str);
        //    }
        //    return str;
        //}
        //private Rect DrawTex(ApparelListRecord alr, int x,int y)
        //{
        //    Rect rect = new Rect(x * IconGap + offset.x, y * IconGap + offset.y, IconSize, IconSize);

        //    switch (alr.Visible)
        //    {
        //        case VisibleMode.Never:
        //            Widgets.DrawBoxSolid(rect, new Color32(170,64,64,255));
        //            break;
        //        case VisibleMode.Invisible:
        //            Widgets.DrawBoxSolid(rect, new Color32(255, 128, 128, 255));
        //            break;
        //        case VisibleMode.Visible:
        //            Widgets.DrawBoxSolid(rect, new Color32(128, 170, 255, 255));
        //            break;
        //        case VisibleMode.Force:
        //            Widgets.DrawBoxSolid(rect, new Color32(64, 128, 255, 255));
        //            break;
        //        default:
        //            break;
        //    }

        //    ww.DrawTexture(rect, alr.tex);

        //    if (!Util.HasGraphic(alr.thing))
        //    {
        //        Widgets.DrawBoxSolid(rect, new Color32(128, 128, 128, 128));
        //    }
        //    return rect;
        //}

        //public void DrawFrame(ArrayPos pos, Color color)
        //{
        //    Color tmpc = GUI.color;
        //    GUI.color = color;

        //    Rect rect = new Rect(pos.x * IconGap + offset.x, pos.y * IconGap + offset.y, IconSize, IconSize);

        //    Widgets.DrawBox(rect, 2);
        //    GUI.color = tmpc;
        //}


        //public ArrayPos? GetMousePosition(int xmax, int ymax)
        //{
        //    for (int x = 0; x < xmax; x++)
        //    {
        //        for (int y = 0; y < ymax; y++)
        //        {
        //            if (Mouse.IsOver(new Rect(x * IconGap + offset.x - Dif, y * IconGap + offset.y - Dif, IconGap, IconGap)))
        //            {
        //                return new ArrayPos(x, y);
        //            }
        //        }
        //    }
        //    return null;
        //}

        //void ApplyChange(ApparelListRecord alr, bool left=true)
        //{
        //    if (left)
        //    {
        //        if (alr.Visible <= 0)
        //        {
        //            alr.Visible = VisibleMode.Visible;
        //        }
        //        else
        //        {
        //            if (alr.Visible == VisibleMode.Visible)
        //            {
        //                alr.Visible = VisibleMode.Force;
        //            }
        //            else
        //            {
        //                alr.Visible = VisibleMode.None;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        if (alr.Visible >= 0)
        //        {
        //            alr.Visible = VisibleMode.Invisible;
        //        }
        //        else
        //        {
        //            if (alr.Visible == VisibleMode.Invisible)
        //            {
        //                alr.Visible = VisibleMode.Never;
        //            }
        //            else
        //            {
        //                alr.Visible = VisibleMode.None;
        //            }
        //        }
        //    }
        //}

        public override void PostClose()
        {
            TakeOffSettings.useInvisibleCount++;
            base.PostClose();
            UI.UnfocusCurrentControl();
        }

    }
}
