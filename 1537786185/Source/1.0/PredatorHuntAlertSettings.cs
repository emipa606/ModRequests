using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PredatorHuntAlert {
    public class PredatorHuntAlertSettings : ModSettings {
        public enum AlertType {
            Message, Letter
        };
        public enum FocusTarget {
            Predator,Victim
        };
        public enum AlertCulpritSequence {
            NoAlert,PredatorOnly,VictimOnly, AlternatePredatorAndVictim
        };

        public AlertType alertType;
        public FocusTarget focusTarget;
        public AlertCulpritSequence alertCulpritSequence;

        public override void ExposeData() {
            base.ExposeData();

            Scribe_Values.Look<AlertType>(ref this.alertType, "alertType", AlertType.Message);
            Scribe_Values.Look<FocusTarget>(ref this.focusTarget, "focusTarget", FocusTarget.Predator);
            Scribe_Values.Look<AlertCulpritSequence>(ref this.alertCulpritSequence, "alertCulpritSequence", AlertCulpritSequence.NoAlert);
        }
    }
}
