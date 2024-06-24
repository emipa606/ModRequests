using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using Verse.AI.Group;
using static PredatorHuntAlert.PredatorHuntAlertSettings;

namespace PredatorHuntAlert {
    public class Alert_PredatorHunt : Alert_Critical {
        //Pair:(Predator,Victim)
        private IEnumerable<Pair<Pawn,Pawn>> PredatorAndVictimPairs {
            get {
                foreach (Pawn p in PawnsFinder.AllMaps_Spawned) {
                    Pawn victim = Util.GetVictim(p.CurJob);

                    if (victim != null) {
                        yield return new Pair<Pawn, Pawn>(p,victim);
                    }
                }
            }
        }

        public Alert_PredatorHunt() {
            this.defaultLabel = "PredatorHunt.AlertPredatorHuntLabel".Translate();
        }

        public override TaggedString GetExplanation() {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var group in this.PredatorAndVictimPairs.GroupBy(pair => pair.First.KindLabel + "/" + pair.Second.Label)) {
                Pair<Pawn, Pawn> current = group.First();
                stringBuilder.AppendLine("PredatorHunt.AlertPredatorHuntDescription".Translate(current.Second.LabelShort, current.First.LabelShort, group.Count()));
            }
            return stringBuilder.ToString();
        }

        public override AlertReport GetReport() {
            AlertCulpritSequence alertCulpritSequence = PredatorHuntAlertMod.Settings.alertCulpritSequence;
            if (alertCulpritSequence == AlertCulpritSequence.NoAlert) {
                return false;
            }

            List<Pawn> pawns = new List<Pawn>();
            foreach (Pair<Pawn, Pawn> pair in this.PredatorAndVictimPairs) {
                if (alertCulpritSequence == AlertCulpritSequence.PredatorOnly || alertCulpritSequence == AlertCulpritSequence.AlternatePredatorAndVictim) {
                    pawns.Add(pair.First);
                }
                if (alertCulpritSequence == AlertCulpritSequence.VictimOnly || alertCulpritSequence == AlertCulpritSequence.AlternatePredatorAndVictim) {
                    pawns.Add(pair.Second);
                }
            }
            if (pawns.NullOrEmpty()) {
                return false;
            }

            return AlertReport.CulpritsAre(pawns);
        }
    }
}
