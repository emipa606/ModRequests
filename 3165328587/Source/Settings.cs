using UnityEngine;
using Verse;

namespace StrayBullets
{
    public class Settings : ModSettings
    {
        static bool _debug = false;

        static float _friendlyFireProbability = 0.4f;

        static float _baseHitProbability = 0.5f;

        static readonly float _cellHeight = Text.LineHeight * 1.5f;

        public static float FriendlyFireProbability => _friendlyFireProbability;

        public static float BaseHitProbability => _baseHitProbability;

        public static bool Debug => _debug;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<float>(ref _friendlyFireProbability, "FriendlyFireProbability", 0.4f);
            Scribe_Values.Look<float>(ref _baseHitProbability, "BaseHitProbability", 0.5f);
        }
        public void DrawWindow(Rect inRect)
        {
            Text.Anchor = TextAnchor.MiddleLeft;
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            Rect rect = listingStandard.GetRect(_cellHeight).LeftHalf();
            Widgets.Label(rect.LeftHalf(), Translator.Translate("setting_baseHitProb"));
            Widgets.HorizontalSlider(rect.RightHalf(), ref _baseHitProbability, new FloatRange(0f, 1f), _baseHitProbability.ToString());
            _baseHitProbability = Mathf.Round(_baseHitProbability * 100f) / 100f;
            Widgets.DrawHighlightIfMouseover(rect);
            listingStandard.Gap(2f);

            Rect rect2 = listingStandard.GetRect(_cellHeight).LeftHalf();
            Widgets.Label(rect2.LeftHalf(), Translator.Translate("setting_friendlyFireProb"));
            Widgets.HorizontalSlider(rect2.RightHalf(), ref _friendlyFireProbability, new FloatRange(0f, 1f), _friendlyFireProbability.ToString());
            _friendlyFireProbability = Mathf.Round(_friendlyFireProbability * 100f) / 100f;
            Widgets.DrawHighlightIfMouseover(rect2);
            listingStandard.Gap(2f);

            listingStandard.End();
            Text.Anchor = TextAnchor.UpperLeft;
        }
    }
}