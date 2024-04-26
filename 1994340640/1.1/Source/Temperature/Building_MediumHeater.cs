using RimWorld;
using UnityEngine;
using Verse;
using WallStuff;

namespace WallStuff
{
    public class Building_MediumHeater : Building_TempControl, IWallAttachable
    {
        private IntVec3 vecNorth;
        private Room roomNorth;
        private bool isWorking;

        private bool WorkingState
        {
            get { return isWorking; }
            set
            {
                isWorking = value;

                if (compPowerTrader == null || compTempControl == null)
                {
                    return;
                }
                if (isWorking)
                {
                    compPowerTrader.PowerOutput = -compPowerTrader.Props.basePowerConsumption;
                }
                else
                {
                    compPowerTrader.PowerOutput = -compPowerTrader.Props.basePowerConsumption*
                                                  compTempControl.Props.lowPowerConsumptionFactor;
                }

                compTempControl.operatingAtHighPower = isWorking;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            vecNorth = Position + IntVec3.North.RotatedBy( Rotation );
        }

        public override void Destroy( DestroyMode mode = DestroyMode.Vanish )
        {
            base.Destroy( mode );
        }

        public override void TickRare()
        {
            if (!Validate())
            {
                WorkingState = false;
                return;
            }

            ControlTemperature();
        }

        protected virtual bool Validate()
        {
            if (vecNorth.Impassable(this.Map))
            {
                return false;
            }

            roomNorth = vecNorth.GetRoom(this.Map);
            if (roomNorth == null)
            {
                return false;
            }

            return (compPowerTrader == null || compPowerTrader.PowerOn);
        }

        private void ControlTemperature()
        {
            var temperature = roomNorth.Temperature;
            float energyMod;
            if (temperature < 20f)
            {
                energyMod = 1f;
            }
            else
            {
                energyMod = temperature > 120f
                    ? 0f
                    : Mathf.InverseLerp( 120f, 20f, temperature );
            }
            var energyLimit = WallStuffSettings.heaterPower*energyMod*4.16666651f;
            var hotAir = GenTemperature.ControlTemperatureTempChange( vecNorth, this.Map, energyLimit,
                                                                      compTempControl.targetTemperature );

            var hotIsHot = !Mathf.Approximately( hotAir, 0f );
            if (hotIsHot)
            {
                roomNorth.Group.Temperature += hotAir;
                WorkingState = true;
            }
            else
            {
                WorkingState = false;
            }
        }
    }
}