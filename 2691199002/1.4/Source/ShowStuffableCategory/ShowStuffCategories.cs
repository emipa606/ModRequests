using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using HarmonyLib;

namespace SSC
{
    [StaticConstructorOnStartup]
    public static class ShowStuffCategories
    {
        static ShowStuffCategories()
        {
            var harmony = new Harmony("UdderlyEvelyn.ShowStuffCategories");
            harmony.PatchAll();
        }
    }
}
