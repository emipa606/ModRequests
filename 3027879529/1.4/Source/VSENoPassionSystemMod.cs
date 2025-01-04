using HarmonyLib;
using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;
using VSE;
using VSE.Passions;

namespace VSENoPassionSystem
{
    public class VSENoPassionSystemMod : Mod
    {
        public VSENoPassionSystemMod(ModContentPack pack) : base(pack)
        {
			new Harmony("VSENoPassionSystemMod").PatchAll();
        }
    }

    //[HarmonyPatch(typeof(PassionPatches), "Do")]
    //public static class PassionPatches_Do_Patch
    //{
    //    public static bool Prefix()
    //    {
    //        return false;
    //    }
    //}

    [HarmonyPatch(typeof(RandomPlusPatches), "Do")]
    public static class RandomPlusPatches_Do_Patch
    {
        public static bool Prefix()
        {
            return false;
        }
    }
}
