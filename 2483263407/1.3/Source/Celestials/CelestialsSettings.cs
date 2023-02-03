using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using RimWorld;
using Verse;

namespace Celestials
{
    public class CelestialsSettings : ModSettings
    {
        public bool resurrectionSideEffects = true;

        public bool empExplosion = true;

        public IntRange resurrectionTicks = new IntRange(30000, 60000);

        public float celestialChance = 0.01f;

        public string chanceOfCelestial_buffer;

        public float chanceOfCelestial = 1f;

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look<bool>(ref resurrectionSideEffects, "resurrectionSideEffects", true);
            Scribe_Values.Look<bool>(ref empExplosion, "empExplosion", true);
            Scribe_Values.Look<IntRange>(ref resurrectionTicks, "resurrectionTicks", new IntRange(30000, 60000));
            Scribe_Values.Look<float>(ref celestialChance, "celestialChance", 0.01f);
        }
    }
}
