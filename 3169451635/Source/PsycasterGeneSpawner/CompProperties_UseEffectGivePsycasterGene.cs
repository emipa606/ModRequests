using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PsycasterGeneSpawner {
    public class CompProperties_UseEffectGivePsycasterGene : CompProperties_UseEffect {
        public GeneDef psycasterGene;

        public CompProperties_UseEffectGivePsycasterGene() {
            compClass = typeof(CompUseEffect_GivePsycasterGene);
        }
    }
}
