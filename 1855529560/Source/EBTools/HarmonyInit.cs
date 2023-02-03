using RimWorld;
using Verse;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EBTools
{
    [StaticConstructorOnStartup]
    internal static class HarmonyInit
    {
    static HarmonyInit()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("EBToolsHarmony");
           // harmony.PatchAll(Assembly.GetExecutingAssembly());
           harmony.PatchAll();
        }
    }    
    
}
