using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using Verse;
using System;
using System.Text;
using System.Collections;
using Verse.Sound;

namespace TTPF
{
    [HarmonyPatch(typeof(Root_Play), nameof(Root_Play.Start))]
    internal class Patch_RootPlay_Start
    {
        internal static void Postfix()
        {
            foreach (var customTab in TTPF_Mod.settings.customResearchTabs)
            {
#if DEBUG
                TTPF.Warning(string.Format("Loading Custom {0}", customTab.researchDefName));
#endif
                var researchDef = DefDatabase<ResearchProjectDef>.GetNamed(customTab.researchDefName, false);
                if (researchDef != null)
                {
                    researchDef.tab = DefDatabase<ResearchTabDef>.GetNamed(customTab.researchTabDefName, false);
                    researchDef.researchViewX = customTab.researchViewX;
                    researchDef.researchViewY = customTab.researchViewY;

                    Traverse.Create(researchDef).Field("x").SetValue(customTab.researchViewX);
                    Traverse.Create(researchDef).Field("y").SetValue(customTab.researchViewY);
#if DEBUG
                    TTPF.Warning(string.Format("Set at X:{0} - Y:{1}", customTab.researchViewX, customTab.researchViewY));
                    TTPF.Warning(string.Format("It is at X:{0} - Y:{1}", researchDef.ResearchViewX, researchDef.ResearchViewY));
#endif
                }
            }
        }
    }
}
