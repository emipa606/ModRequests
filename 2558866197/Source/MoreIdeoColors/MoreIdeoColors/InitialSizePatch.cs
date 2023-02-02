using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using HarmonyLib;
using System.Reflection;
using Verse;
using Verse.Sound;

namespace MoreIdeoColors
{
    [HarmonyPatch(typeof(Dialog_StylingStation))]
    [HarmonyPatch("InitialSize", MethodType.Getter)]
    class InitialSizePatch
    {
        static bool Prefix(ref Vector2 __result)
        {
            __result = new Vector2(1075f, 750f);
            return false;
        }
    }
}
