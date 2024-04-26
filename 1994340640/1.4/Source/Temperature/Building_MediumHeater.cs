using RimWorld;
using UnityEngine;
using Verse;
using WallStuff;
using WallStuff.Temperature;

namespace WallStuff
{
    public class Building_MediumHeater : Building_TempControl, IWallAttachable
    {
        private IntVec3 vecFacing;
        private Room roomFacing;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            vecFacing = Position + IntVec3.North.RotatedBy(Rotation);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);
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
            TemperatureControl.HeatRoom(roomFacing, vecFacing, base.Map, compTempControl, compPowerTrader);
        }
    }
}