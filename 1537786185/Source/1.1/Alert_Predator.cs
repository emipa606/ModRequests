using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using Verse.AI.Group;

namespace PredatorHuntAlert {
    public class Alert_Predator : Alert {
        private List<Pawn> predators = new List<Pawn>();

        private List<Pawn> Predators {
            get {
                this.predators.Clear();
                foreach (Pawn pawn in PawnsFinder.AllMaps_Spawned) {
                    if (pawn.RaceProps.predator && (pawn.Faction == null || !pawn.Faction.IsPlayer)) {
                        predators.Add(pawn);
                    }
                }
                return this.predators;
            }
        }

        public Alert_Predator() {
            this.defaultLabel = "PredatorHunt.AlertPredatorLabel".Translate();
        }

        public override TaggedString GetExplanation() {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("PredatorHunt.AlertPredatorDescriptionBase".Translate());
            foreach (var group in this.Predators.GroupBy(x => x.kindDef)) {
                stringBuilder.AppendLine("PredatorHunt.AlertPredatorDescriptionItems".Translate(group.First().KindLabel,group.Count()));
            }
            return stringBuilder.ToString();
        }

        public override AlertReport GetReport() {
            if (!PredatorHuntAlertMod.Settings.enablePredatorAlert) {
                return false;
            }

            return AlertReport.CulpritsAre(this.Predators);
        }
    }
}
