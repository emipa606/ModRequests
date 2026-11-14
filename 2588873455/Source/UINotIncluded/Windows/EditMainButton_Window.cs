using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RimWorld;
using UINotIncluded.Widget.Configs;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace UINotIncluded.Windows
{
    public class EditMainButton_Window : Window
    {
        private const int IconSize = 40;
        private const int IconPadding = 5;
        private const int IconMargin = 5;
        private const int ColorSize = 22;
        private const int ColorPadding = 2;
        private const int MaxSymbolLength = 40;
        private static readonly Vector2 ButSize = new Vector2(150f, 38f);
        private static readonly float EditFieldHeight = 30f;
        private static readonly float ResetButtonWidth = 60f;
        private static readonly Regex ValidSymbolRegex = new Regex("^[\\p{L}0-9 '\\-]*$");
        private static readonly Dictionary<string, Texture2D> cacheIcons = new Dictionary<string, Texture2D>();
        private static List<string> cacheIconsPath;
        internal readonly ButtonConfig config;

        //private string newLabel;
        //private string iconPath;
        //private bool newMinimized;
        private Vector2 scrollPos;

        private float viewHeight;

        public EditMainButton_Window(ButtonConfig config)
        {
            this.config = config;
            absorbInputAroundWindow = true;
        }

        public override Vector2 InitialSize => new Vector2(350f, 600f);

        public override void OnAcceptKeyPressed()
        {
            TryAccept();
            Event.current.Use();
        }

        public override void DoWindowContents(Rect rect)
        {
            var inRect = rect;
            inRect.height -= CloseButSize.y;
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(inRect.x, inRect.y, rect.width, 35f),
                "UINotIncluded.Windows.EditButton".Translate());
            Text.Font = GameFont.Small;
            var defaultLabel = (string)"Default".Translate();
            inRect.yMin += 45f;
            var curY = inRect.y;
            var iconX = (float)Math.Floor(inRect.width / 2) - 44f;
            if (config.Icon != null) GUI.DrawTexture(new Rect(rect.x + iconX, curY + 5f, 88f, 88f), config.Icon);
            curY += 93f;
            var x = inRect.x + inRect.width / 3f;
            var width = (float)(inRect.xMax - (double)x - ResetButtonWidth - 10.0);
            var labelY = curY;
            Widgets.Label(inRect.x, ref labelY, inRect.width, "Name".Translate());
            config.Label = Widgets.TextField(new Rect(x, curY, width, EditFieldHeight), config.Label, 40,
                ValidSymbolRegex);

            var labelRect = new Rect(x, curY, width, EditFieldHeight);
            var defaultLabelRect = new Rect(labelRect.xMax + 10f, curY, ResetButtonWidth, EditFieldHeight);

            if (Widgets.ButtonText(defaultLabelRect, defaultLabel))
            {
                SoundDefOf.Click.PlayOneShotOnCamera();
                config.Label = config.Def.label;
            }

            curY += EditFieldHeight + 10f;

            var minimizedRect = new Rect(inRect.x, curY, width * 2 - Widgets.CheckboxSize - 11f, EditFieldHeight);
            var defaultMinimizedRect = new Rect(minimizedRect.xMax + 10f, curY, ResetButtonWidth, EditFieldHeight);
            Widgets.CheckboxLabeled(minimizedRect, "UINotIncluded.Windows.Minimize".Translate(), ref config.minimized);

            if (Widgets.ButtonText(defaultMinimizedRect, defaultLabel))
            {
                SoundDefOf.Click.PlayOneShotOnCamera();
                config.minimized = ((MainButtonDef)config.Def).minimized;
            }

            curY += EditFieldHeight + 10f;

            var hidelabelRect = new Rect(inRect.x, curY, width * 2 - Widgets.CheckboxSize - 11f, EditFieldHeight);
            Widgets.CheckboxLabeled(hidelabelRect, "Hide label", ref config.hideLabel);

            curY += EditFieldHeight + 10f;

            var forcheShow = new Rect(inRect.x, curY, width * 2 - Widgets.CheckboxSize - 11f, EditFieldHeight);
            Widgets.CheckboxLabeled(forcheShow, "Force visible", ref config.forceShow);

            curY += EditFieldHeight + 10f;

            var iconRect = new Rect(inRect.x, curY, width * 2 - Widgets.CheckboxSize - 11f, EditFieldHeight);
            var defaulticonRect = new Rect(minimizedRect.xMax + 10f, curY, ResetButtonWidth, EditFieldHeight);
            Widgets.Label(iconRect, "Icon".Translate());

            if (Widgets.ButtonText(defaulticonRect, defaultLabel))
            {
                SoundDefOf.Click.PlayOneShotOnCamera();
                config.IconPath = ((MainButtonDef)config.Def).iconPath;
            }

            curY += EditFieldHeight + 10f;

            var iconSelectorRect = inRect;
            iconSelectorRect.yMax -= 4f;
            iconSelectorRect.yMin = curY;

            DoIconSelector(iconSelectorRect);
            if (Widgets.ButtonText(new Rect(0.0f, rect.height - ButSize.y, ButSize.x, ButSize.y), "Reset".Translate()))
            {
                config.Reset();
                Close();
            }

            if (!Widgets.ButtonText(
                    new Rect(InitialSize.x - ButSize.x - Margin * 2, rect.height - ButSize.y, ButSize.x, ButSize.y),
                    "DoneButton".Translate())) return;
            TryAccept();
        }

        private void DoIconSelector(Rect mainRect)
        {
            var num1 = 50;
            var viewRect = new Rect(0.0f, 0.0f, mainRect.width - 16f, viewHeight);
            Widgets.BeginScrollView(mainRect, ref scrollPos, viewRect);
            var allPaths = GetAvaibleIcons();
            var num2 = Mathf.FloorToInt(viewRect.width / (num1 + 5));
            var num3 = allPaths.Count();
            var num4 = 0;
            foreach (var iconPath in allPaths)
            {
                if (iconPath != null && !cacheIcons.ContainsKey(iconPath))
                    cacheIcons.Add(iconPath, ContentFinder<Texture2D>.Get(iconPath));
                var num5 = num4 / num2;
                var num6 = num4 % num2;
                var num7 = num4 >= num3 - num3 % num2 ? num3 % num2 : num2;
                var rect = new Rect(
                    (float)((viewRect.width - (double)(num7 * num1) - (num7 - 1) * 5) / 2.0) + num6 * num1 + num6 * 5,
                    num5 * num1 + num5 * 5, num1, num1);
                Widgets.DrawLightHighlight(rect);
                Widgets.DrawHighlightIfMouseover(rect);
                if (iconPath == config.IconPath)
                    Widgets.DrawBox(rect);
                if (iconPath != null && cacheIcons[iconPath] != null)
                    GUI.DrawTexture(new Rect(rect.x + 5f, rect.y + 5f, 40f, 40f), cacheIcons[iconPath]);
                GUI.color = Color.white;
                if (Widgets.ButtonInvisible(rect))
                {
                    config.IconPath = iconPath;
                    SoundDefOf.Tick_High.PlayOneShotOnCamera();
                }

                viewHeight = Mathf.Max(viewHeight, rect.yMax);
                ++num4;
            }

            GUI.color = Color.white;
            Widgets.EndScrollView();
        }

        public static void InitializeIconsPathCache()
        {
            if (cacheIconsPath == null)
            {
                cacheIconsPath = new List<string>();
                foreach (var button in DefDatabase<MainButtonDef>.AllDefs)
                    if (button.iconPath != null)
                        cacheIconsPath.Add(button.iconPath);
                foreach (var icon in DefDatabase<MainIconDef>.AllDefs) cacheIconsPath.Add(icon.path);
            }
        }

        private IEnumerable<string> GetAvaibleIcons()
        {
            InitializeIconsPathCache();
            yield return null;
            foreach (var path in cacheIconsPath) yield return path;
        }

        private void TryAccept()
        {
            if (!config.Label.NullOrEmpty())
                config.Label = config.Label.Trim();
            Close();
        }
    }
}