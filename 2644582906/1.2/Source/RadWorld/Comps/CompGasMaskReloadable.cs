using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace RadWorld
{

	public class CompGasMaskReloadable : CompReloadable
	{
        private int tickUntilWornOut;

        private const int filterDuration = GenDate.TicksPerDay;

        [HarmonyPatch(typeof(CompReloadable), "ReloadFrom")]
        internal static class Patch_ReloadFrom
        {
            private static void Postfix(CompReloadable __instance)
            {
                if (__instance is CompGasMaskReloadable compGasMaskReloadable)
                {
                    compGasMaskReloadable.PostReload();
                }
            }
        }
        private void PostReload()
        {
            tickUntilWornOut = filterDuration;
        }

        public override void CompTick()
        {
            base.CompTick();
            tickUntilWornOut--;
            if (tickUntilWornOut <= 0)
            {
                this.UsedOnce();
                if (this.RemainingCharges > 0)
                {
                    tickUntilWornOut = filterDuration;
                }
            }
        }

        public int GetRemainingHours()
        {
            return (tickUntilWornOut + ((this.RemainingCharges - 1) * filterDuration));
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref tickUntilWornOut, "tickUntilWornOut");
        }
    }
}