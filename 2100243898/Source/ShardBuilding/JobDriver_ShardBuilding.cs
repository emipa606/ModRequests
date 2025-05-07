using DTechprinting.Comps;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using Verse;
using Verse.AI;

namespace DTechprinting
{
    class JobDriver_ShardBuilding : JobDriver_Deconstruct
    {
		protected override DesignationDef Designation
		{
			get
			{
				return Base.DefOf.ShardBuilding;
			}
		}
		protected override void FinishedRemoving()
		{
			Thing shards = ShardMaker.MakeShards(base.Target);
			IntVec3 pos = base.Target.Position;
			Map map = base.Target.Map;
			base.Target.Destroy(DestroyMode.Vanish);
			GenPlace.TryPlaceThing(shards, pos, map, ThingPlaceMode.Near);	
			this.pawn.records.Increment(RecordDefOf.ThingsDeconstructed);
		}

		protected override void TickAction()
		{
			if (base.Building.def.CostListAdjusted(base.Building.Stuff, true).Count > 0)
			{
				this.pawn.skills.Learn(SkillDefOf.Intellectual, 0.25f, false);
			}
		}


	}
}
