using HarmonyLib;
using RimWorld;
using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace NoobertNebulous
{
    public class NoobertNebulousMod : Mod
    {
        public static NoobertNebulousSettings settings;
        public NoobertNebulousMod(ModContentPack pack) : base(pack)
        {
            settings = GetSettings<NoobertNebulousSettings>();
			new Harmony("NoobertNebulousMod").PatchAll();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            settings.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return Content.Name;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class HotSwappableAttribute : Attribute
    {
    }

    [HotSwappableAttribute]

    public class NoobertNebulousSettings : ModSettings
    {
        public static int wealthInterval = 5000;
        public static IntRange queueSizeRange = new IntRange(1, 4);
        public static int maxVideoLimit = 30;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref wealthInterval, "wealthInterval", 5000);
            Scribe_Values.Look(ref queueSizeRange, "queueSizeRange", new IntRange(1, 4));
            Scribe_Values.Look(ref maxVideoLimit, "maxVideoCount", 30);
        }

        private Vector2 scrollPos;
        private float scrollHeight = 99999999;
		
        public void DoSettingsWindowContents(Rect inRect)
        {
            var viewRect = new Rect(inRect.x, inRect.y, inRect.width - 16, scrollHeight);
            scrollHeight = 0;
            Widgets.BeginScrollView(inRect, ref scrollPos, viewRect);
            var ls = new Listing_Standard();
            ls.Begin(viewRect);
            var initY = ls.curY;
            wealthInterval = (int)ls.SliderLabeled("NN.WealthInterval".Translate() + ": " + wealthInterval, wealthInterval, 100, 10000);
            IntRangeLabeled(ls, "NN.QueueSizeRange".Translate(), ref queueSizeRange, 1, 10);
            maxVideoLimit = (int)ls.SliderLabeled("NN.MaxVideoLimit".Translate() + ": " + maxVideoLimit, maxVideoLimit, 1, 100);
            if (ls.ButtonText("Reset".Translate(), widthPct: 0.3f))
            {
                wealthInterval = 5000;
                queueSizeRange = new IntRange(1, 4);
            }
            ls.End();
            Widgets.EndScrollView();
            scrollHeight = ls.curY - initY;
		}

        public void IntRangeLabeled(Listing_Standard ls, string label, ref IntRange range, int min, int max)
        {
            Rect rect = ls.GetRect(28f);
            var labelRect = new Rect(rect.x, rect.y, 430, rect.height);
            Widgets.Label(labelRect, label);
            var intSetterRect = new Rect(labelRect.xMax, labelRect.y, rect.width - labelRect.width, rect.height);
            if (!ls.BoundingRectCached.HasValue || rect.Overlaps(ls.BoundingRectCached.Value))
            {
                Widgets.IntRange(intSetterRect, (int)ls.CurHeight, ref range, min, max);
            }
            ls.Gap(ls.verticalSpacing);
        }
    }
}
