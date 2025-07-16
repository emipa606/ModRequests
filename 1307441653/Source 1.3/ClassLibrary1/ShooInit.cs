using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using HarmonyLib;
using System.Reflection;

namespace Shoo
{
    [StaticConstructorOnStartup]
    internal static class ShooInit
    {
    static ShooInit()
        {
            Harmony harmonyInstance = new Harmony("jamaicancastle.shoo");
            harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
