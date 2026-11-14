using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UINotIncluded.Widget.Configs;
using Verse;

namespace UINotIncluded.Widget
{
    [StaticConstructorOnStartup]
    public static class WidgetManager
    {
        public const string ALL = "Widgets and Buttons";
        public const string BUTTONS = "Available Buttons";
        public const string WIDGETS = "Available Widgets";

        public static readonly Dictionary<string, Func<IEnumerable<ElementConfig>>> availableGetters;
        public static readonly Dictionary<string, Func<IEnumerable<ButtonConfig>>> buttonGetters;

        private static string selectedGetterName;
        private static Func<IEnumerable<ElementConfig>> selectedGetterFunction;

        static WidgetManager()
        {
            availableGetters = new Dictionary<string, Func<IEnumerable<ElementConfig>>>();
            buttonGetters = new Dictionary<string, Func<IEnumerable<ButtonConfig>>>();
            AddButtonGetter(BUTTONS, AllMainButtons);
            AddGetter(WIDGETS, AllWidgets);
            AddGetter(ALL, AllElements);
        }

        public static IEnumerable<ButtonConfig> MainTabButtons
        {
            get
            {
                foreach (var button in DefDatabase<MainButtonDef>.AllDefs.OrderBy(def => def.order))
                {
                    if (button.defName == "Inspect") continue;
                    var config = new ButtonConfig(button);
                    config.RefreshIcon();
                    yield return config;
                }
            }
        }

        public static string SelectedGetterName
        {
            get
            {
                if (selectedGetterName == null) SelectGetter(ALL);
                return selectedGetterName;
            }
        }

        public static IEnumerable<ElementConfig> AvailableSelectedWidgets(bool allowAlreadyOnBars = false)
        {
            if (selectedGetterFunction == null) SelectGetter(ALL);

            foreach (var config in selectedGetterFunction())
            {
                if (!allowAlreadyOnBars && !config.Repeatable &&
                    (Settings.TopBarElements.Exists(e => e.Equivalent(config)) ||
                     Settings.BottomBarElements.Exists(e => e.Equivalent(config)))) continue;
                yield return config;
            }
        }

        public static void SelectGetter(string name)
        {
            if (!availableGetters.ContainsKey(name))
            {
                UINI.Error("Invalid WidgetGetter name");
                return;
            }

            selectedGetterName = name;
            selectedGetterFunction = availableGetters[name];
        }

        public static IEnumerable<ButtonConfig> AllMainButtons()
        {
            foreach (var config in MainTabButtons) yield return config;
        }

        public static IEnumerable<ElementConfig> AllWidgets()
        {
            foreach (var widgetDef in DefDatabase<WidgetDef>.AllDefs)
            {
                var config = widgetDef.GetNewConfig();
                yield return config;
            }
        }

        public static IEnumerable<ElementConfig> AllElements()
        {
            foreach (ElementConfig config in AllMainButtons()) yield return config;
            foreach (var config in AllWidgets()) yield return config;
        }

        public static void AddGetter(string name, Func<IEnumerable<ElementConfig>> getterFunction)
        {
            availableGetters[name] = getterFunction;
        }

        public static void AddButtonGetter(string name, Func<IEnumerable<ButtonConfig>> getterFunction)
        {
            buttonGetters[name] = getterFunction;
            AddGetter(name, getterFunction);
        }
    }
}