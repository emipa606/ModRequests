using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace LifeSupport {
    public static class LifeSupportUtility {
        public static bool ValidLifeSupportNearby(this Pawn pawn) {
            return (
                pawn.CurrentBed() is Building_Bed bed &&
                GenAdj.CellsAdjacent8Way(bed).Any(cell => {
                    return cell.GetThingList(bed.Map).Any(cellThing => {
                        if (cellThing.TryGetComp<LifeSupportComp>() is LifeSupportComp lifeSupport) {
                            return lifeSupport.Active;
                        } else {
                            return false;
                        }
                    });
                })
            );
        }

        internal static bool WouldDieWithoutLifeSupport(this Pawn pawn) {
            PawnCapacitiesHandler capacitiesHandler = pawn.health.capacities;
            bool isFlesh = pawn.RaceProps.IsFlesh;
            foreach (PawnCapacityDef pawnCapacityDef in DefDatabase<PawnCapacityDef>.AllDefsListForReading) {
                if (isFlesh ? !pawnCapacityDef.lethalFlesh : !pawnCapacityDef.lethalMechanoids) {
                    // not deadly
                } else if (!capacitiesHandler.CapableOf(pawnCapacityDef)) {
                    return true;
                }
            }
            return false;
        }

        public static void SetHediffs(this Pawn pawn) {
            bool validLifeSupportNearby = pawn.ValidLifeSupportNearby();
            pawn.SetHediffs(validLifeSupportNearby);
        }

        public static void SetHediffs(this Pawn pawn, bool validLifeSupportNearby) {
            var health = pawn.health;
            var hediff_deathrattle = new List<Hediff>();

            Hediff hediff_lifesupport = null;
            var Hediff_DeathRattle = HarmonyPatches.Hediff_DeathRattle;
            foreach (var hediff in health.hediffSet.hediffs) {
                if (hediff.def == LifeSupportDefOf.QE_LifeSupport) {
                    hediff_lifesupport = hediff;
                } else if (!(Hediff_DeathRattle is null) && Hediff_DeathRattle.IsInstanceOfType(hediff)) {
                    hediff_deathrattle.Add(hediff);
                }
            }

            if (validLifeSupportNearby) {
                if (hediff_lifesupport is null) {
                    hediff_lifesupport = health.AddHediff(LifeSupportDefOf.QE_LifeSupport);
                }
                hediff_lifesupport.Severity = pawn.WouldDieWithoutLifeSupport() ? 1.0f : 0.5f;

                foreach (var hediff in hediff_deathrattle) {
                    health.RemoveHediff(hediff);
                }
            } else if (!(hediff_lifesupport is null)) {
                health.RemoveHediff(hediff_lifesupport);
            }
        }
    }
}
