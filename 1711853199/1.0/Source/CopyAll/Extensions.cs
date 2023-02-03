using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Copyprint
{
    public static class Extensions
    {
        #region Methods

        public static bool IsPawn( this Thing thing)
        {
            return thing is Pawn;
        }

        /*
         * public static bool IsValidCopyprintTerrain( this TerrainDef terrain )
        {
            return terrain.designationCategory != null;
        }
        
        public static bool IsValidCopyprintThing( this Thing thing )
        {
            if ( thing is Blueprint )
            {
                return ( thing as Blueprint).def.entityDefToBuild.designationCategory != null && thing.Faction == Faction.OfPlayer;
            }

            if ( thing is Frame )
            {
                return (thing as Frame).def.entityDefToBuild.designationCategory != null && thing.Faction == Faction.OfPlayer;
            }

            return thing.def.designationCategory != null && thing.Faction == Faction.OfPlayer;
        }*/

        #endregion Methods
    }
}
