using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MoreRecordsInteraction {
    public class InteractionRecordDefGenerator {
        private static string InteractRecordDefPrefix = "TMB_Interact";
        private static string InteractedRecordDefPrefix = "TMB_Interacted";

        public static string GetInteractRecordDefName(InteractionDef interactionDef) {
            return InteractionRecordDefGenerator.InteractRecordDefPrefix + "_" + interactionDef.defName;
        }

        public static string GetInteractedRecordDefName(InteractionDef interactionDef) {
            return InteractionRecordDefGenerator.InteractedRecordDefPrefix + "_" + interactionDef.defName;
        }

        public static RecordDef GetInteractRecordDef(InteractionDef interactionDef) {
            return DefDatabase<RecordDef>.GetNamed(GetInteractRecordDefName(interactionDef));
        }

        public static RecordDef GetInteractedRecordDef(InteractionDef interactionDef) {
            return DefDatabase<RecordDef>.GetNamed(GetInteractedRecordDefName(interactionDef));
        }

        public static IEnumerable<RecordDef> ImpliedRecordDefs() {
            foreach (InteractionDef interactionDef in DefDatabase<InteractionDef>.AllDefs) {
                RecordDef recordDef = InteractionRecordDefGenerator.BaseInteractionRecord();
                recordDef.defName = InteractionRecordDefGenerator.GetInteractRecordDefName(interactionDef);
                recordDef.label = "MoreRecordsInteraction.InteractRecordLabel".Translate(interactionDef.label);
                recordDef.description = "MoreRecordsInteraction.InteractRecordDescription".Translate(interactionDef.label);
                recordDef.descriptionHyperlinks = new List<DefHyperlink>{
                    new DefHyperlink(interactionDef)
                };
                yield return recordDef;

                RecordDef recordDef2 = InteractionRecordDefGenerator.BaseInteractionRecord();
                recordDef2.defName = InteractionRecordDefGenerator.GetInteractedRecordDefName(interactionDef);
                recordDef2.label = "MoreRecordsInteraction.InteractedRecordLabel".Translate(interactionDef.label);
                recordDef2.description = "MoreRecordsInteraction.InteractedRecordDescription".Translate(interactionDef.label);
                recordDef2.descriptionHyperlinks = new List<DefHyperlink>{
                    new DefHyperlink(interactionDef)
                };
                yield return recordDef2;
            }
            yield break;
        }

        private static RecordDef BaseInteractionRecord() {
            return new RecordDef {
                type = RecordType.Int
            };
        }
    }
}
