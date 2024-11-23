using RimWorld;
using HarmonyLib;
using Verse;
using System.Collections.Generic;

namespace HDream
{
    public class WishCompProperties_Master : WishCompProperties
    {
        public List<WishDef> subWish;

        public int subFulfillNeeded = -1; 

        public WishCompProperties_Master()
        {
            compClass = typeof(WishComp_Master);
        }
    }
}
