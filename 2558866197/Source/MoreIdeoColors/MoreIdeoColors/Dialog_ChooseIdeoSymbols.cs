using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RimWorld;

namespace MoreIdeoColors
{
    [HarmonyPatch(typeof(Dialog_ChooseIdeoSymbols))]
    [HarmonyPatch("InitialSize", MethodType.Getter)]
    class Dialog_ChooseIdeoSymbolsFix
    {
        public static
            bool Prefix(ref Vector2 __result)
        {
            __result = new Vector2(740f, 700f);
            return false;
            
        }
    }
}
