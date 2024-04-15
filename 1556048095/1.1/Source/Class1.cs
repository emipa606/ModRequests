using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.Sound;

namespace EnhancedBattery
{
    public class CompChargeBackBatteryPrototype : CompPowerBattery
    {
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (Prefs.DevMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Fill",
                    action = delegate ()
                    {
                        this.SetStoredEnergyPct(1f);
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Empty",
                    action = delegate ()
                    {
                        this.SetStoredEnergyPct(0f);
                    }
                };
            }
            yield break;
        }
    }
    public class CompSpeedChargeBattery : CompPowerBattery
    {
        public float chargeEfficiency = 0;

        public new float AmountCanAccept
        {
            get
            {
                if (this.parent.IsBrokenDown())
                {
                    return 0f;
                }
                CompProperties_Battery props = this.Props;
                return (props.storedEnergyMax - StoredEnergy) / chargeEfficiency;
            }
        }

        public new CompProperties_Battery Props
        {
            get
            {
                return (CompProperties_Battery)this.props;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<float>(ref this.chargeEfficiency, "chargeEfficiency", 0f, false);
            CompProperties_Battery props = this.Props;
            if (StoredEnergy > props.storedEnergyMax)
            {
                SetStoredEnergyPct(1);
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            this.DrawPower(Mathf.Min(5f * CompPower.WattsToWattDaysPerTick, StoredEnergy));
            chargeEfficiency = (Props.efficiency - StoredEnergyPct);
        }

        public new void AddEnergy(float amount)
        {
            if (amount < 0f)
            {
                Log.Error("Cannot add negative energy " + amount, false);
                return;
            }
            if (amount > this.AmountCanAccept)
            {
                amount = this.AmountCanAccept;
            }
            amount *= chargeEfficiency;
            SetStoredEnergyPct((StoredEnergy + amount) / Props.storedEnergyMax);
        }
        public override void ReceiveCompSignal(string signal)
        {
            if (signal == "Breakdown")
            {
                this.DrawPower(this.StoredEnergy);
            }
        }

        public override string CompInspectStringExtra()
        {
            CompProperties_Battery props = this.Props;
            string text = string.Concat(new string[]
            {
                "PowerBatteryStored".Translate(),
                ": ",
                StoredEnergy.ToString("F0"),
                " / ",
                props.storedEnergyMax.ToString("F0"),
                " Wd"
            });
            string text2 = text;
            text = string.Concat(new string[]
            {
                text2,
                "\n",
                "PowerBatteryEfficiency".Translate(),
                ": ",
                (chargeEfficiency * 100f).ToString("F0"),
                "%"
            });
            if (StoredEnergy > 0f)
            {
                text2 = text;
                text = string.Concat(new string[]
                {
                    text2,
                    "\n",
                    "SelfDischarging".Translate(),
                    ": ",
                    5f.ToString("F0"),
                    " W"
                });
            }
            if (PowerNet == null)
            {
                return text+"\n" + "PowerNotConnected".Translate();
            }
            string value = (this.PowerNet.CurrentEnergyGainRate() / CompPower.WattsToWattDaysPerTick).ToString("F0");
            string value2 = this.PowerNet.CurrentStoredEnergy().ToString("F0");

            return text + "\n" + "PowerConnectedRateStored".Translate(value, value2);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (Prefs.DevMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Fill",
                    action = delegate ()
                    {
                        this.SetStoredEnergyPct(1f);
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Empty",
                    action = delegate ()
                    {
                        this.SetStoredEnergyPct(0f);
                    }
                };
            }
            yield break;
        }
    }

    public class CompChargeBackPowerPlantPrototype :CompPowerTrader
    {
        protected CompPowerBattery compPowerBattery;
        protected CompBreakdownable breakdownableComp;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            this.breakdownableComp = this.parent.GetComp<CompBreakdownable>();
            if (base.Props.basePowerConsumption < 0f && !this.parent.IsBrokenDown() && FlickUtility.WantsToBeOn(this.parent))
            {
                base.PowerOn = true;
            }
            if(parent.GetComp<CompPowerBattery>()!=null)
            {
                compPowerBattery = parent.GetComp<CompPowerBattery>();
            }
        }

        public override void PostExposeData()
        {
            Thing thing = null;
            if (Scribe.mode == LoadSaveMode.Saving && this.connectParent != null)
            {
                thing = this.connectParent.parent;
            }
            //Scribe_References.Look<Thing>(ref thing, "parentThing", false);
            if (thing != null)
            {
                this.connectParent = ((ThingWithComps)thing).GetComp<CompPower>();
            }
            if (Scribe.mode == LoadSaveMode.PostLoadInit && this.connectParent != null)
            {
                this.ConnectToTransmitter(this.connectParent, true);
            }
        }
        public override void CompTick()
        {
            base.CompTick();
            this.UpdateDesiredPowerOutput();
        }
        public override void SetUpPowerVars()
        {
            base.SetUpPowerVars();
            CompProperties_Power props = base.Props;
            if (compPowerBattery != null)
            {
                if (compPowerBattery.StoredEnergyPct > 0.9)
                {
                    PowerOutput = -Props.basePowerConsumption;
                    return;
                }
            }
            PowerOutput = 0;
        }
        public virtual void UpdateDesiredPowerOutput()
        {
            if ((this.breakdownableComp != null && this.breakdownableComp.BrokenDown) || (this.flickableComp != null && !this.flickableComp.SwitchIsOn) || !base.PowerOn)
            {
                base.PowerOutput = 0f;
                return;
            }
            if (compPowerBattery != null)
            {
                if (compPowerBattery.StoredEnergyPct > 0.8)
                {
                    PowerOutput = -Props.basePowerConsumption;
                    return;
                }
            }
            PowerOutput = 0;
        }

        public override string CompInspectStringExtra()
        {
            if (compPowerBattery != null)
            {
                if (compPowerBattery.PowerNet == null)
                {
                    return string.Empty;
                }
            }
            else
            {
                return string.Empty;
            }
            string str;
            str = "PowerOutput".Translate() + ": " + this.PowerOutput.ToString("#####0") + " W";
            return str;
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            yield break;
        }
    }
    public class CompChargeBackPowerPlant : CompChargeBackPowerPlantPrototype
    {
        public float currentPowerEfficiency = 1.0f;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            currentPowerEfficiency = 0;
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<float>(ref currentPowerEfficiency, "currentPowerEfficiency");
        }
        public override void UpdateDesiredPowerOutput()
        {
            if ((this.breakdownableComp != null && this.breakdownableComp.BrokenDown) || (this.flickableComp != null && !this.flickableComp.SwitchIsOn) || !base.PowerOn)
            {
                base.PowerOutput = 0f;
                return;
            }
            if (compPowerBattery != null)
            {
                if(compPowerBattery.StoredEnergyPct>0.99)
                {
                    currentPowerEfficiency = 1;
                }
                else
                {
                    int tempEff = (int)(compPowerBattery.StoredEnergyPct * 100);
                    tempEff /= 10;
                    currentPowerEfficiency = ((float)tempEff) / 10;
                }
                PowerOutput = -(Props.basePowerConsumption * currentPowerEfficiency);
                return;
            }
            PowerOutput = 0;
        }
    }
    public class CompSpeedChargeBackPowerPlant : CompChargeBackPowerPlant
    {
        public override void UpdateDesiredPowerOutput()
        {
            if ((this.breakdownableComp != null && this.breakdownableComp.BrokenDown) || (this.flickableComp != null && !this.flickableComp.SwitchIsOn) || !base.PowerOn)
            {
                base.PowerOutput = 0f;
                return;
            }
            if (compPowerBattery != null)
            {
                currentPowerEfficiency = (1 - compPowerBattery.StoredEnergyPct);
                PowerOutput = -(Props.basePowerConsumption * currentPowerEfficiency);
                return;
            }
            PowerOutput = 0;
        }
    }
    public class CompResonancePowerPlant : CompChargeBackPowerPlant
    {
        public static float range = 2;
        public float additionalEfficiency=0.2f;
        HashSet<Thing> linkedBuildings = new HashSet<Thing>();

        public static HashSet<CompResonancePowerPlant> nearDrowingList = new HashSet<CompResonancePowerPlant>();

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            this.breakdownableComp = this.parent.GetComp<CompBreakdownable>();
            if (base.Props.basePowerConsumption < 0f && !this.parent.IsBrokenDown() && FlickUtility.WantsToBeOn(this.parent))
            {
                base.PowerOn = true;
            }
            if (parent.GetComp<CompPowerBattery>() != null)
            {
                compPowerBattery = parent.GetComp<CompPowerBattery>();
            }
            currentPowerEfficiency = 1;
            additionalEfficiency = ((CompPropertie_ResonancePowerPlant)Props).additionalEfficiency;
            linkedBuildings.Add(parent);
            LinkToNearbyBuildings();
        }
        public override void PostDeSpawn(Map map)
        {
            UnlinkAll();
        }
        public bool AddLink(Thing thing)
        {
            if(linkedBuildings.Add(thing))
            {
                currentPowerEfficiency += additionalEfficiency;
                return true;
            }
            return false;
        }
        public bool RemoveLink(Thing thing)
        {
            if(linkedBuildings.Remove(thing))
            {
                currentPowerEfficiency -= additionalEfficiency;
                return true;
            }
            return false;
        }

        public void LinkToNearbyBuildings()
        {
            foreach (Thing thing in GenRadial.RadialDistinctThingsAround(parent.Position, parent.Map, range, true))
            {
                CompResonancePowerPlant comp = thing.TryGetComp<CompResonancePowerPlant>();
                if (comp != null)
                {
                    if (comp.AddLink(parent))
                    {
                        linkedBuildings.Add(comp.parent);
                        currentPowerEfficiency += additionalEfficiency;
                    }
                }
            }
            foreach (Thing thing in GenRadial.RadialDistinctThingsAround(parent.Position+parent.Rotation.FacingCell, parent.Map, range, true))
            {
                CompResonancePowerPlant comp = thing.TryGetComp<CompResonancePowerPlant>();
                if (comp != null)
                {
                    if (comp.AddLink(parent))
                    {
                        linkedBuildings.Add(comp.parent);
                        currentPowerEfficiency += additionalEfficiency;
                    }
                }
            }
        }
        public void UnlinkAll()
        {
            List<Thing> tmpList = new List<Thing>(linkedBuildings);
            for (int index = 0; index < tmpList.Count; index++)
            {
                tmpList[index].TryGetComp<CompResonancePowerPlant>().RemoveLink(parent);
            }
            linkedBuildings.Clear();
        }
        public override void UpdateDesiredPowerOutput()
        {
            if ((this.breakdownableComp != null && this.breakdownableComp.BrokenDown) || (this.flickableComp != null && !this.flickableComp.SwitchIsOn) || !base.PowerOn)
            {
                base.PowerOutput = 0f;
                return;
            }
            if (compPowerBattery != null)
            {
                PowerOutput = -(Props.basePowerConsumption * currentPowerEfficiency);
                return;
            }
            PowerOutput = 0;
        }
        public override string CompInspectStringExtra()
        {
            if(compPowerBattery!=null)
            {
                if (compPowerBattery.PowerNet == null)
                {
                    return string.Empty;
                }
            }
            else
            {
                return string.Empty;
            }
            string str;
            str = "PowerOutput".Translate() + ": " + (-Props.basePowerConsumption * currentPowerEfficiency).ToString("#####0") + " W";
            return str;
            
        }
        public override void PostDrawExtraSelectionOverlays()
        {
            foreach (Thing t in linkedBuildings)
            {
                if (t.TryGetComp<CompResonancePowerPlant>().CanBeActive)
                {
                    GenDraw.DrawLineBetween(this.parent.TrueCenter(), t.TrueCenter());
                }
                else
                {
                    GenDraw.DrawLineBetween(this.parent.TrueCenter(), t.TrueCenter(), CompAffectedByFacilities.InactiveFacilityLineMat);
                }
            }
        }
        public bool CanBeActive
        {
            get
            {
                CompPowerTrader compPowerTrader = this.parent.TryGetComp<CompPowerTrader>();
                return compPowerTrader == null || compPowerTrader.PowerOn;
            }
        }
        public static void DrawLinesToPotentialThingsToLinkTo(ThingDef myDef, IntVec3 myPos, Rot4 myRot, Map map)
        {
            nearDrowingList.Clear();
            Vector3 pointingPos = GenThing.TrueCenter(myPos, myRot, myDef.size, myDef.Altitude);
            foreach (Thing thing in GenRadial.RadialDistinctThingsAround(myPos, map, range, true))
            {
                CompResonancePowerPlant comp = thing.TryGetComp<CompResonancePowerPlant>();
                if (comp != null)
                {
                    nearDrowingList.Add(comp);
                }
            }
            foreach (Thing thing in GenRadial.RadialDistinctThingsAround(myPos+myRot.FacingCell, map, range, true))
            {
                CompResonancePowerPlant comp = thing.TryGetComp<CompResonancePowerPlant>();
                if (comp != null)
                {
                    nearDrowingList.Add(comp);
                }
            }
            foreach (CompResonancePowerPlant comp in nearDrowingList)
            {
                if (comp.CanBeActive)
                {
                    GenDraw.DrawLineBetween(pointingPos, comp.parent.TrueCenter());
                }
                else
                {
                    GenDraw.DrawLineBetween(pointingPos, comp.parent.TrueCenter(), CompAffectedByFacilities.InactiveFacilityLineMat);
                }
            }
        }
    }

    [StaticConstructorOnStartup]
    public class Building_ChargeBackBattery : Building
    {
        public int ticksToExplode;

        public Sustainer wickSustainer;

        public static readonly Vector2 BarSize = new Vector2(1.3f, 0.4f);

        public const float MinEnergyToExplode = 500f;

        public const float EnergyToLoseWhenExplode = 400f;

        public const float ExplodeChancePerDamage = 0.05f;

        public static readonly Material BatteryBarFilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.9f, 0.85f, 0.2f), false);

        public static readonly Material BatteryBarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f), false);

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.ticksToExplode, "ticksToExplode", 0, false);
        }

        public override void Draw()
        {
            base.Draw();
            DrawEnergyBar();
            if (this.ticksToExplode > 0 && base.Spawned)
            {
                base.Map.overlayDrawer.DrawOverlay(this, OverlayTypes.BurningWick);
            }
        }

        public virtual void DrawEnergyBar()
        {
            CompPowerBattery comp = base.GetComp<CompPowerBattery>();
            GenDraw.FillableBarRequest r = default(GenDraw.FillableBarRequest);
            r.center = this.DrawPos + Vector3.up * 0.1f;
            r.size = Building_ChargeBackBattery.BarSize;
            r.fillPercent = comp.StoredEnergy / comp.Props.storedEnergyMax;
            r.filledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.2f, 0.85f, 0.85f), false);
            r.unfilledMat = BatteryBarUnfilledMat;
            r.margin = 0.15f;
            Rot4 rotation = base.Rotation;
            rotation.Rotate(RotationDirection.Clockwise);
            r.rotation = rotation;
            GenDraw.DrawFillableBar(r);
        }

        public override void Tick()
        {
            base.Tick();
            if (this.ticksToExplode > 0)
            {
                if (this.wickSustainer == null)
                {
                    this.StartWickSustainer();
                }
                else
                {
                    this.wickSustainer.Maintain();
                }
                this.ticksToExplode--;
                if (this.ticksToExplode == 0)
                {
                    IntVec3 randomCell = this.OccupiedRect().RandomCell;
                    float radius = Rand.Range(0.5f, 1f) * 3f;
                    GenExplosion.DoExplosion(randomCell, base.Map, radius, DamageDefOf.Flame, null, -1, -1f, null, null, null, null, null, 0f, 1, false, null, 0f, 1, 0f, false);
                    base.GetComp<CompPowerBattery>().DrawPower(400f);
                }
            }
        }

        public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.PostApplyDamage(dinfo, totalDamageDealt);
            if (!base.Destroyed && this.ticksToExplode == 0 && dinfo.Def == DamageDefOf.Flame && Rand.Value < 0.05f && base.GetComp<CompPowerBattery>().StoredEnergy > 500f)
            {
                this.ticksToExplode = Rand.Range(70, 150);
                this.StartWickSustainer();
            }
        }

        private void StartWickSustainer()
        {
            SoundInfo info = SoundInfo.InMap(this, MaintenanceType.PerTick);
            this.wickSustainer = SoundDefOf.HissSmall.TrySpawnSustainer(info);
        }
    }
    [StaticConstructorOnStartup]
    public class Building_SpeedChargeBattery : Building_ChargeBackBattery
    {
        public override void DrawEnergyBar()
        {
            CompSpeedChargeBattery comp = base.GetComp<CompSpeedChargeBattery>();
            GenDraw.FillableBarRequest r = default(GenDraw.FillableBarRequest);
            r.center = this.DrawPos + Vector3.up * 0.1f;
            r.size = Building_SpeedChargeBattery.BarSize;
            r.fillPercent = comp.StoredEnergy / comp.Props.storedEnergyMax;
            r.filledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.25f + comp.StoredEnergyPct, 1.2f - comp.StoredEnergyPct * 1.2f, 1f - comp.StoredEnergyPct), false);
            r.unfilledMat = BatteryBarUnfilledMat;
            r.margin = 0.15f;
            Rot4 rotation = base.Rotation;
            rotation.Rotate(RotationDirection.Clockwise);
            r.rotation = rotation;
            GenDraw.DrawFillableBar(r);
        }
    }

    public class CompPropertie_ResonancePowerPlant : CompProperties_Power
    {
        public float additionalEfficiency = 0.2f;
        public float range = 2;
        public override void ResolveReferences(ThingDef parentDef)
        {
            CompResonancePowerPlant.range = range;
        }
    }

    public class PlaceWorker_ShowResonanceLink : PlaceWorker
    {
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            Map currentMap = Find.CurrentMap;
            if (def.HasComp(typeof(CompResonancePowerPlant)))
            {
                CompResonancePowerPlant.DrawLinesToPotentialThingsToLinkTo(def, center, rot, currentMap);
            }
        }
    }


    public class DamageWorker_Void : DamageWorker_AddInjury
    {
        protected override BodyPartRecord ChooseHitPart(DamageInfo dinfo, Pawn pawn)
        {
            return pawn.health.hediffSet.GetRandomNotMissingPart(dinfo.Def, dinfo.Height, BodyPartDepth.Outside, null);
        }

        protected override void ApplySpecialEffectsToPart(Pawn pawn, float totalDamage, DamageInfo dinfo, DamageResult result)
        {
            if (dinfo.HitPart.depth == BodyPartDepth.Inside)
            {
                List<BodyPartRecord> list = new List<BodyPartRecord>();
                for (BodyPartRecord bodyPartRecord = dinfo.HitPart; bodyPartRecord != null; bodyPartRecord = bodyPartRecord.parent)
                {
                    list.Add(bodyPartRecord);
                    if (bodyPartRecord.depth == BodyPartDepth.Outside)
                    {
                        break;
                    }
                }
                float num = (float)(list.Count - 1) + 0.5f;
                for (int i = 0; i < list.Count; i++)
                {
                    DamageInfo dinfo2 = dinfo;
                    dinfo2.SetHitPart(list[i]);
                    base.FinalizeAndAddInjury(pawn, totalDamage / num * ((i != 0) ? 1f : 0.5f), dinfo2, result);
                }
            }
            else
            {
                int num2 = (def.cutExtraTargetsCurve == null) ? 0 : GenMath.RoundRandom(def.cutExtraTargetsCurve.Evaluate(Rand.Value));
                List<BodyPartRecord> list2;
                if (num2 != 0)
                {
                    IEnumerable<BodyPartRecord> enumerable = dinfo.HitPart.GetDirectChildParts();
                    if (dinfo.HitPart.parent != null)
                    {
                        enumerable = enumerable.Concat(dinfo.HitPart.parent);
                        if (dinfo.HitPart.parent.parent != null)
                        {
                            enumerable = enumerable.Concat(dinfo.HitPart.parent.GetDirectChildParts());
                        }
                    }
                    list2 = (from x in enumerable.Except(dinfo.HitPart).InRandomOrder(null).Take(num2)
                             where !x.def.conceptual
                             select x).ToList();
                }
                else
                {
                    list2 = new List<BodyPartRecord>();
                }
                list2.Add(dinfo.HitPart);
                float num3 = totalDamage * (1f + def.cutCleaveBonus) / (list2.Count + def.cutCleaveBonus);
                if (num2 == 0)
                {
                    num3 = base.ReduceDamageToPreserveOutsideParts(num3, dinfo, pawn);
                }
                for (int j = 0; j < list2.Count; j++)
                {
                    DamageInfo dinfo3 = dinfo;
                    dinfo3.SetHitPart(list2[j]);
                    base.FinalizeAndAddInjury(pawn, num3, dinfo3, result);
                }
            }

        }
        public override void ExplosionStart(Explosion explosion, List<IntVec3> cellsToAffect)
        {
            GenTemperature.PushHeat(explosion.Position, explosion.Map, def.explosionHeatEnergyPerCell * cellsToAffect.Count);
            MoteMaker.MakeStaticMote(explosion.Position, explosion.Map, ThingDefOf.Mote_ExplosionFlash, explosion.radius * 6f);
            if (explosion.Map == Find.CurrentMap)
            {
                float magnitude = (explosion.Position.ToVector3Shifted() - Find.Camera.transform.position).magnitude;
                Find.CameraDriver.shaker.DoShake(4f * explosion.radius / magnitude);
            }
            ExplosionVisualEffectCenter(explosion);
        }
    }
}
