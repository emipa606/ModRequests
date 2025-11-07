using RimWorld;
using UnityEngine;
using Verse;

namespace BillDoorsFramework
{
    public class Skyfaller_Custom : Skyfaller
    {
        public ActiveDropPodInfo ActiveDropPodInfo
        {
            get
            {
                return activeDropPodInfo;
            }
            set
            {
                if (activeDropPodInfo != null)
                {
                    activeDropPodInfo.parent = null;
                }
                if (value != null)
                {
                    value.parent = this;
                }
                activeDropPodInfo = value;
            }
        }

        private ActiveDropPodInfo activeDropPodInfo;

        public SkyfallerCustomExtension extension => def.GetModExtension<SkyfallerCustomExtension>();

        protected override void SpawnThings()
        {
            for (int num = innerContainer.Count - 1; num >= 0; num--)
            {
                GenPlace.TryPlaceThing(innerContainer[num], base.Position, base.Map, ThingPlaceMode.Near, delegate (Thing thing, int count)
                {
                    PawnUtility.RecoverFromUnwalkablePositionOrKill(thing.Position, thing.Map);
                    if (thing.def.Fillage == FillCategory.Full && def.skyfaller.CausesExplosion && def.skyfaller.explosionDamage.isExplosive && thing.Position.InHorDistOf(base.Position, def.skyfaller.explosionRadius))
                    {
                        base.Map.terrainGrid.Notify_TerrainDestroyed(thing.Position);
                    }
                }, null, innerContainer[num].Rotation);
            }

            if (extension != null && extension.returnShuttleFallerDef != null)
            {
                SkyfallerMaker.SpawnSkyfaller(extension.returnShuttleFallerDef, Position, Map);
            }
        }

        public override string Label
        {
            get
            {
                if ((extension == null || extension.variableLabel) && innerContainer.Any)
                {
                    return innerContainer[0].Label;
                }
                return base.Label;
            }
        }
    }

    public class SkyfallerCustomExtension : DefModExtension
    {
        public bool variableLabel;

        public ThingDef returnShuttleFallerDef;
    }
    public class CompSkyfallerThrownMoteEmitter : ThingComp
    {
        public bool emittedBefore;

        public int ticksSinceLastEmitted;

        Vector3 lastDrawPos = new Vector3(0, 0, 0);

        private CompProperties_SkyfallerThrownMote Props => (CompProperties_SkyfallerThrownMote)props;

        private Vector3 EmissionOffset => new Vector3(Rand.Range(Props.offsetMin.x, Props.offsetMax.x), Rand.Range(Props.offsetMin.y, Props.offsetMax.y), Rand.Range(Props.offsetMin.z, Props.offsetMax.z));

        private Color EmissionColor => Color.Lerp(Props.colorA, Props.colorB, Rand.Value);

