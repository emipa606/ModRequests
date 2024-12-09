using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Psycasts_Electrified
{
    [HarmonyPatch(typeof(Pawn_PsychicEntropyTracker))]
    public class Pawn_PsychicEntropyTracker_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("PsyfocusFallPerDay", MethodType.Getter)]
        public static float ChargingEars(float reduceAmount, ref Pawn ___pawn)
        {
            IEnumerable<CompChargingEar> ears = Helper.GetChargingEars(___pawn);
            if (ears.EnumerableNullOrEmpty())
            {
                return reduceAmount;
            }

            foreach (CompChargingEar ear in ears)
            {
                if (ear == null) continue;

                if (ear.charge != 0) return 0f;
            }

            return reduceAmount;
        }

        [HarmonyPostfix]
        [HarmonyPatch("RecoveryRate", MethodType.Getter)]
        public static float NeuralHeatRecovery(float recoveryRate, ref Pawn ___pawn)
        {
            int heatsinkCount = ___pawn.health.hediffSet.GetHediffCount(ModDefOf.NeuralHeatsinkEar);
            if (heatsinkCount <= 0)
            {
                return recoveryRate;
            }

            return recoveryRate *= ((Settings.neuralHeatsink * heatsinkCount) / 100) + 1.0f;
        }
    }
}
