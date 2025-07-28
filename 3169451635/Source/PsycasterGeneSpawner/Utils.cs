using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VanillaPsycastsExpanded;
using Verse;
using VFECore.Abilities;

namespace PsycasterGeneSpawner {

    [StaticConstructorOnStartup]
    public static class Utils {

        private static readonly List<GeneDef> geneList = DefDatabase<GeneDef>.AllDefsListForReading
            .Where(gene => gene.defName.Contains("Gene_"))
            .ToList();

        public static List<GeneDef> AvaliablePsycasterGenes { get => geneList;}

        public static GeneDef GetPsycasterGene(Pawn pawn) {
            
            foreach (GeneDef psyGene in AvaliablePsycasterGenes) {
                if (pawn.genes.HasGene(psyGene)) {
                    return psyGene;
                }
            }

            return null;
        }

        public static Hediff_PsycastAbilities GivePsylink(Pawn pawn) {
            if (pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.PsychicAmplifier) is not Hediff_Psylink psylink) {

                psylink = HediffMaker.MakeHediff(HediffDefOf.PsychicAmplifier, pawn, pawn.health.hediffSet.GetBrain()) as Hediff_Psylink;
                pawn.health.AddHediff(psylink);
            }

            //var implant = pawn.health.hediffSet.GetFirstHediffOfDef(VPE_DefOf.VPE_PsycastAbilityImplant) as Hediff_PsycastAbilities ??
                //HediffMaker.MakeHediff(VPE_DefOf.VPE_PsycastAbilityImplant, pawn,
                  // pawn.RaceProps.body.GetPartsWithDef(BodyPartDefOf.Brain).FirstOrFallback()) as Hediff_PsycastAbilities;

            var implant = pawn.health.hediffSet.GetFirstHediffOfDef(VPE_DefOf.VPE_PsycastAbilityImplant) as Hediff_PsycastAbilities ??
            HediffMaker.MakeHediff(VPE_DefOf.VPE_PsycastAbilityImplant, pawn,
            pawn.health.hediffSet.GetBrain()) as Hediff_PsycastAbilities;


            if (implant.psylink == null)
                implant.InitializeFromPsylink(psylink);

            return implant;
        }

        public static void GivePsycasterPath(Pawn pawn, GeneDef psycasterGene, Hediff_PsycastAbilities implant) {
            string geneName = psycasterGene.defName.Substring(5);
            var path = DefDatabase<PsycasterPathDef>.AllDefsListForReading.Where(ppd => ppd.defName.Contains(geneName)).FirstOrDefault();

            if (path == null) {
                Log.Message($"{geneName} - Does not have an correspondent in DefDatabase<PsycasterPathDef>");
                return;
            }

            implant.UnlockPath(path);

            CompAbilities comp = pawn.GetComp<CompAbilities>();
            if (comp == null) return;

            var abilities = path.abilities.Except(comp.LearnedAbilities.Select(ab => ab.def));

            do {
                if (abilities.Where(aab => aab.GetModExtension<AbilityExtension_Psycast>().PrereqsCompleted(comp)).TryRandomElement(out var ab)) {
                    comp.GiveAbility(ab);
                    if (implant.points <= 0)
                        implant.ChangeLevel(1, false);
                    implant.points--;
                    abilities = abilities.Except(ab);
                } else
                    break;
            } while (Rand.Value < PsycastsMod.Settings.additionalAbilityChance);
        }
    }


}
