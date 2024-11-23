using RimWorld;
using HarmonyLib;
using Verse;

namespace HDream
{
    public class WishCompProperties_Sub : WishCompProperties
    {
        public bool preventWishMood = true;
        public WishCompProperties_Sub()
        {
            compClass = typeof(WishComp_Sub);
        }
    }
}
