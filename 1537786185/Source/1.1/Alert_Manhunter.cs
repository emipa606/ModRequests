using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using Verse.AI.Group;

namespace PredatorHuntAlert {
    public class Alert_Manhunter : Alert_Critical {
        private List<Pawn> manhunters = new List<Pawn>();

        private List<Pawn> Manhunters {
            get {
                this.manhunters.Clear();
                foreach (Pawn pawn in PawnsFinder.AllMaps_Spawned) {
                    if (pawn.MentalStateDef == MentalStateDefOf.ManhunterPermanent) {
                        manhunters.Add(pawn);
                    }
                }
                foreach (Pawn pawn in PawnsFinder.AllMaps_Spawned) {
                    if (pawn.MentalStateDef == MentalStateDefOf.Manhunter) {
                        manhunters.Add(pawn);
                    }
                }
                return this.manhunters;
            }
        }

        public Alert_Manhunter() {
            this.defaultLabel = "PredatorHunt.AlertManhunterLabel".Translate();
        }

        public override TaggedString GetExplanation() {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("PredatorHunt.AlertManhunterDescriptionBase".Translate());
            foreach (var group in this.Manhunters.GroupBy(x => x.kindDef.defName + "/" + x.MentalStateDef.defName)) {
                Pawn p = group.First();
                string permanent = (p.MentalStateDef == MentalStateDefOf.ManhunterPermanent) ? "PredatorHunt.AlertManhunterDescriptionItemsPermanent".Translate().ToString() : "";
                stringBuilder.AppendLine("PredatorHunt.AlertManhunterDescriptionItems".Translate(p.Label, permanent, group.Count()));
            }
            return stringBuilder.ToString();
        }

        public override AlertReport GetReport() {
            if (!PredatorHuntAlertMod.Settings.enableManhunterAlert) {
                return false;
            }

            return AlertReport.CulpritsAre(this.Manhunters);
        }
    }
}
