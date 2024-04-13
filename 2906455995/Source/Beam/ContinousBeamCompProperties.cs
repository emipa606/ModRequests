using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;


namespace Beam
{
    public class ContinousBeamCompProperties : CompProperties
    {
        public float beamHeat;
        public HediffDef debuff;
        public SoundDef sound;

        public ContinousBeamCompProperties()
        {
            compClass = typeof(ContinousBeamComp);
        }
    }
}