        private bool IsOn
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
                if (parent is Building_MusicalInstrument building_MusicalInstrument && !building_MusicalInstrument.IsBeingPlayed)
                {
                    return false;
                }
                CompInitiatable comp3 = parent.GetComp<CompInitiatable>();
                if (comp3 != null && !comp3.Initiated)
                {
                    return false;
                }
                if (parent is Skyfaller skyfaller && skyfaller.FadingOut)
                {
                    return false;
                }
                return true;
            }
        }

        public override void CompTick()
        {
            Skyfaller skyfaller = parent as Skyfaller;
            if (!IsOn || skyfaller == null || skyfaller.ticksToImpact > Props.timeFrame.max || skyfaller.ticksToImpact < Props.timeFrame.min)
            {
                return;
            }
            if (Props.emissionInterval != -1)
            {
                if (ticksSinceLastEmitted >= Props.emissionInterval)
                {
                    Emit(skyfaller);
                    ticksSinceLastEmitted = 0;
                }
                else
                {
                    ticksSinceLastEmitted++;
                }
            }
            else if (!emittedBefore)
            {
                Emit(skyfaller);
                emittedBefore = true;
            }
            lastDrawPos = parent.DrawPos;
        }

        private void Emit(Skyfaller skyfaller)
        {
            Vector3 curveOffset = new Vector3(skyfaller.def.skyfaller.xPositionCurve == null ? 0 : skyfaller.def.skyfaller.xPositionCurve.Evaluate(TimeInAnimation(skyfaller)), 0, skyfaller.def.skyfaller.zPositionCurve == null ? 0 : skyfaller.def.skyfaller.zPositionCurve.Evaluate(TimeInAnimation(skyfaller))); ; ;
            float rotation = skyfaller.def.skyfaller.rotationCurve == null ? 0 : skyfaller.def.skyfaller.rotationCurve.Evaluate(TimeInAnimation(skyfaller));

            for (int i = 0; i < Props.burstCount; i++)
            {
                MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(Props.mote);
                moteThrown.Scale = Props.scale.RandomInRange;
                moteThrown.rotationRate = Props.rotationRate.RandomInRange;
                moteThrown.exactRotation = Props.rotationRate.RandomInRange;
                moteThrown.instanceColor = EmissionColor;
                moteThrown.SetVelocity(Props.velocityX.RandomInRange, Props.velocityY.RandomInRange);

                if (Props.onLandingSite)
                {
                    moteThrown.exactPosition = skyfaller.TrueCenter() + EmissionOffset;
                }
                else
                {
                    moteThrown.exactPosition = skyfaller.DrawPos + curveOffset + skyfaller.DrawPos - lastDrawPos + EmissionOffset.RotatedBy(rotation);
                    moteThrown.Velocity = moteThrown.Velocity.RotatedBy(rotation) + (skyfaller.DrawPos - lastDrawPos) * 60;
                }

                if (moteThrown.exactPosition.ToIntVec3().InBounds(parent.Map))
                {
                    GenSpawn.Spawn(moteThrown, moteThrown.exactPosition.ToIntVec3(), parent.Map);
                }
            }
        }

        float TimeInAnimation(Skyfaller skyfaller)
        {
            if (skyfaller.def.skyfaller.reversed)
            {
                return skyfaller.ticksToImpact / (float)skyfaller.LeaveMapAfterTicks;
            }
            return 1f - (float)skyfaller.ticksToImpact / 220;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref ticksSinceLastEmitted, "ticksSinceLastEmitted", 0);
            Scribe_Values.Look(ref emittedBefore, "emittedBefore", defaultValue: false);
            Scribe_Values.Look(ref lastDrawPos, "lastDrawPos");
        }
    }

    public class CompProperties_SkyfallerThrownMote : CompProperties_ThrownMoteEmitter
    {
        public IntRange timeFrame = new IntRange(-1, 1000);

        public bool onLandingSite = false;

        public CompProperties_SkyfallerThrownMote()
        {
            compClass = typeof(CompSkyfallerThrownMoteEmitter);
        }
    }

    public class CompSkyfallerShadow : ThingComp
    {
        private CompProperties_SkyfallerShadow Props => (CompProperties_SkyfallerShadow)props;

        private Material cachedExactShadow;
        private Material ExactShadow
        {
            get
            {
                if (cachedExactShadow == null && Props.shadow != null)
                {
                    cachedExactShadow = MaterialPool.MatFrom(Props.shadow, ShaderDatabase.Transparent);
                }
                return cachedExactShadow;
            }
        }

        Skyfaller skyfaller => parent as Skyfaller;

        public override void PostDraw()
        {
            Material shadowMaterial = ExactShadow;
            if (shadowMaterial != null)
            {
                Vector3 shadowPos = parent.DrawPos;
                shadowPos.z = parent.TrueCenter().z;
                shadowPos.y = AltitudeLayer.Shadows.AltitudeFor();

                Color color = shadowMaterial.color;
                color.a = Mathf.Clamp(1f - (float)skyfaller.ticksToImpact / 150, 0.2f, 1);
                shadowMaterial.color = color;

                Matrix4x4 matrix = default;
                matrix.SetTRS(shadowPos, parent.Rotation.AsQuat, new Vector3(parent.DrawSize.x, 1f, parent.DrawSize.y));
                Graphics.DrawMesh(MeshPool.plane10Back, matrix, shadowMaterial, 0, null, 0);
            }
        }
    }

    public class CompProperties_SkyfallerShadow : CompProperties
    {
        public string shadow;

        public CompProperties_SkyfallerShadow()
        {
            compClass = typeof(CompSkyfallerShadow);
        }
    }
}
