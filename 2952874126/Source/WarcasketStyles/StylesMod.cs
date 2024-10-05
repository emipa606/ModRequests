using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;
using HarmonyLib;
using System.Linq;
using System;

namespace WarcasketStyles
{
    public class StylesMod : Mod
    {
        public StylesMod(ModContentPack content) : base(content)
        {
            Harmony harmony = new Harmony("com.canonwarcasketstyles.patches");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
