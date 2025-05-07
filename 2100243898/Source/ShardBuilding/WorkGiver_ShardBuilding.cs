using DTechprinting.Comps;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace DTechprinting
{
    public class WorkGiver_ShardBuilding : WorkGiver_Deconstruct
    {
		protected override DesignationDef Designation
		{
			get
			{
				return Base.DefOf.ShardBuilding;
			}
		}

		protected override JobDef RemoveBuildingJob
		{
			get
			{
				return Base.DefOf.ShardBuildingJob;
			}
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Building building = t.GetInnerIfMinified() as Building;
			return base.HasJobOnThing(pawn, t, forced) && Base.ThingIsPrintable(building.def);
		}
	}
}
