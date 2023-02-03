using DubsBadHygiene;
using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace DBHExtension
{
    public class HydroElectricWaterStorageProperties : CompProperties_WaterStorage
    {
		public float waterOut;
        public HydroElectricWaterStorageProperties()=>compClass=typeof(HydroElectricWaterStorageComp);
    }
	
    public class HydroElectricWaterStorageComp : CompWaterStorage
    {
		private bool draining;
		public bool Draining
        {
            get
            {
				return draining;
            }
        }

        public override void CompTick()
        {
			if ((this.Props.Auto || (this.Props.AutoOnRain && this.parent.Map.weatherManager.RainRate > 0.01f)) && this.parent.IsHashIntervalTick(this.Props.Tickrate) && this.WorkingNow)
			{
				float num = this.Props.AutoGenRate;
				if (this.Props.AutoOnRain)
				{
					num *= this.parent.Map.weatherManager.RainRate;
				}
				this.WaterStorage += num;
				WaterStorage = WaterStorage > Props.WaterStorageCap ? Props.WaterStorageCap : WaterStorage;
				if (float.IsNaN(this.WaterStorage))
				{
					this.WaterStorage = 0f;
					Log.Error("NaN on WaterStorage in CompTick1");
				}
			}
			CompPipe pipeComp = this.PipeComp;
			if (((pipeComp != null) ? pipeComp.pipeNet : null) != null)
			{
				if (!draining)
				{
					WaterStorage += PipeComp.pipeNet.WaterPumpedPerTick;
					WaterStorage = ModOption.WaterPumpCapacity.Val == 0f || WaterStorage > Props.WaterStorageCap ? Props.WaterStorageCap : WaterStorage < 0f ? 0f : WaterStorage;
					if (float.IsNaN(WaterStorage))
					{
						WaterStorage = 0f;
						Log.Error("NaN on WaterStorage in CompTick2");
					}
					if (parent.IsHashIntervalTick(5000))
					{
						WaterQuality = PipeComp.pipeNet.HasFilter ? WaterQuality = ContaminationLevel.Treated : WaterQuality != ContaminationLevel.Contaminated ? WaterQuality = ContaminationLevel.Untreated : WaterQuality;
					}
                }
                else if(WaterStorage>0 && draining)
                {
					WaterStorage -= Props.waterOut/60;
                }
				if (WaterStorage == 0)
					draining = false;
			}
            base.CompTick();
        }

		public void StartDraining()
        {
			draining= true;
        }

		public void StopDraining()
        {
			draining = false;
        }

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (!draining)
			{
				yield return new Command_Action
				{
					action = new Action(this.StartDraining),
					defaultLabel = "HydroElectricDrain".Translate(),
					defaultDesc = "HydroElectricDrainDesc".Translate(),
					icon = ContentFinder<Texture2D>.Get("DBH/UI/drainOut", true)
				};
			}
			else
			{
				yield return new Command_Action
				{
					action = new Action(this.StopDraining),
					defaultLabel = "HydroElectricStopTraining".Translate(),
					defaultDesc = "HydroElectricStopDrainingDesc".Translate(),
					icon = ContentFinder<Texture2D>.Get("DBH/UI/drainOut", true)
				};
			}
			if (DebugSettings.godMode)
			{
				yield return new Command_Action
				{
					action = new Action(this.FillTank),
					defaultLabel = "Fill"
				};
				yield return new Command_Action
				{
					action = new Action(this.contam),
					defaultLabel = "contam"
				};
			}
			yield break;
		}


		public HydroElectricWaterStorageProperties Props
        {
            get
            {
                return (HydroElectricWaterStorageProperties)props;
            }
        }
    }


	public class HydroElectricPowerPlant : CompPowerPlant
    {
		private HydroElectricWaterStorageComp waterStorage;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
			this.waterStorage=parent.TryGetComp<HydroElectricWaterStorageComp>();
            base.PostSpawnSetup(respawningAfterLoad);
        }
        protected override float DesiredPowerOutput
        {
            get
            {
				return waterStorage.Draining ? base.DesiredPowerOutput : 0f;
            }
        }
    }
}
