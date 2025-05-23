﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Radiology
{
    public class RadiationInflurence
    {
        public FloatRange perSecond;
        public float multiplier = 1.0f;
    }

    public class MoteSprayer
    {
        public ThingDef mote;
        public FloatRange scaleRange;
        public FloatRange speed;
        public FloatRange offset;
        public float spread;
        public float initialSpread = 0f;
        public bool skip = false;
        public float reflectChance = 0f;
    }

    public class CompPropertiesIrradiator :CompProperties
    {
        public CompPropertiesIrradiator()
        {
            compClass = typeof(CompIrradiator);
        }

        public List<MoteSprayer> motes;

        public RadiationInflurence burn;
        public RadiationInflurence mutate;
        public RadiationInflurence mutateRare;
        public float powerConsumption;

        public SoundDef soundIrradiate;

        public override void ResolveReferences(ThingDef parentDef)
        {
            base.ResolveReferences(parentDef);

            if (burn == null) burn = new RadiationInflurence();
            if (mutate == null) mutate = new RadiationInflurence();
        }
    }
}
