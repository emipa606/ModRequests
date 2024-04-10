using HarmonyLib;
using UnityEngine;
using RimWorld;
using Verse;

namespace UglyTogether
{
    public class UglyTogetherMod : Mod
    {
        public UglyTogetherMod(ModContentPack content) : base(content)
        {
            Log.Message("[UglyTogether]: Initialising Harmony");
            new Harmony(this.Content.PackageIdPlayerFacing).PatchAll();
        }
    }
}
