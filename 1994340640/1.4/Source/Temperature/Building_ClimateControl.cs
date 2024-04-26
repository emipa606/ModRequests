using RimWorld;
using UnityEngine;
using Verse;
using WallStuff;
using WallStuff.Temperature;

namespace WallStuff
{
    public class Building_ClimateControl : Building_TempControl, IWallAttachable
    {

        private IntVec3 vecFacing;
        private Room roomFacing;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            vecFacing = Position + IntVec3.North.RotatedBy(Rotation);
        }

        public override void Destroy( DestroyMode mode = DestroyMode.Vanish )
        {
            base.Destroy( mode );
        }

        public override void TickRare()
        {
            if (!Validate()) return;

            ControlTemperature();
        }

        protected virtual bool Validate()
        {
            if (vecFacing.Impassable(this.Map))
            {
                return false;
            }

            roomFacing = vecFacing.GetRoom(this.Map);
            if (roomFacing == null)
            {
                return false;
            }

            return (compPowerTrader == null || compPowerTrader.PowerOn);
        }

        private void ControlTemperature()
        {
            var temperature = roomFacing.Temperature;

            bool operatingAtTemperatureFlag = !Mathf.Approximately(temperature, compTempControl.targetTemperature);

            if (operatingAtTemperatureFlag)
            {
                if (temperature < compTempControl.targetTemperature)
                {
                    TemperatureControl.HeatRoom(roomFacing, vecFacing, base.Map, compTempControl, compPowerTrader);
                }
                else if (temperature > compTempControl.targetTemperature)
                {
                    TemperatureControl.CoolRoom(roomFacing, vecFacing, base.Map, compTempControl, compPowerTrader);
                }
            }
            else
            {
                CompProperties_Power props = compPowerTrader.Props;
                compPowerTrader.PowerOutput = (0f - props.PowerConsumption) * compTempControl.Props.lowPowerConsumptionFactor;
            }

            compTempControl.operatingAtHighPower = operatingAtTemperatureFlag;
        }
    }
}