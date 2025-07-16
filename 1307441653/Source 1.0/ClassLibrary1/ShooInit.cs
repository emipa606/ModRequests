using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Harmony;
using System.Reflection;

namespace Shoo
{
    [StaticConstructorOnStartup]
    internal static class ShooInit
    {
    static ShooInit()
        {
            HarmonyInstance harmonyInstance = HarmonyInstance.Create("jamaicancastle.shoo");
            harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
