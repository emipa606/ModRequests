using System.Linq;
using Verse;

namespace WallStuff
{
    public class PlaceWorker_WallChecker : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing( BuildableDef def, IntVec3 center, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            var things = map.thingGrid.ThingsListAt( center );
            if (things.Exists( s => s is IWallAttachable ))
            {
                if(things.Exists( s => s.Rotation.Equals(rot)))
                {                    
                    return ResourceBank.WallAlreadyOccupied;
                }                
            }
            IntVec3 facingCell = center + rot.FacingCell;
            if ((!GenGrid.InBounds(facingCell, map) || GenGrid.Impassable(facingCell, map)))
            {
                return ResourceBank.SpaceInFrontOfWallNotClear;
            }
            return true;
        }
    }
}