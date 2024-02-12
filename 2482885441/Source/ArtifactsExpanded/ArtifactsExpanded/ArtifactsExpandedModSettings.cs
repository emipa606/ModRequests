using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace ArtifactsExpanded
{
    public class ArtifactsExpandedModSettings : ModSettings
    {
        //the fields
        public bool sacrificerBeltCountAnimals = false;
        public bool sacrificerBeltCountPrisoners = false;
        public bool sacrificerBeltGivesThought = true;
        public float archotechNeurotrainerExpFactor = 1f;
        public float growerMechSerumGrowFactor = 1f;
        public float revitalizerMechSerumFactor = 1f;
        public bool antiPsychicPulserAffectsAll = true;
        public float mindSuppressorStrength = 0.1f;
        public float rewardStandardHighFreqWeightFactor = 1f;

        //allows saving the data
        public override void ExposeData()
        {
            Scribe_Values.Look(ref sacrificerBeltCountAnimals, "sacrificerBeltCountAnimals", false, false);
            Scribe_Values.Look(ref sacrificerBeltCountPrisoners, "sacrificerBeltCountPrisoners", false, false);
            Scribe_Values.Look(ref sacrificerBeltGivesThought, "sacrificerBeltGivesThought", true, false);
            Scribe_Values.Look(ref archotechNeurotrainerExpFactor, "archotechNeurotrainerExpFactor", 1f, false);
            Scribe_Values.Look(ref growerMechSerumGrowFactor, "growerMechSerumGrowFactor", 1f, false);
            Scribe_Values.Look(ref revitalizerMechSerumFactor, "revitalizerMechSerumFactor", 1f, false);
            Scribe_Values.Look(ref antiPsychicPulserAffectsAll, "antiPsychicPulserAffectsAll", true, false);
            Scribe_Values.Look(ref mindSuppressorStrength, "mindSuppressorStrength", 0.1f, false);
            Scribe_Values.Look(ref rewardStandardHighFreqWeightFactor, "rewardStandardHighFreqWeightFactor", 1f, false);

            base.ExposeData();
        }
    }
}