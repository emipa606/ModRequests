﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace HediffResourceFramework
{
    public class HediffResource : HediffWithComps
    {
        public new HediffResourceDef def => base.def as HediffResourceDef;
        private float resourceAmount;
        public int duration;
        public int delayTicks;
        public float ResourceAmount
        {
            get
            {
                return resourceAmount;
            }
            set
            {
                resourceAmount = value;
                Log.Message(this.def + " setting resourceAmount to " + value);
                if (resourceAmount > ResourceCapacity)
                {
                    if (ResourceCapacity == 0)
                    {
                        HRFLog.Message($"{this} Resource amount ({resourceAmount}) is bigger than ResourceCapacity, setting it to {ResourceCapacity}");
                    }
                    resourceAmount = ResourceCapacity;
                }

                if (resourceAmount < 0)
                {
                    resourceAmount = 0;
                }

                if (resourceAmount <= 0 && !this.def.keepWhenEmpty)
                {
                    this.pawn.health.RemoveHediff(this);
                }
                else
                {
                    this.Severity = resourceAmount;
                }
            }
        }

        public bool CanGainResource => Find.TickManager.TicksGame > this.delayTicks;
        public void AddDelay(int newDelayTicks)
        {
            this.delayTicks = Find.TickManager.TicksGame + newDelayTicks;
        }
        public bool CanHaveDelay(int newDelayTicks)
        {
            if (Find.TickManager.TicksGame > delayTicks || newDelayTicks > Find.TickManager.TicksGame - delayTicks)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CanApplyStatBooster(StatBooster statBooster)
        {
            if (statBooster.resourceOnComplete != -1f && this.ResourceAmount < -statBooster.resourceOnComplete)
            {
                return false;
            }
            else if (statBooster.resourcePerSecond != -1 && this.ResourceAmount < -statBooster.resourcePerSecond)
            {
                return false;
            }
            return true;
        }

        public float ResourceCapacity
        {
            get
            {
                return this.def.maxResourceCapacity + HediffResourceUtils.GetHediffResourceCapacityGainFor(this.pawn, def) + GetHediffResourceCapacityGainFromAmplifiers();
            }
        }

        public bool CanGainCapacity(float newCapacity)
        {
            return ResourceCapacity > newCapacity || ResourceCapacity > 0;
        }

        public List<Thing> amplifiers = new List<Thing>();

        public Dictionary<Thing, IAdjustResouceInArea> cachedAmplifiers = new Dictionary<Thing, IAdjustResouceInArea>();

        public float GetHediffResourceCapacityGainFromAmplifiers()
        {
            float num = 0;
            foreach (var compAmplifier in Amplifiers)
            {
                num += compAmplifier.GetResourceCapacityGainFor(this.def);
            }
            return num;
        }
        public IAdjustResouceInArea GetCompAmplifierFor(Thing thing)
        {
            IAdjustResouceInArea comp = null;
            if (!cachedAmplifiers.TryGetValue(thing, out comp))
            {
                comp = thing.TryGetComp<CompAdjustHediffsArea>();
                cachedAmplifiers[thing] = comp;
            }
            return comp;
        }
        public IEnumerable<IAdjustResouceInArea> Amplifiers
        {
            get
            {
                for (int num = amplifiers.Count - 1; num >= 0; num--)
                {
                    var amplifier = amplifiers[num];
                    if (amplifier is null || amplifier.Destroyed)
                    {
                        amplifiers.RemoveAt(num);
                    }
                    else
                    {
                        var comp = GetCompAmplifierFor(amplifier);
                        if (comp != null && comp.InRadiusFor(this.pawn.Position, this.def))
                        {
                            yield return comp;
                        }
                        else
                        {
                            amplifiers.RemoveAt(num);
                        }
                    }
                }
            }
        }

        public void TryAddAmplifier(IAdjustResouceInArea comp)
        {
            if (!amplifiers.Contains(comp.Parent))
            {
                amplifiers.Add(comp.Parent);
                cachedAmplifiers[comp.Parent] = comp;
            }
        }

        public IEnumerable<IAdjustResouceInArea> GetAmplifiersFor(HediffResourceDef hediffResourceDef)
        {
            foreach (var amplifier in Amplifiers)
            {
                foreach (var option in amplifier.ResourceSettings)
                {
                    if (option.hediff == hediffResourceDef)
                    {
                        yield return amplifier;
                    }
                }
            }
        }
        public override string Label
        {
            get 
            {
                var label = base.Label;
                if (!def.hideResourceAmount)
                {
                    label += ": " + this.ResourceAmount.ToStringDecimalIfSmall() + " / " + this.ResourceCapacity.ToStringDecimalIfSmall();
                }
                if (this.def.lifetimeTicks != -1)
                {
                    label += " (" + Mathf.CeilToInt((this.def.lifetimeTicks - this.duration).TicksToSeconds()) + "s)";
                }
                return label;
            }
        }

        public override string TipStringExtra
        {
            get
            {
                return base.TipStringExtra + "\n" + this.def.fulfilsTranslationKey.Translate((TotalResourceGainAmount()).ToStringDecimalIfSmall());
            }
        }

        public override bool ShouldRemove
        {
            get
            {
                if (this.def.lifetimeTicks != -1 && duration > this.def.lifetimeTicks)
                {
                    return true;
                }
                if (this.def.keepWhenEmpty && this.ResourceAmount <= 0)
                {
                    return false;
                }
                //if (this.ResourceCapacity < 0)
                //{
                //    return true;
                //}
                if (SourceOnlyAmplifiers())
                {
                    return true;
                }
                var value = base.ShouldRemove;
                if (value)
                {
                    HRFLog.Message("Removing: " + this + " this.ResourceAmount: " + this.ResourceAmount + " - this.Severity: " + this.Severity);
                }
                return value;
            }
        }
        
        public bool SourceOnlyAmplifiers()
        {
            var amplifiers = Amplifiers;
            if (!this.def.keepWhenEmpty && amplifiers.Any())
            {
                foreach (var amplifier in amplifiers)
                {
                    var comp = GetCompAmplifierFor(amplifier.Parent);
                    if (comp != null && !comp.InRadiusFor(this.pawn.Position, this.def))
                    {
                        var option = comp.GetFirstHediffOptionFor(this.def);
                        if (option.removeOutsideArea)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public override void Tick()
        {
            base.Tick();
            this.duration++;
            if (Find.TickManager.TicksGame % 30 == 0 && ResourceCapacity < 0)
            {
                HediffResourceUtils.TryDropExcessHediffGears(this.pawn);
            }
        }
        
        private Vector3 impactAngleVect;

        private int lastAbsorbDamageTick = -9999;
        public void AbsorbedDamage(DamageInfo dinfo)
        {
            SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(base.pawn.Position, base.pawn.Map));
            impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
            Vector3 loc = base.pawn.TrueCenter() + impactAngleVect.RotatedBy(180f) * 0.5f;
            float num = Mathf.Min(10f, 2f + dinfo.Amount / 10f);
            MoteMaker.MakeStaticMote(loc, base.pawn.Map, ThingDefOf.Mote_ExplosionFlash, num);
            int num2 = (int)num;
            for (int i = 0; i < num2; i++)
            {
                MoteMaker.ThrowDustPuff(loc, base.pawn.Map, Rand.Range(0.8f, 1.2f));
            }
            lastAbsorbDamageTick = Find.TickManager.TicksGame;
        }

        private Material bubbleMat;

        public Material BubbleMat
        {
            get
            {
                if (bubbleMat is null)
                {
                    bubbleMat = MaterialPool.MatFrom("Other/ShieldBubble", ShaderDatabase.Transparent, this.def.shieldProperties.shieldColor);
                }
                return bubbleMat;
            }
        }
        public void Draw()
        {
            if (this.def.shieldProperties != null && this.ResourceAmount > 0)
            {
                float num = Mathf.Lerp(1.2f, 1.55f, this.def.lifetimeTicks != -1 ? (this.def.lifetimeTicks - duration) / this.def.lifetimeTicks : 1);
                Vector3 drawPos = base.pawn.Drawer.DrawPos;
                drawPos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
                int num2 = Find.TickManager.TicksGame - lastAbsorbDamageTick;
                if (num2 < 8)
                {
                    float num3 = (float)(8 - num2) / 8f * 0.05f;
                    drawPos += impactAngleVect * num3;
                    num -= num3;
                }
                float angle = Rand.Range(0, 360);
                Vector3 s = new Vector3(num, 1f, num);
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(drawPos, Quaternion.AngleAxis(angle, Vector3.up), s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, BubbleMat, 0);
            }
        }
        public float TotalResourceGainAmount()
        {
            float num = 0;

            var comps = HediffResourceUtils.GetAllAdjustHediffsComps(this.pawn);
            foreach (var comp in comps)
            {
                var resourceSettings = comp.ResourceSettings;
                if (resourceSettings != null)
                {
                    foreach (var option in resourceSettings)
                    {
                        if (option.hediff == def)
                        {
                            var num2 = option.resourcePerSecond;
                            if (option.qualityScalesResourcePerSecond && comp.TryGetQuality(out QualityCategory qc))
                            {
                                num2 *= HediffResourceUtils.GetQualityMultiplier(qc);
                            }
                            num += num2;
                        }
                    }
                }
            }

            var hediffCompResourcePerDay = this.TryGetComp<HediffComp_ResourcePerSecond>();
            if (hediffCompResourcePerDay != null)
            {
                num += hediffCompResourcePerDay.Props.resourcePerSecond;
            }
            return num;
        }
        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.Notify_PawnPostApplyDamage(dinfo, totalDamageDealt);
            if (this.def.resourceGainPerDamages != null && this.def.resourceGainPerDamages.resourceGainOffsets.TryGetValue(dinfo.Def.defName, out float resourceGain))
            {
                this.ResourceAmount += resourceGain;
            }
            else if (this.def.resourceGainPerAllDamages != 0f)
            {
                this.ResourceAmount += this.def.resourceGainPerAllDamages;
            }
        }
        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            HRFLog.Message(this.def.defName + " adding resource hediff to " + this.pawn);

            this.resourceAmount = this.def.initialResourceAmount;
            this.duration = 0;
            if (this.def.sendLetterWhenGained && this.pawn.Faction == Faction.OfPlayer)
            {
                Find.LetterStack.ReceiveLetter(this.def.letterTitleKey.Translate(this.pawn.Named("PAWN"), this.def.Named("RESOURCE")),
                    this.def.letterMessageKey.Translate(this.pawn.Named("PAWN"), this.def.Named("RESOURCE")), this.def.letterType, this.pawn);
            }
        }


        public override void PostRemoved()
        {
            base.PostRemoved();
            HRFLog.Message(this.def.defName + " removing resource hediff from " + this.pawn);

            var comps = HediffResourceUtils.GetAllAdjustHediffsComps(this.pawn);
            foreach (var comp in comps)
            {
                var resourceSettings = comp.ResourceSettings;
                if (resourceSettings != null)
                {
                    foreach (var option in resourceSettings)
                    {
                        if (option.dropIfHediffMissing && option.hediff == def)
                        {
                            comp.Drop();
                        }
                    }
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref resourceAmount, "resourceAmount");
            Scribe_Values.Look(ref duration, "duration");
            Scribe_Values.Look(ref delayTicks, "delayTicks");
            Scribe_Collections.Look(ref amplifiers, "amplifiers", LookMode.Reference);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                foreach (var amplifier in amplifiers)
                {
                    cachedAmplifiers[amplifier] = amplifier.TryGetComp<CompAdjustHediffsArea>();
                }
            }
        }
    }
}
