using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Unity;
using HarmonyLib;
using Steamworks;

namespace Psychic_Coiling_VRE_Addon
{
    [StaticConstructorOnStartup]
    public class Main
    {
        static Main()
        {
            var harmony = new HarmonyLib.Harmony("com.Psychic_Coiling_VRE_Addon");
            harmony.PatchAll();
        }
    }
}