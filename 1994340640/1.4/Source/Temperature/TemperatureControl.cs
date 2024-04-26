using RimWorld;
using UnityEngine;
using Verse;
using WallStuff;

namespace WallStuff.Temperature
{
    class TemperatureControl
    {

        public static void CoolRoom(Room roomFacing, IntVec3 vecFacing, Map map, CompTempControl compTempControl, CompPowerTrader compPowerTrader)
        {
            float lowTempLimit = -20f;
            float highTempLimit = -120f;
            float power = WallStuffSettings.coolerPower;
            var temperature = roomFacing.Temperature;
            float energyMod = ((temperature > lowTempLimit) ? 1f : ((!(temperature < highTempLimit)) ? Mathf.InverseLerp(highTempLimit, lowTempLimit, temperature) : 0f));
                        
            ControlTemperature(roomFacing, vecFacing, map, compTempControl, compPowerTrader, energyMod, power, temperature);
        }

        public static void HeatRoom(Room roomFacing, IntVec3 vecFacing, Map map, CompTempControl compTempControl, CompPowerTrader compPowerTrader)
        {
            float lowTempLimit = 20f;
            float highTempLimit = 120f;
            float power = WallStuffSettings.heaterPower;
            var temperature = roomFacing.Temperature;
            float energyMod = ((temperature < lowTempLimit) ? 1f : ((!(temperature > highTempLimit)) ? Mathf.InverseLerp(highTempLimit, lowTempLimit, temperature) : 0f));

            ControlTemperature(roomFacing, vecFacing, map, compTempControl, compPowerTrader, energyMod, power, temperature);
        }

        private static void ControlTemperature(Room roomFacing, IntVec3 vecFacing, Map map, CompTempControl compTempControl, CompPowerTrader compPowerTrader, float energyMod, float power, float temperature)
        {
            float energyLimit = power * energyMod * 4.16666651f;
            float controlTempChange = GenTemperature.ControlTemperatureTempChange(vecFacing, map, energyLimit, compTempControl.targetTemperature);
            bool atTemperatureFlag = !Mathf.Approximately(controlTempChange, 0f);
            CompProperties_Power props = compPowerTrader.Props;
            if (atTemperatureFlag)
            {
                roomFacing.Temperature += controlTempChange;
                compPowerTrader.PowerOutput = 0f - props.PowerConsumption;
            }
            else
            {
                compPowerTrader.PowerOutput = (0f - props.PowerConsumption) * compTempControl.Props.lowPowerConsumptionFactor;
            }
            compTempControl.operatingAtHighPower = atTemperatureFlag;
        }
    }
}
