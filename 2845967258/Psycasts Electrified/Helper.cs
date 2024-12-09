using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Psycasts_Electrified
{
    public static class Helper
    {
        public static JobDef ChargeEar;

        public static IEnumerable<CompChargingEar> GetChargingEars(Pawn pawn)
        {
            return pawn.health.hediffSet.GetAllComps().OfType<CompChargingEar>();
        }

        public static float GetElectricalSuperloadCost(Pawn pawn)
        {
            float cost = Settings.electricalSuperloadBaseCost;

            if (pawn.HasPsylink)
            {
                for (int i = 0; i < pawn.psychicEntropy.Psylink.level; i++)
                {
                    cost += Settings.electricalSuperloadIncreasePerLevel;
                }
            }

            return cost;
        }

        public static float GetElectricalOverloadCost(Pawn pawn)
        {
            float cost = Settings.electricalOverloadCost;

            for (float i = 0; i < (1f - pawn.psychicEntropy.CurrentPsyfocus); i += 0.01f)
            {
                cost += Settings.electricalOverloadCost;
            }

            return cost;
        }

        public static bool HasChargingEar(Pawn pawn)
        {
            return pawn.health.hediffSet.HasHediff(ModDefOf.ChargingEar);
        }
    }
}
