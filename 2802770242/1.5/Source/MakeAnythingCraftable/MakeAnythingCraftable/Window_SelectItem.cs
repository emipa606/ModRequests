using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;
using System.Linq;
using RimWorld;
using System;
using System.Security.Cryptography;

namespace MakeAnythingCraftable
{
    public class Window_SelectItem<T> : Window where T : Def
    {
        private Vector2 scrollPosition;
        public override Vector2 InitialSize => new Vector2(620f, 500f);

        public List<T> allItems;

        public Action<T> actionOnSelect;

        public Func<T, int> ordering;
        public Func<T, string> labelGetter;
        public Window_SelectItem(List<T> items, Action<T> actionOnSelect, Func<T, int> ordering = null, Func<T, string> labelGetter = null)
        {
            doCloseButton = true;
            doCloseX = true;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = false;
            this.allItems = items;
            this.actionOnSelect = actionOnSelect;
            this.ordering = ordering;
            this.labelGetter = labelGetter;
        }
        string searchKey;
        public string GetLabel(T def) => labelGetter != null ? labelGetter(def) : def.label;
        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            Text.Anchor = TextAnchor.MiddleLeft;
            var searchLabel = new Rect(inRect.x, inRect.y, 60, 24);
            Widgets.Label(searchLabel, "MAC.Search".Translate());
            var searchRect = new Rect(searchLabel.xMax + 5, searchLabel.y, 200, 24f);
            searchKey = Widgets.TextField(searchRect, searchKey);
            Text.Anchor = TextAnchor.UpperLeft;

            Rect outRect = new Rect(inRect);
            outRect.y = searchRect.yMax + 5;
            outRect.yMax -= 70f;
            outRect.width -= 16f;

            var defs = searchKey.NullOrEmpty() ? allItems : allItems.Where(x => GetLabel(x).ToLower().Contains(searchKey.ToLower())).ToList();

            Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, (float)defs.Count() * 35f);
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            try
            {
                float num = 0f;
                if (ordering != null)
                {
                    defs = defs.OrderBy(x => ordering(x)).ThenBy(x => x.label).ToList();
                }
                foreach (T def in defs)
                {
                    Rect iconRect = new Rect(0f, num, 24, 32);
                    Widgets.InfoCardButton(iconRect, def);
                    if (def is ThingDef thingDef2)
                    {
                        iconRect.x += 24;
                        Widgets.ThingIcon(iconRect, thingDef2);
                    }
                    Rect rect = new Rect(iconRect.xMax + 5, num, viewRect.width * 0.7f, 32f);
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.Label(rect, GetLabel(def));
                    Text.Anchor = TextAnchor.UpperLeft;
                    rect.x = rect.xMax + 10;
                    rect.width = 100;
                    if (Widgets.ButtonText(rect, "MAC.Select".Translate()))
                    {
                        actionOnSelect(def);
                        SoundDefOf.Click.PlayOneShotOnCamera();
                        this.Close();
                    }
                    num += 35f;
                }
            }
            finally
            {
                Widgets.EndScrollView();
            }
        }
    }
}
