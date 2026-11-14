using System;
using System.Collections.Generic;
using System.Linq;
using UINotIncluded.Widget;
using UINotIncluded.Widget.Configs;
using UnityEngine;
using Verse;

namespace UINotIncluded
{
    public class Settings : ModSettings
    {
        public static bool designationsOnLeft;
        public static GameFont fontSize;
        private static List<string> hiddenDesignations;
        public static bool initializedDefaultBar;
        private static List<string> leftDesignations;
        private static List<string> mainDesignations;
        private static List<string> rightDesignations;
        public static bool togglersOnTop;
        public static bool useDesignatorBar;
        public static bool settingsChecked;
        public static bool centeredWindows;

        public static bool vanillaReadout = true;
        public static bool vanillaControlSpeed = true;
        public static bool vanillaDate = true;
        public static bool vanillaRealtime = true;
        public static bool vanillaWeather = true;
        public static bool vanillaTemperature = true;

        public static bool forceCustomTab = true;

        public static float barsHeight = 35f;

        public static string lastVersion;

        private static readonly Dictionary<string, Designator> _availableDesignators =
            new Dictionary<string, Designator>();

        private static List<ElementConfig> topBar;
        private static List<ElementConfig> bottomBar;

        private static BarStyle _barStyle;
        private static Type _barStyleType;

        private static List<Designator> CachedDesignators;

        public static bool TabsOnTop => TopBarElements.Any();
        public static bool TabsOnBottom => BottomBarElements.Any();

        public static BarStyle BarStyle
        {
            get
            {
                if (_barStyle == null)
                {
                    if (_barStyleType == null) _barStyleType = typeof(BarStyle_VanillaBluePlus);
                    _barStyle = (BarStyle)Activator.CreateInstance(_barStyleType);
                }

                return _barStyle;
            }

            set
            {
                _barStyle = value;
                _barStyleType = value.GetType();
            }
        }

        public static List<ElementConfig> TopBarElements => topBar ?? (topBar = new List<ElementConfig>());

        public static List<ElementConfig> BottomBarElements => bottomBar ?? (bottomBar = new List<ElementConfig>());

        public static List<Designator>[] GetDesignationConfigs()
        {
            var arrayDesignatorConfigs = new List<Designator>[4];

            for (var i = 0; i < 4; i++) arrayDesignatorConfigs[i] = GetDesignationList((DesignationConfig)i);
            return arrayDesignatorConfigs;
        }

        private static List<Designator> GetDesignationList(DesignationConfig list)
        {
            switch (list)
            {
                case DesignationConfig.hidden:
                    return StringToDesignation(hiddenDesignations);

                case DesignationConfig.left:
                    return StringToDesignation(leftDesignations);

                case DesignationConfig.main:
                    return StringToDesignation(mainDesignations);

                case DesignationConfig.right:
                    return StringToDesignation(rightDesignations);

                default:
                    throw new NotImplementedException();
            }
        }

        private static void InitializeDesignators()
        {
            if (_availableDesignators.Count() != 0) return;


            if (CachedDesignators == null)
            {
                CachedDesignators = new List<Designator>();
                foreach (var designator in
                         DefDatabase<DesignationCategoryDef>.GetNamed("Orders").AllResolvedDesignators)
                    CachedDesignators.Add(designator);
            }

            foreach (var designator in CachedDesignators)
                _availableDesignators[designator.GetType().ToString()] = designator;

            foreach (var key in _availableDesignators.Keys)
                if (!(leftDesignations.Contains(key) || mainDesignations.Contains(key) ||
                      rightDesignations.Contains(key) || hiddenDesignations.Contains(key)))
                    hiddenDesignations.Add(key);
        }

        public static void RestoreDesignationLists()
        {
            SetDefaultList(DesignationConfig.hidden);
            SetDefaultList(DesignationConfig.left);
            SetDefaultList(DesignationConfig.main);
            SetDefaultList(DesignationConfig.right);
            _availableDesignators.Clear();
            InitializeDesignators();
            DesignatorManager.Pull();
            UINI.Log("DesignationConfigs default restored.");
        }

