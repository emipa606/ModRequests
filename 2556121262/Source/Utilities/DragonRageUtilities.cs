using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace HalfDragons
{
    public static class DragonRageUtilities
    {
        public static Hediff GetDragonRageHediff(this Pawn pawn)
        {
            List<Hediff> hediffs = pawn?.health?.hediffSet?.hediffs;
            if (hediffs.NullOrEmpty())
            {
                return null;
            }
            return hediffs.Find(hediff => hediff.def == HD_HediffDefOf.HD_dragonRage);
        }

        public static bool IsHalfDragon(this Pawn pawn)
        {
            return pawn.def == HD_Common.halfDragonRace;
        }

        public static bool HasDragonRage(this Pawn pawn)
        {
            return pawn.GetDragonRageHediff() != null;
        }

        public static void AddDragonRage(this Pawn pawn)
        {
            Hediff dragonRage = HediffMaker.MakeHediff(HD_HediffDefOf.HD_dragonRage, pawn);
            dragonRage.PostMake();
            pawn.health.AddHediff(dragonRage);
        }

        public static void IncreaseDragonRage(this Pawn pawn)
        {
            //Log.Message("Increasing dragon rage");
            Hediff dragonRage = pawn.GetDragonRageHediff();
            if (dragonRage?.Severity >= HD_Common.dragonRageMaxSeverity)
            {
                //Log.Message("Resetting dragon rage");
                pawn.health.RemoveHediff(dragonRage);
                float dragonBloodGainFromDragonRage = SettingsAccess.dragonBloodGainFromDragonRage;
                pawn.needs.TryGetNeed<Need_DragonBlood>().CurLevel += dragonBloodGainFromDragonRage;
            }
            if (dragonRage == null)
            {
                //Log.Message("Adding dragon rage");
                pawn.AddDragonRage();
                dragonRage = pawn.GetDragonRageHediff();
            }
            dragonRage.Severity += 0.1f;
            //Log.Message("Increased dragon rage for " + pawn.LabelShort + ", new severity: " + dragonRage.Severity);
        }
    }
}
