using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Addictol
{
    [DefOf]
    class AddictolDefOf
    {
        public static HediffDef AddictolDetox;
        static AddictolDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(AddictolDefOf));
        }
    }

    public class IngestionOutcomeDoer_AddictolDetox : IngestionOutcomeDoer
    {
        /// Variables that control when the Addictol hediff is removed ///
        public bool containsAddictol = false;
        public float addHediffChance = 1f;
        public HediffDef hediffToAdd;

        /// Main function that executes the code upon ingestion ///
        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
        {
            /// This section, along with the IF statement beneath it, are used to add the AddictolDetox Hediff ///
            List<Hediff> hediffs1 = pawn.health.hediffSet.hediffs.ToList(); ;
            foreach (Hediff hediff in hediffs1)
            {
                var StrHediff = hediff.ToString();

                if (StrHediff.Contains("AddictolDetox"))
                {
                    containsAddictol = true;
                    hediff.Severity += 1f;
                }
            }

            /// Should Addictol not be active on the pawn ///
            if (!containsAddictol)
            {
                pawn.health.AddHediff(AddictolDefOf.AddictolDetox);

                List<Hediff> hediffs2 = pawn.health.hediffSet.hediffs.ToList(); ;
                foreach (Hediff hediff in hediffs2)
                {
                    var StrHediff = hediff.ToString();

                    if (StrHediff.Contains("AddictolDetox"))
                    {
                        hediff.Severity = 1f;
                    }
                }
            }

            /// Once Addictol has been added, this section iterates through all Hediffs on the pawn, removing all ones that Addictol cures ///
            List<Hediff> hediffs3 = pawn.health.hediffSet.hediffs.ToList();
            foreach (Hediff hediff in hediffs3)
            {
                /// This Switch iterates through all Hediffs, if any of the following cases are found, it removes it and then continues until the list is exhausted ///
                switch (hediff.def.defName)
                {
                    case "AlcoholAddiction":
                        pawn.health.RemoveHediff(hediff);
                        break;
                    case "AlcoholTolerance":
                        pawn.health.RemoveHediff(hediff);
                        break;
                    case "AmbrosiaAddiction":
                        pawn.health.RemoveHediff(hediff);
                        break;
                    case "AmbrosiaTolerance":
                        pawn.health.RemoveHediff(hediff);
                        break;
                    case "PsychiteAddiction":
                        pawn.health.RemoveHediff(hediff);
                        break;
                    case "PsychiteTolerance":
                        pawn.health.RemoveHediff(hediff);
                        break;
                    case "SmokeleafAddiction":
                        pawn.health.RemoveHediff(hediff);
                        break;
                    case "SmokeleafTolerance":
                        pawn.health.RemoveHediff(hediff);
                        break;
                    case "WakeUpAddiction":
                        pawn.health.RemoveHediff(hediff);
                        break;
                    case "WakeUpTolerance":
                        pawn.health.RemoveHediff(hediff);
                        break;
                    case "GoJuiceAddiction":
                        pawn.health.RemoveHediff(hediff);
                        break;
                    case "GoJuiceTolerance":
                        pawn.health.RemoveHediff(hediff);
                        break;
                    default:
                        System.Console.WriteLine("Nothing found");
                        break;
                }
            }
        }
    }
 }