        public static void RestoreDefaultMainBar()
        {
            TopBarElements.Clear();
            BottomBarElements.Clear();

            foreach (var element in WidgetManager.AllMainButtons()) BottomBarElements.Add(element);
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref togglersOnTop, "togglersOnTop", true);
            Scribe_Values.Look(ref designationsOnLeft, "designationsOnLeft");
            Scribe_Values.Look(ref centeredWindows, "centeredWindows");

            Scribe_Values.Look(ref settingsChecked, "settingsChecked");

            Scribe_Values.Look(ref vanillaReadout, "vanillaReadout");
            Scribe_Values.Look(ref vanillaControlSpeed, "vanillaControlSpeed");
            Scribe_Values.Look(ref vanillaDate, "vanillaDate");
            Scribe_Values.Look(ref vanillaRealtime, "vanillaRealtime");
            Scribe_Values.Look(ref vanillaWeather, "vanillaWeather");
            Scribe_Values.Look(ref vanillaTemperature, "vanillaTemperature");

            Scribe_Values.Look(ref useDesignatorBar, "useDesignatorBar", true);
            Scribe_Values.Look(ref initializedDefaultBar, "initializedDefaultBar");
            Scribe_Values.Look(ref fontSize, "fontSize");

            Scribe_Values.Look(ref _barStyleType, "StyleType", typeof(BarStyle_RustyOrange));

            Scribe_Collections.Look(ref hiddenDesignations, "hiddenDesignations", LookMode.Value);
            Scribe_Collections.Look(ref leftDesignations, "leftDesignations", LookMode.Value);
            Scribe_Collections.Look(ref mainDesignations, "mainDesignations", LookMode.Value);
            Scribe_Collections.Look(ref rightDesignations, "rightDesignations", LookMode.Value);

            Scribe_Collections.Look(ref topBar, "top", LookMode.Deep);
            Scribe_Collections.Look(ref bottomBar, "bottom", LookMode.Deep);

            Scribe_Values.Look(ref lastVersion, "lastVersion");
            Scribe_Values.Look(ref barsHeight, "barsHeight", 35f);

            Scribe_Values.Look(ref forceCustomTab, "forceCustomTab", true);

            base.ExposeData();
        }

