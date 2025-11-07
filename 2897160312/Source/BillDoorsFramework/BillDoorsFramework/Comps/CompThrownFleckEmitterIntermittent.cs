using RimWorld;
using Verse.Sound;
using Verse;
using System.Collections.Generic;
using UnityEngine;
using Verse.Noise;

namespace BillDoorsFramework
{
    public class CompThrownFleckEmitterIntermittent : ThingComp
    {
        public int nextEmissionTick = 0;

        private CompProperties_ThrownFleckEmitterIntermittent Props => (CompProperties_ThrownFleckEmitterIntermittent)props;


        FloatRange rotRange => new FloatRange(0, 360);

        FloatRange offsetRange => new FloatRange(-Props.offsetRange, Props.offsetRange);
        private Color EmissionColor => Color.Lerp(Props.colorA, Props.colorB, Rand.Value);

        protected virtual bool IsOn
        {
            get
            {
                if (!parent.Spawned)
                {
                    return false;
                }
                CompPowerTrader comp = parent.GetComp<CompPowerTrader>();
                if (comp != null && !comp.PowerOn)
                {
                    return false;
                }
                CompSendSignalOnCountdown comp2 = parent.GetComp<CompSendSignalOnCountdown>();
                if (comp2 != null && comp2.ticksLeft <= 0)
                {
                    return false;
                }
                var comp3 = parent.GetComp<CompInitiatable>();
                if (comp3 != null && !comp3.Initiated)
                {
                    return false;
                }
                var comp4 = parent.GetComp<CompFlickable>();
                if (comp4 != null && !comp4.SwitchIsOn)
                {
                    return false;
                }
                CompHackable comp5 = parent.GetComp<CompHackable>();
                if (comp5 != null && comp5.IsHacked)
                {
                    return false;
                }
                if (parent is Building_Crate building_Crate && !building_Crate.HasAnyContents)
                {
                    return false;
                }
                return true;
            }
        }

        public override void CompTick()
        {
            if (IsOn && nextEmissionTick <= Find.TickManager.TicksAbs)
            {
                Emit();
                nextEmissionTick = Props.emissionInterval.RandomInRange + Find.TickManager.TicksAbs;
            }
        }

        protected void Emit()
        {
            var dataStatic = FleckMaker.GetDataStatic(parent.DrawPos + new Vector3(offsetRange.RandomInRange, 0, offsetRange.RandomInRange), parent.MapHeld, Props.fleck, Props.scaleRange.RandomInRange);
            dataStatic.rotation = rotRange.RandomInRange;
            dataStatic.rotationRate = Props.rotationRate.RandomInRange;
            dataStatic.instanceColor = EmissionColor;
            dataStatic.velocityAngle = Props.velocityX.RandomInRange;
            dataStatic.velocitySpeed = Props.velocityY.RandomInRange;
            parent.MapHeld.flecks.CreateFleck(dataStatic);
            if (!Props.soundOnEmission.NullOrUndefined())
            {
                Props.soundOnEmission.PlayOneShot(SoundInfo.InMap(parent));
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref nextEmissionTick, ((Props.saveKeysPrefix != null) ? (Props.saveKeysPrefix + "_") : "") + "ticksSinceLastEmitted", 0);
        }
    }

    public class CompProperties_ThrownFleckEmitterIntermittent : CompProperties
    {
        public FleckDef fleck;

        public float offsetRange = 0;

        public IntRange emissionInterval = new IntRange(1, 1);

        public FloatRange scaleRange = new FloatRange(1, 1);

        public SoundDef soundOnEmission;

        public int burstCount = 1;

        public Color colorA = Color.white;

        public Color colorB = Color.white;

        public FloatRange rotationRate;

        public FloatRange velocityX;

        public FloatRange velocityY;

        public string saveKeysPrefix;

        public CompProperties_ThrownFleckEmitterIntermittent()
        {
            compClass = typeof(CompThrownFleckEmitterIntermittent);
        }

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            if (fleck == null)
            {
                yield return "CompFleckEmitter must have a fleck assigned.";
            }
        }
    }
}
