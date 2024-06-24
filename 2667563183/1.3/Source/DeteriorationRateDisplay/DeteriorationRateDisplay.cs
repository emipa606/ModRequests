using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using HarmonyLib;

namespace DRD
{
    [StaticConstructorOnStartup]
    public static class DeteriorationRateDisplay
    {
        static DeteriorationRateDisplay()
        {
            var harmony = new Harmony("UdderlyEvelyn.DeteriorationRateDisplay");
            harmony.PatchAll();
        }
    }
}
