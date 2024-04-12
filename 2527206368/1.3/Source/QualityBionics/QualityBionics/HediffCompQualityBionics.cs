using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static Verse.PawnCapacityUtility;

namespace QualityBionics
{
    public class HediffCompProperties_QualityBionics : HediffCompProperties
    {
        public float baseEfficiency; //new - Used to calculate the efficiency in the harmony-patches.
        public HediffCompProperties_QualityBionics()
        {
            this.compClass = typeof(HediffCompQualityBionics);
        }
    }

    public class HediffCompQualityBionics : HediffComp
    {
        public HediffCompProperties_QualityBionics Props => (HediffCompProperties_QualityBionics)props; //new - Used to get access to the Properties from here.
        public QualityCategory quality = QualityCategory.Normal;
        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref quality, "quality", QualityCategory.Normal);
        }
    }

}
