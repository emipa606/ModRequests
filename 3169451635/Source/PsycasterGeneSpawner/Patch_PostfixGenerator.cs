using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using RimWorld;
using SpawnThoseGenes;
using Verse;

using VanillaPsycastsExpanded;
using VFECore.Abilities;


namespace PsycasterGeneSpawner {

    [HarmonyPatch(typeof(SpawnThoseGenesMod), "PostfixGenerator")]
    public class Patch_PostfixGenerator {
        public static void Postfix(Pawn pawn) {
            GeneDef psycasterGene = Utils.GetPsycasterGene(pawn);

            if (psycasterGene != null && pawn.DevelopmentalStage == DevelopmentalStage.Adult) {
                Hediff_PsycastAbilities implant = Utils.GivePsylink(pawn);

                Utils.GivePsycasterPath(pawn, psycasterGene, implant);
            }
        }

    }
}
