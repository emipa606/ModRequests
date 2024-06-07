using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using HarmonyLib;

namespace ForceXenogermImlpantation
{
    [StaticConstructorOnStartup]
    static class LoadHarmony
    {
        static LoadHarmony()
        {
            Harmony harmony = new Harmony("zed_0xff.ForceXenogermImlpantation");

            harmony.PatchAll();
        }
    }
}
