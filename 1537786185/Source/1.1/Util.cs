using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace PredatorHuntAlert {
    public static class Util {

        public static Pawn GetVictim(Job job) {
            if (job == null) {
                return null;
            }

            if (job.def != JobDefOf.PredatorHunt) {
                return null;
            }

            Pawn victim = job.targetA.Thing as Pawn;
            if (victim == null) {
                return null;
            }

            if (PawnUtility.ShouldSendNotificationAbout(victim)) {
                return victim;
            }

            return null;
        }

        public static Pawn ResolveFocusTarget(Pawn predator, Pawn victim) {
            return (PredatorHuntAlertMod.Settings.focusTarget == PredatorHuntAlertSettings.FocusTarget.Predator) ? predator : victim;
        }
    }
}
