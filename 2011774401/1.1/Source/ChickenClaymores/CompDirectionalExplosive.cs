using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace ChickenExplosives
{
    class CompDirectionalExplosive : CompExplosive
    {
        public new CompProperties_DirectionalExplosive Props => (CompProperties_DirectionalExplosive)base.props;

        protected new void Detonate(Map map, bool ignoreUnspawned = false)
        {
            if (!ignoreUnspawned && !base.parent.SpawnedOrAnyParentSpawned) return;
            float radius = ExplosiveRadius();
            if (Props.explosiveExpandPerFuel > 0f) base.parent.GetComp<CompRefuelable>()?.ConsumeFuel(base.parent.GetComp<CompRefuelable>().Fuel);
            if(Props.destroyThingOnExplosionSize <= radius && !base.parent.Destroyed)
            {
                destroyedThroughDetonation = true;
                base.parent.Kill(null, null);
            }
            EndWickSustainer();
            wickStarted = false;
            if (map == null) Log.Warning("Tried to detonate a CompDirectionalExplosive in a null map.");
            else
            {
                if(Props.explosionEffect != null)
                {
                    Effecter effecter = Props.explosionEffect.Spawn();
                    effecter.Trigger(new TargetInfo(base.parent.PositionHeld, map), new TargetInfo(base.parent.PositionHeld, map));
                    effecter.Cleanup();
                }
                DoExplosion();
            }
        }
        public void DoExplosion()
        {

        }
        public void EndWickSustainer()
        {
            if (wickSoundSustainer != null)
            {
                wickSoundSustainer.End();
                wickSoundSustainer = null;
            }
        }
    }
    class CompProperties_DirectionalExplosive : CompProperties_Explosive
    {
        public int angleDegrees = 15, projectileCount = 0;
        public ThingDef projectileDef = null;
        public bool disableExplosion = false;

        public CompProperties_DirectionalExplosive()
        {
            base.compClass = typeof(CompDirectionalExplosive);
        }

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (string s in base.ConfigErrors(parentDef)) yield return s;
            yield return "CompDirectionalExplosive is not yet complete and will not work!";
            if (angleDegrees < 0)   yield return "angle is less than 0, explosion will not happen";
            if (angleDegrees > 360) yield return "angle is greater than 360, set it to 360 or less";
        }
    }
}
