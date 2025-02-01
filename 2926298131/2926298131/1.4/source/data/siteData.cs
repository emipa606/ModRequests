using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using HarmonyLib;
using UnityEngine;
using RimWorld.Planet;

namespace LostTechnology
{
    public class siteData : IExposable
    {
        private Site s;
        public int durationInTicks;
        // Data
        public List<ThingDef> ar_td = new List<ThingDef>();

        // Data Save
        public void ExposeData()
        {
            Scribe_Values.Look(ref durationInTicks, "durationInTicks");
            Scribe_Collections.Look(ref ar_td, $"ar_td", LookMode.Def);
        }

        public void setParent(Site _s)
        {
            s = _s;
        }
    }

    [HarmonyPatch(typeof(Site), nameof(Site.Tick))]
    public static class Site_Tick_Patch
    {
        public static void Postfix(Site __instance)
        {
            if (!__instance.HasMap)
            {
                var data = dataUtility.GetData(__instance);
                if (data.durationInTicks > 0 && Find.TickManager.TicksGame >= data.durationInTicks)
                {
                    __instance.Destroy();
                }
            }
        }
    }
}
