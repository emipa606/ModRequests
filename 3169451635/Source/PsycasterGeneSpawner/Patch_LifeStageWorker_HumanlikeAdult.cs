using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PsycasterGeneSpawner {

    [HarmonyPatch(typeof(LifeStageWorker_HumanlikeAdult), "Notify_LifeStageStarted")]
    public class Patch_LifeStageWorker_HumanlikeAdult {
        public static void Postfix(Pawn pawn) {
            GeneDef psycasterGene = Utils.GetPsycasterGene(pawn);

            /*
            Log.Message($"Pawn {pawn.Name} became an adult");
            foreach(GeneDef gene in Utils.AvaliablePsycasterGenes) {
                Log.Message($"{gene}");
            }
            */

            if (psycasterGene != null) {
                Utils.GivePsylink(pawn);
            }
        }


    }
}
