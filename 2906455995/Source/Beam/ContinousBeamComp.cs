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
    public class ContinousBeamComp : ThingComp
    {
        public new ContinousBeamCompProperties Props => (ContinousBeamCompProperties)props;

        public float getBeamHeat()
        {
            return Props.beamHeat;
        }

        public HediffDef getHediffDef()
        {
            return Props.debuff;
        }
        public SoundDef GetSoundDef()
        {
            return Props.sound;
        }
    }
}