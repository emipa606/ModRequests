using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using HarmonyLib;

namespace SA
{
    [StaticConstructorOnStartup]
    public static class SensibleAiming
    {
        static SensibleAiming()
        {
            var harmony = new Harmony("UdderlyEvelyn.SensibleAiming");
            harmony.PatchAll();
        }
    }
}
