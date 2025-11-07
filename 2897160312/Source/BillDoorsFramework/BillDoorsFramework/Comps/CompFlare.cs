using RimWorld;
using UnityEngine;
using Verse;

namespace BillDoorsFramework
{
    public class CompFlare : ThingComp
    {
        public CompProperties_Flare Props => props as CompProperties_Flare;

        int lifeTime = 0;

        Effecter effecter;
        Effecter destroyWarningEffecter;

        Vector3 flarePos => Vector3.forward * (Props.initHeight - Props.descentSpeedPerTick * lifeTime);

        public override void CompTick()
        {
            if (effecter == null)
            {
                effecter = Props.effecter.Spawn(parent.Position, parent.Map, flarePos);
                parent.Map.effecterMaintainer.AddEffecterToMaintain(effecter, parent.Position, Props.lifeTimeTicks);
            }
            effecter.offset = flarePos;
            effecter.scale = Mathf.Lerp(1, 0.5f, lifeTime / Props.lifeTimeTicks);
            effecter.EffectTick(parent, parent);
            lifeTime++;

            if (Props.destroyWarningEffecter != null && lifeTime >= Props.warningTimeTicks)
            {
                if (destroyWarningEffecter == null)
                {
                    destroyWarningEffecter = Props.destroyWarningEffecter.Spawn(parent.Position, parent.Map, flarePos);
                    parent.Map.effecterMaintainer.AddEffecterToMaintain(destroyWarningEffecter, parent.Position, Props.lifeTimeTicks);
                }
                destroyWarningEffecter.EffectTick(parent, parent);
            }

            if (lifeTime >= Props.lifeTimeTicks)
            {
                effecter.Cleanup();
                if (Props.destroy) parent.Destroy();
            }
        }


        public override void PostExposeData()
        {
            Scribe_Values.Look(ref lifeTime, "lifeTime");
        }
    }

    public class CompProperties_Flare : CompProperties
    {
        public CompProperties_Flare()
        {
            compClass = typeof(CompFlare);
        }

        public EffecterDef effecter;
        public EffecterDef destroyWarningEffecter;

        public bool destroy = true;

        public float initHeight = 5;

        public float finalHeight = 1;

        public float descentSpeedPerTick => (initHeight - finalHeight) / lifeTimeTicks;

        float lifeTimeSec = 15;

        float warningTimeSec = 5;

        public int warningTimeTicks => (int)(lifeTimeTicks - warningTimeSec * 60);
        public int lifeTimeTicks => (int)(lifeTimeSec * 60);
    }
}
