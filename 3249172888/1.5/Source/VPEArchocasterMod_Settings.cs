using HarmonyLib;
using System.Linq;
using UnityEngine;
using Verse;

namespace VPEArchocaster
{
    [StaticConstructorOnStartup]
    public class VPEArchocasterMod_Settings : ModSettings
    {

        public static float honorToGoodWillRatio = 5;
        public static float allowThrallsLearning = 0f;
        public static float maxThrallLearningMultiplier;


        public override void ExposeData()
        {
            Scribe_Values.Look(ref honorToGoodWillRatio, "honorToGoodWillRatio", 5);
            Scribe_Values.Look(ref allowThrallsLearning, "allowThrallsLearning", 0f);
            base.ExposeData();
        }
    }
}
