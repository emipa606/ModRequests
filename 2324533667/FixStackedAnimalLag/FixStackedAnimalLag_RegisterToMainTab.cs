using HarmonyLib;
using ModButtons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace FixStackedAnimalLag
{

    [HarmonyPatch(typeof(MainTabWindow_ModButtons))]
    [HarmonyPatch("DoWindowContents")]
    class Patch_MainTabWindow_ModButtons_DoWindowContents
    {
        static void Prefix(MainTabWindow_ModButtons __instance, ref Rect canvas)
        {
            FixStackedAnimalLag_RegisterToMainTab.ensureMainTabRegistered();
        }
    }

    class FixStackedAnimalLag_RegisterToMainTab
    {

        public static bool wasRegistered = false;

        public static void ensureMainTabRegistered()
        {
            if (wasRegistered) { return; }

            Log.Message("Hello from FixStackedAnimalLag_RegisterToMainTab ensureMainTabRegistered");

            List<List<ModButton_Text>> columns = MainTabWindow_ModButtons.columns;

            List<ModButton_Text> buttons = new List<ModButton_Text>();

            buttons.Add(new ModButton_Text(
                delegate
                {
                    string buttonLabel = "Enforcing Enemy Collision Currently:" + Environment.NewLine;
                    if (FixStackedAnimalLag_GlobalRuntimeSettings.shouldCollideEnemies)
                    {
                        buttonLabel += "ENABLED";
                    }
                    else
                    {
                        buttonLabel += "DISABLED";
                    }
                    return buttonLabel;
                },
                delegate {
                    FixStackedAnimalLag_GlobalRuntimeSettings.shouldCollideEnemies = !FixStackedAnimalLag_GlobalRuntimeSettings.shouldCollideEnemies;
                }
            ));

            columns.Add(buttons);

            wasRegistered = true;
        }
    }
}
