using HarmonyLib;
using RimWorld;
using Verse;

namespace MoreMechanoids.Harmony
{
    /// <summary>
    /// Don't spawn Mechanoid sapper attacks all the time when wealth is high
    /// </summary>
    public class RaidStrategyWorker_ImmediateAttackSappers_Patch
    {
        [HarmonyPatch(typeof(RaidStrategyWorker_ImmediateAttackSappers), nameof(RaidStrategyWorker_ImmediateAttackSappers.CanUsePawn))]
        public class CanUsePawn
        {
            internal static void Postfix(float pointsTotal, Pawn p, ref bool __result)
            {
                if (!__result) return;
                if (p?.def.defName == "Mech_Mammoth")
                {
                    // Only can be sappers 15% of the time; use points as seed, so same attack acts the same way
                    __result = Rand.ChanceSeeded(0.15f, GenMath.RoundRandom(pointsTotal));
                }
                if (p?.def.defName == "Mech_Flamebot")
                {
                    // Only can be sappers 20% of the time; use points as seed, so same attack acts the same way
                    __result = Rand.ChanceSeeded(0.20f, GenMath.RoundRandom(pointsTotal)+1);
                }
            }
        }
    }
}
