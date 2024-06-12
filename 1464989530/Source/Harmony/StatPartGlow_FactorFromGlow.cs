#if HARM12
using Harmony;
#else
using HarmonyLib;
#endif
using RimWorld;
using System.Diagnostics;
using Verse;

namespace NightVision.Harmony
{
    [HarmonyPatch(typeof(StatPart_Glow), "FactorFromGlow")]
    public static class StatPartGlow_FactorFromGlow
    {
#if DEBUG
        public static double TotalGlFactorNanoSec;
        public static long TotalTicks;
        private static readonly Stopwatch GlfactorTimer = new Stopwatch();
        private static int GlfactorTicks;
#endif
        public static void Postfix(Thing t, ref float __result)
        {
#if DEBUG
            GlfactorTimer.Start();
#endif
            if (t is Pawn pawn && pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision comp)
            {
                float glowat = pawn.Map.glowGrid.GameGlowAt(pawn.Position);
                __result = comp.FactorFromGlow(glowat);
            }

#if DEBUG
            GlfactorTimer.Stop();

            if (Find.TickManager.TicksGame - GlfactorTicks > 600)
            {
                int elapsedTicks = Find.TickManager.TicksGame - GlfactorTicks;
                GlfactorTicks = Find.TickManager.TicksGame;
                TotalGlFactorNanoSec += GlfactorTimer.ElapsedMilliseconds * 1000000;
                TotalTicks += elapsedTicks;
                GlfactorTimer.Reset();
            }
#endif
        }
    }
}