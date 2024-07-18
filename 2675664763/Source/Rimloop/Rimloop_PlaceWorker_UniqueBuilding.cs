using System;
using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimloopMod
{
    public class PlaceWorker_UniqueBuilding : PlaceWorker
    {
		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
		{
			ThingDef thingDef = checkingDef as ThingDef;
			if (thingDef != null)
			{
				bool unique = true;
				foreach (Map i in Current.Game.Maps)
				{
					foreach (Thing x in i.spawnedThings)
                    {
						if (x.def == thingDef) { unique = false; }
                    }
				}

				if (!unique)
				{
					return new AcceptanceReport("Only one of these buildings can be built simultaneously.");
				}
			}
			return true;
		}
	}

}
