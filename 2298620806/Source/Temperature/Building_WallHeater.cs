using WallHeater;
using RimWorld;
using Verse;
using UnityEngine;


namespace WallHeater
{
    public class Building_WallHeater : Building_Heater
    {

        private IntVec3 vecNorth;
        private IntVec3 dx;
        private Room roomNorth;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            dx = IntVec3.North.RotatedBy( Rotation );
            vecNorth = Position + dx;
        }
        public override void TickRare()
        {
            base.Position += dx;
            base.TickRare();
            base.Position -= dx;
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
    }
}