        private static void SetDefaultList(DesignationConfig designationList)
        {
            switch (designationList)
            {
                case DesignationConfig.hidden:
                    hiddenDesignations = new List<string>();
                    return;

                case DesignationConfig.left:
                    leftDesignations = new List<string>
                    {
                        "RimWorld.Designator_Forbid", "RimWorld.Designator_Uninstall", "RimWorld.Designator_Unforbid",
                        "RimWorld.Designator_Claim"
                    };
                    break;

                case DesignationConfig.main:
                    mainDesignations = new List<string>
                    {
                        "RimWorld.Designator_Mine", "RimWorld.Designator_PlantsHarvestWood",
                        "RimWorld.Designator_PlantsHarvest", "RimWorld.Designator_Cancel",
                        "RimWorld.Designator_Deconstruct"
                    };
                    break;

                case DesignationConfig.right:
                    rightDesignations = new List<string>
                    {
                        "RimWorld.Designator_Haul", "RimWorld.Designator_SmoothSurface",
                        "RimWorld.Designator_Slaughter", "RimWorld.Designator_PlantsCut", "RimWorld.Designator_Hunt",
                        "RimWorld.Designator_Tame"
                    };
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        private static List<Designator> StringToDesignation(List<string> classes)
        {
            InitializeDesignators();
            var result = new List<Designator>();

            foreach (var designator_class in classes)
                if (_availableDesignators.TryGetValue(designator_class, out var designator))
                    result.Add(designator);

            return result;
        }

        public static void UpdateDesignations(List<Designator> designations, DesignationConfig list)
        {
            switch (list)
            {
                case DesignationConfig.hidden:
                    UpdateList(designations, hiddenDesignations);
                    break;

                case DesignationConfig.left:
                    UpdateList(designations, leftDesignations);
                    break;

                case DesignationConfig.main:
                    UpdateList(designations, mainDesignations);
                    break;

                case DesignationConfig.right:
                    UpdateList(designations, rightDesignations);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        private static void UpdateList(List<Designator> designations, List<string> names)
        {
            names.Clear();
            foreach (var designation in designations) names.Add(designation.GetType().ToString());
        }
    }

    internal class UINI_Mod : Mod
    {
        public static Settings settings;
        private readonly SettingPages settingPages = new SettingPages();

        public UINI_Mod(ModContentPack content) : base(content)
        {
            settings = GetSettings<Settings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Settings.settingsChecked = true;
            var pageTittleHeight = 26f;
            var pageTittleWidth = (float)Math.Floor(inRect.width / 3);

            var contentRect = new Rect(inRect.x, inRect.y + pageTittleHeight, inRect.width,
                inRect.height - pageTittleHeight - 30f);

            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(
                new Rect(inRect.x + pageTittleWidth + pageTittleHeight, inRect.y,
                    pageTittleWidth - 2 * pageTittleHeight, pageTittleHeight), settingPages.label);
            if (Widgets.ButtonImage(new Rect(inRect.x + pageTittleWidth, inRect.y, pageTittleHeight, pageTittleHeight),
                    ModTextures.chevronLeft)) settingPages.Prev();
            if (Widgets.ButtonImage(
                    new Rect(inRect.x + 2 * pageTittleWidth - pageTittleHeight, inRect.y, pageTittleHeight,
                        pageTittleHeight), ModTextures.chevronRight)) settingPages.Next();
            Text.Anchor = TextAnchor.UpperLeft;

            settingPages.DoPage(contentRect);
            base.DoSettingsWindowContents(inRect);
        }

        public override void WriteSettings()
        {
            settingPages.cacheAvailableElements = null;
            base.WriteSettings();
        }

        public override string SettingsCategory()
        {
            return UINI.Name;
        }
    }

    internal class SettingPages
    {
        private readonly List<Page> pages;
        public List<ElementConfig> cacheAvailableElements;
        private int current;
        public string label;

        public SettingPages()
        {
            pages = new List<Page>
            {
                new Page
                {
                    label = "General",
                    action = DoGeneralPage
                },
                new Page
                {
                    label = "Designator",
                    action = DoDesignatorPage
                },
                new Page
                {
                    label = "Toolbars",
                    action = DoToolbarDraggables
                }
            };

            current = 0;
            label = pages[current].label;
        }

        public void DoPage(Rect inRect)
        {
            pages[current].action(inRect);
        }

        public void Next()
        {
            current++;
            if (current == pages.Count()) current = 0;
            label = pages[current].label;
        }

        public void Prev()
        {
            current--;
            if (current < 0) current = pages.Count() - 1;
            label = pages[current].label;
        }

        private void DoGeneralPage(Rect inRect)
        {
            var columnWidth = inRect.width / 2;
            var height = inRect.height;
            var column1 = new Rect(columnWidth - columnWidth / 2, inRect.y, columnWidth, height).ContractedBy(2f);
            var listingStandard = new Listing_Standard();

            listingStandard.Begin(column1);
            listingStandard.CheckboxLabeled("UINotIncluded.Setting.togglersOnTop".Translate(),
                ref Settings.togglersOnTop, "UINotIncluded.Setting.togglersOnTop.Description".Translate());
            listingStandard.CheckboxLabeled("UINotIncluded.Setting.centeredWindows".Translate(),
                ref Settings.centeredWindows, "UINotIncluded.Setting.centeredWindows.Description".Translate());
            listingStandard.CheckboxLabeled("UINotIncluded.Setting.forceCustomTab".Translate(),
                ref Settings.forceCustomTab, "UINotIncluded.Setting.forceCustomTab.Description".Translate());

            listingStandard.CheckboxLabeled("UINotIncluded.Setting.vanillaReadout".Translate(),
                ref Settings.vanillaReadout, "UINotIncluded.Setting.vanillaReadout.Description".Translate());
            listingStandard.CheckboxLabeled("UINotIncluded.Setting.vanillaRealtime".Translate(),
                ref Settings.vanillaRealtime, "UINotIncluded.Setting.vanillaRealtime.Description".Translate());
            listingStandard.CheckboxLabeled("UINotIncluded.Setting.vanillaWeather".Translate(),
                ref Settings.vanillaWeather, "UINotIncluded.Setting.vanillaWeather.Description".Translate());
            listingStandard.CheckboxLabeled("UINotIncluded.Setting.vanillaTemperature".Translate(),
                ref Settings.vanillaTemperature, "UINotIncluded.Setting.vanillaTemperature.Description".Translate());

            listingStandard.CheckboxLabeled("UINotIncluded.Setting.vanillaDate".Translate(), ref Settings.vanillaDate,
                "UINotIncluded.Setting.vanillaDate.Description".Translate());
            listingStandard.CheckboxLabeled("UINotIncluded.Setting.vanillaControlSpeed".Translate(),
                ref Settings.vanillaControlSpeed, "UINotIncluded.Setting.vanillaControlSpeed.Description".Translate());


            listingStandard.Gap();

            if (listingStandard.ButtonTextLabeled("UINotIncluded.Setting.barHeight".Translate(Settings.barsHeight),
                    "Default".Translate()))
            {
                Settings.barsHeight = 35f;
                if (Find.CurrentMap != null) Find.ColonistBar.MarkColonistsDirty();
            }

            var newHeight = listingStandard.Slider(Settings.barsHeight, 25f, 50f);
            if (newHeight != Settings.barsHeight)
            {
                Settings.barsHeight = newHeight;
                if (Find.CurrentMap != null) Find.ColonistBar.MarkColonistsDirty();
            }

            if (listingStandard.ButtonTextLabeled("UINotIncluded.Setting.fontSize".Translate(),
                    Settings.fontSize.ToString()))
            {
                var options = new List<FloatMenuOption>();
                foreach (GameFont font in Enum.GetValues(typeof(GameFont)))
                    options.Add(new FloatMenuOption(font.ToString(), () => { Settings.fontSize = font; }));
                Find.WindowStack.Add(new FloatMenu(options));
            }

            if (listingStandard.ButtonTextLabeled("UINotIncluded.Setting.style".Translate(), Settings.BarStyle.Name))
            {
                var options = new List<FloatMenuOption>();
                foreach (var styleType in new[]
                         {
                             typeof(BarStyle_RustyOrange), typeof(BarStyle_VanillaBlue),
                             typeof(BarStyle_VanillaBluePlus)
                         })
                {
                    var style = (BarStyle)Activator.CreateInstance(styleType);
                    options.Add(new FloatMenuOption(style.Name, () => Settings.BarStyle = style));
                }

                Find.WindowStack.Add(new FloatMenu(options));
            }

            listingStandard.End();
        }

        private void DoDesignatorPage(Rect inRect)
        {
            var columnWidth = inRect.width / 2;
            var height = 30f;
            var column1 = new Rect(inRect.x, inRect.y, columnWidth, height).ContractedBy(2f);
            var column2 = new Rect(inRect.x + columnWidth, inRect.y, columnWidth, height).ContractedBy(2f);
            var listingStandard = new Listing_Standard();
            listingStandard.Begin(column1);
            listingStandard.CheckboxLabeled("UINotIncluded.Setting.useDesignatorBar".Translate(),
                ref Settings.useDesignatorBar, "UINotIncluded.Setting.useDesignatorBar.Description".Translate());
            listingStandard.End();
            listingStandard.Begin(column2);
            listingStandard.CheckboxLabeled("UINotIncluded.Setting.designationsOnLeft".Translate(),
                ref Settings.designationsOnLeft, "UINotIncluded.Setting.designationsOnLeft.Description".Translate());
            listingStandard.End();
            DoJobBarConfigurationWidget(new Rect(inRect.x, inRect.y + height, inRect.width, inRect.height - height));
        }

        private void DoJobBarConfigurationWidget(Rect rect)
        {
            var columnWidth = rect.width / 4;
            var curY = rect.y;
            var designation_lists = DesignatorManager.GetDesignationConfigs();

            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(
                new Rect(rect.x + (float)Math.Floor(rect.width / 4), curY, (float)Math.Floor(rect.width / 2), 25f),
                new GUIContent("UINotIncluded.Setting.DesignatorBar.Description".Translate()));
            Text.Anchor = TextAnchor.UpperLeft;
            curY += 25f;
            if (Widgets.ButtonText(
                    new Rect(rect.x + (float)Math.Floor(rect.width / 4), curY, (float)Math.Floor(rect.width / 2), 25f),
                    "Restore to default")) Settings.RestoreDesignationLists();
            curY += 25f;

            var manager = new DragManager<Designator>(
                DesignatorManager.Push,
                designator => designator.defaultLabel);

            DragMemory.hoveringOver = null;
            for (var i = 0; i < 4; i++)
                CustomLists.Draggable(((DesignationConfig)i).ToStringHuman(),
                    new Rect(rect.x + columnWidth * i, curY, columnWidth, rect.height - 25f).ContractedBy(3f),
                    designation_lists[i], designator => designator.defaultLabel, manager);
            manager.Update();
        }

        private void DoToolbarDraggables(Rect rect)
        {
            var columnWidth = rect.width / 3;
            var curY = rect.y;

            if (cacheAvailableElements == null)
                cacheAvailableElements = WidgetManager.AvailableSelectedWidgets().ToList();

            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(
                new Rect(rect.x + (float)Math.Floor(rect.width / 4), curY, (float)Math.Floor(rect.width / 2), 25f),
                new GUIContent("UINotIncluded.Setting.Toolbars.Description".Translate()));
            Text.Anchor = TextAnchor.UpperLeft;
            curY += 25f;
            if (Widgets.ButtonText(
                    new Rect(rect.x + (float)Math.Floor(rect.width / 4), curY, (float)Math.Floor(rect.width / 2), 25f),
                    "Restore to default"))
            {
                Settings.RestoreDefaultMainBar();
                UpdateCache();
            }

            curY += 25f;

            var manager = new DragManager<ElementConfig>(
                UpdateCache,
                element => element.SettingLabel,
                element =>
                {
                    if (element.Configurable) return element.Worker.OpenConfigWindow;
                    return null;
                });

            var selectTypeRect = new Rect(rect.x, curY, columnWidth, 30f);
            if (Widgets.ButtonText(selectTypeRect.ContractedBy(2f), WidgetManager.SelectedGetterName))
            {
                var options = new List<FloatMenuOption>();
                foreach (var widgetGetter in WidgetManager.availableGetters.Keys)
                    options.Add(new FloatMenuOption(widgetGetter, () =>
                    {
                        WidgetManager.SelectGetter(widgetGetter);
                        UpdateCache();
                    }));
                Find.WindowStack.Add(new FloatMenu(options));
            }

            DragMemory.hoveringOver = null;

            CustomLists.Draggable("Available",
                new Rect(rect.x, curY + selectTypeRect.height, columnWidth, rect.height - 25f - selectTypeRect.height)
                    .ContractedBy(3f), cacheAvailableElements, element => element.SettingLabel, manager, false);
            CustomLists.Draggable("Top Bar",
                new Rect(rect.x + columnWidth, curY, columnWidth, rect.height - 25f).ContractedBy(3f),
                Settings.TopBarElements, element => element.SettingLabel, manager);
            CustomLists.Draggable("Bottom Bar",
                new Rect(rect.x + columnWidth * 2, curY, columnWidth, rect.height - 25f).ContractedBy(3f),
                Settings.BottomBarElements, element => element.SettingLabel, manager);

            manager.Update();
        }

        private void UpdateCache()
        {
            cacheAvailableElements.Clear();
            foreach (var element in WidgetManager.AvailableSelectedWidgets()) cacheAvailableElements.Add(element);
        }

        private struct Page
        {
            public Action<Rect> action;
            public string label;
        }
    }
}