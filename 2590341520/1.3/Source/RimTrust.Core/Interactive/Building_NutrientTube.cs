using RimTrust.Trade;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimTrust.Core.Interactive
{
    [StaticConstructorOnStartup]
    public class Building_NutrientTube : Building
    {
        public CompPowerTrader powerComp;
        internal static bool debug = false;

        private int NutrientCount;
        private float progressInt;
        private Material barFilledCachedMat;
        public const int MaxCapacity = 75;  // 30 days worth of food, 30 nutrition, which needs ~4 nutrition input <=> 80 rice/corn) 
        private const int BaseProductionDuration = 3600;   //360000 -> 3600
        private static readonly Vector2 BarSize = new Vector2(0.55f, 0.1f);
        private static readonly Color BarZeroProgressColor = new Color(0.4f, 0.27f, 0.22f);
        private static readonly Color BarProducedColor = new Color(0.9f, 0.85f, 0.2f);
        private static readonly Material BarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f), false);
        public float lowPowerConsumptionFactor = 0.1f;

        public float Progress
        {
            get
            {
                return this.progressInt;
            }
            set
            {
                if (value == this.progressInt)
                {
                    return;
                }
                this.progressInt = value;
                this.barFilledCachedMat = null;
            }
        }

        private Material BarFilledMat
        {
            get
            {
                if (this.barFilledCachedMat == null)
                {
                    this.barFilledCachedMat = SolidColorMaterials.SimpleSolidColorMaterial(Color.Lerp(Building_NutrientTube.BarZeroProgressColor, Building_NutrientTube.BarProducedColor, this.Progress), false);
                }
                return this.barFilledCachedMat;
            }
        }

        public int SpaceLeftForNutrient
        {
            get
            {
                if (this.Produced)
                {
                    return 0;
                }
                return 75 - this.NutrientCount;
            }
        }
        private bool Empty
        {
            get
            {
                return this.NutrientCount <= 0;
            }
        }
        public bool Produced
        {
            get
            {
                return !this.Empty && this.Progress >= 1f;
            }
        }

        private float CurrentPowerProgressSpeedFactor
        {
            get
            {
                CompProperties_TemperatureRuinable compProperties = this.def.GetCompProperties<CompProperties_TemperatureRuinable>();
                float ambientTemperature = base.AmbientTemperature;
                bool powerOn = this.powerComp.PowerOn;
                if (!this.powerComp.PowerOn)
                {
                    return 0f;
                }
                if (powerOn)
                {
                    if (ambientTemperature < 7f)
                    {
                        return GenMath.LerpDouble(compProperties.minSafeTemperature, 7f, 0.1f, 1f, ambientTemperature);
                    }
                }
                return 1f;
            }
        }

        private float ProgressPerTickAtPowerState
        {
            get
            {
                return 2.77777781E-06f * this.CurrentPowerProgressSpeedFactor;
            }
        }
        private int EstimatedTicksLeft
        {
            get
            {
                return Mathf.Max(Mathf.RoundToInt((1f - this.Progress) / this.ProgressPerTickAtPowerState), 0);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.NutrientCount, "NutrientCount", 0, false);
            Scribe_Values.Look<float>(ref this.progressInt, "progress", 0f, false);
        }
        public override void TickRare()
        {
            base.TickRare();
            if (!this.Empty)
            {
                this.Progress = Mathf.Min(this.Progress + 10000f * this.ProgressPerTickAtPowerState, 1f);
                CompProperties_Power props = this.powerComp.Props;
                if (this.Produced)
                {
                    this.powerComp.PowerOutput = -props.basePowerConsumption * this.lowPowerConsumptionFactor;
                }
                else if (!this.Produced)
                {
                    this.powerComp.PowerOutput = -props.basePowerConsumption;
                }
            }
            else if (this.Empty)
            {
                CompProperties_Power props = this.powerComp.Props;
                this.powerComp.PowerOutput = -props.basePowerConsumption * this.lowPowerConsumptionFactor;
            }
        }
        public void AddNutrition(int count)
        {
            base.GetComp<CompTemperatureRuinable>().Reset();
            if (this.Produced)
            {
                if (debug)
                {
                    //Log.Warning("Tried to add nutrition to a full nutrient tube. Colonists should take the nutrient supplement pills first.", false);
                }
                return;
            }
            int num = Mathf.Min(count, 75 - this.NutrientCount);
            if (num <= 0)
            {
                return;
            }
            this.Progress = GenMath.WeightedAverage(0f, (float)num, this.Progress, (float)this.NutrientCount);
            this.NutrientCount += num;
        }

        protected override void ReceiveCompSignal(string signal)
        {
            if (signal == "RuinedByPowerloss")
            {
                this.Reset();
            }
        }
        private void Reset()
        {
            this.NutrientCount = 0;
            this.Progress = 0f;
        }
        public void AddNutrition(Thing plantMatter)
        {
            int num = Mathf.Min(plantMatter.stackCount, 75 - this.NutrientCount);
            if (num > 0)
            {
                this.AddNutrition(num);
                plantMatter.SplitOff(num).Destroy(DestroyMode.Vanish);
            }
        }

        public static bool IsAcceptableFeedstock(ThingDef def)
        {
            return def.IsNutritionGivingIngestible && def.ingestible.preferability != FoodPreferability.Undefined && (def.ingestible.foodType & FoodTypeFlags.Plant) != FoodTypeFlags.Plant && (def.ingestible.foodType & FoodTypeFlags.Tree) != FoodTypeFlags.Tree;
        }
        
        public override string GetInspectString()
        {
            //Log.Message("Building_NutrientTube_GetInspectString()");
            StringBuilder stringBuilder = new StringBuilder();
            //Log.Message("Building_NutrientTube_NewStringBuilder()");
            stringBuilder.Append(base.GetInspectString());
            if (stringBuilder.Length != 0)
            {
                stringBuilder.AppendLine();
            }
            CompTemperatureRuinable comp = base.GetComp<CompTemperatureRuinable>();
            //Log.Message("CompTemperatureRuinable comp = base.GetComp<CompTemperatureRuinable>()");
            if (!this.Empty && !comp.Ruined)
            {
                //Log.Message("if (!this.Empty && !comp.Ruined)");
                if (this.Produced)
                {
                    //Log.Message("if (this.Produced)");
                    stringBuilder.AppendLine("ContainsNutrientSupplementPills".Translate((this.NutrientCount / 3), 25));
                }
                else
                {
                    //Log.Message("if (this.Produced) else ");
                    stringBuilder.AppendLine("ContainsNutrients".Translate(this.NutrientCount, 75));
                }
            }
            if (!this.Empty)
            {
                //Log.Message("if (!this.Empty");
                if (this.Produced)
                {
                    stringBuilder.AppendLine("Produced".Translate());
                }
                else
                {
                    stringBuilder.AppendLine("ProductionProgress".Translate(this.Progress.ToStringPercent(), this.EstimatedTicksLeft.ToStringTicksToPeriod(true, false, true, true)));
                    if (this.CurrentPowerProgressSpeedFactor != 1f)
                    {
                        stringBuilder.AppendLine("NutrientTubeOutOfIdealTemperature".Translate(this.CurrentPowerProgressSpeedFactor.ToStringPercent()));
                    }
                }
            }
            stringBuilder.AppendLine("Temperature".Translate() + ": " + base.AmbientTemperature.ToStringTemperature("F0"));
            stringBuilder.AppendLine("IdealFermentingTemperature".Translate() + ": " + 7f.ToStringTemperature("F0") + " ~ " + comp.Props.maxSafeTemperature.ToStringTemperature("F0"));
            return stringBuilder.ToString().TrimEndNewlines();
        }

        
        public Thing TakeOutNutrientSupplementPills()
        {
            if (!this.Produced)
            {
                //Log.Warning("Tried to get nutrient supplement pills but it's not yet produced.", false);
                return null;
            }
            Thing thing = ThingMaker.MakeThing(CoreDefOf.MealNutrientSupplementPill, null);
            thing.stackCount =(this.NutrientCount / 3);
            this.Reset();
            return thing;
        }
        public override void Draw()
        {
            base.Draw();
            if (!this.Empty)
            {
                Vector3 drawPos = this.DrawPos;
                drawPos.y += 0.042857144f;
                drawPos.z += 0.25f;
                GenDraw.DrawFillableBar(new GenDraw.FillableBarRequest
                {
                    center = drawPos,
                    size = Building_NutrientTube.BarSize,
                    fillPercent = (float)this.NutrientCount / 75f,
                    filledMat = this.BarFilledMat,
                    unfilledMat = Building_NutrientTube.BarUnfilledMat,
                    margin = 0.1f,
                    rotation = Rot4.North
                });
            }
        }
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            //IEnumerator<Gizmo> enumerator = null;
            if (Prefs.DevMode && !this.Empty)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Debug: Set progress to 1",
                    action = delegate ()
                    {
                        this.Progress = 1f;
                    }
                };
            }
            yield break;
        }
        
        public bool CanUseNutrientTubeNow
        {
            get
            {
                if (!base.Spawned || !base.Map.gameConditionManager.ConditionIsActive(GameConditionDefOf.SolarFlare))
                {
                    return powerComp.PowerOn;
                }
                return false;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            powerComp = GetComp<CompPowerTrader>();
        }

        /*
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            (!selPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Some))
            {
                FloatMenuOption item = new FloatMenuOption("CannotUseNoPath".Translate(), null);
                return new List<FloatMenuOption>
            {
                item
            };
            }
            if (base.Spawned && base.Map.gameConditionManager.ConditionIsActive(GameConditionDefOf.SolarFlare))
            {
                FloatMenuOption item2 = new FloatMenuOption("CannotUseSolarFlare".Translate(), null);
                return new List<FloatMenuOption>
            {
                item2
            };
            }
            if (!powerComp.PowerOn)
            {
                FloatMenuOption item3 = new FloatMenuOption("CannotUseNoPower".Translate(), null);
                return new List<FloatMenuOption>
            {
                item3
            };
            }
            if (!selPawn.health.capacities.CapableOf(PawnCapacityDefOf.Sight))
            {
                FloatMenuOption item4 = new FloatMenuOption("CannotUseReason".Translate("IncapableOfCapacity".Translate(PawnCapacityDefOf.Sight.label)), null);
                return new List<FloatMenuOption>
            {
                item4
            };
            }
            return FloatMenuManagerNutrientTube.RequestBuild(this, selPawn);
        } 
        */
    }
}