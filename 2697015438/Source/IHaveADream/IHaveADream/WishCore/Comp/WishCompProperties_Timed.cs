using RimWorld;
using HarmonyLib;
using Verse;

namespace HDream
{
    public class WishCompProperties_Timed : WishCompProperties
    {
        public float daysToHold;

        public bool resetTimerOnFailHold = false;

        public int removePendingOnHoldOffset = 0;
        public float removePendingOnHoldPercent = 0;

        public float removePendingPerTickFactor = 1;

        public WishCompProperties_Timed()
        {
            compClass = typeof(WishComp_Timed);
        }
    }
}
