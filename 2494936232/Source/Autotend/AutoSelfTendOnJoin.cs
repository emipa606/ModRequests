using UnityEngine;
using Verse;

namespace Autotend
{
    public class AutoSelfTendOnJoin : Mod
    {
        public SelfTendOnJoinSettings Settings { get; }

        public AutoSelfTendOnJoin(ModContentPack content) : base(content)
        {
            Settings = GetSettings<SelfTendOnJoinSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("Enable auto setting priorities based on below settings", ref Settings.AutoSetPriorities);
            listingStandard.Label("Use -1 to disable auto setting that priority");
            listingStandard.LabeledIntRange("Priority 0", ref Settings.PriorityNone, -1, 20);
            listingStandard.LabeledIntRange("Priority 4", ref Settings.PriorityFour, -1, 20);
            listingStandard.LabeledIntRange("Priority 3", ref Settings.PriorityThree, -1, 20);
            listingStandard.LabeledIntRange("Priority 2", ref Settings.PriorityTwo, -1, 20);
            listingStandard.LabeledIntRange("Priority 1", ref Settings.PriorityOne, -1, 20);
            listingStandard.CheckboxLabeled("Increase priority by 1 in case of passion", ref Settings.OneUpForPassions);
            listingStandard.SliderLabeled("Minimum skill required to auto-enable self tending", ref Settings.MinimumSkillForSelfTending, "{0:0.##}", -1, 20);
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory() => "Self Tend On Join";
    }

    public class SelfTendOnJoinSettings : ModSettings
    {
        public IntRange PriorityOne = new IntRange(12, 20);
        public IntRange PriorityTwo = new IntRange(9, 12);
        public IntRange PriorityThree = new IntRange(6, 9);
        public IntRange PriorityFour = new IntRange(3, 6);
        public IntRange PriorityNone = new IntRange(0, 3);

        public bool AutoSetPriorities = true;
        public bool OneUpForPassions = true;
        public int MinimumSkillForSelfTending;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref PriorityOne, nameof(PriorityOne));
            Scribe_Values.Look(ref PriorityTwo, nameof(PriorityTwo));
            Scribe_Values.Look(ref PriorityThree, nameof(PriorityThree));
            Scribe_Values.Look(ref PriorityFour, nameof(PriorityFour));
            Scribe_Values.Look(ref PriorityNone, nameof(PriorityNone));
            Scribe_Values.Look(ref AutoSetPriorities, nameof(AutoSetPriorities), true, true);
            Scribe_Values.Look(ref OneUpForPassions, nameof(OneUpForPassions), true, true);
            Scribe_Values.Look(ref MinimumSkillForSelfTending, nameof(MinimumSkillForSelfTending));
        }
    }

    public static class SettingsHelper
    {
        public static void LabeledIntRange(this Listing_Standard ls, string label, ref IntRange range, int min = 0, int max = 100, string tooltip = null)
        {
            Rect rect = ls.GetRect(height: Text.LineHeight);
            Rect rect2 = rect.LeftPart(pct: .70f).Rounded();
            Rect rect3 = rect.RightPart(pct: .30f).Rounded().LeftPart(pct: .9f).Rounded();
            rect3.yMin -= 5f;

            TextAnchor anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rect: rect2, label: label);

            Text.Anchor = TextAnchor.MiddleRight;
            int id = ls.CurHeight.GetHashCode();
            Widgets.IntRange(rect: rect3, id: id, range: ref range, min: min, max: max, labelKey: null);
            if (!tooltip.NullOrEmpty())
            {
                TooltipHandler.TipRegion(rect: rect, tip: tooltip);
            }
            Text.Anchor = anchor;
            ls.Gap(gapHeight: ls.verticalSpacing);
        }
        
        public static void SliderLabeled(this Listing_Standard ls, string label, ref int val, string format, float min = 0f, float max = 1f, string tooltip = null)
        {
            Rect rect = ls.GetRect(height: Text.LineHeight);
            Rect rect2 = rect.LeftPart(pct: .70f).Rounded();
            Rect rect3 = rect.RightPart(pct: .30f).Rounded().LeftPart(pct: .67f).Rounded();
            Rect rect4 = rect.RightPart(pct: .10f).Rounded();

            TextAnchor anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rect: rect2, label: label);

            float result = Widgets.HorizontalSlider(rect: rect3, value: val, leftValue: min, rightValue: max, middleAlignment: true);
            val = (int)result;
            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(rect: rect4, label: string.Format(format: format, arg0: val));
            if (!tooltip.NullOrEmpty())
            {
                TooltipHandler.TipRegion(rect: rect, tip: tooltip);
            }

            Text.Anchor = anchor;
            ls.Gap(gapHeight: ls.verticalSpacing);
        }

        public static bool Includes(this IntRange intRange, int val)
        {
            if (val >= intRange.min)
            {
                return val <= intRange.max;
            }
            return false;
        }

    }
}
