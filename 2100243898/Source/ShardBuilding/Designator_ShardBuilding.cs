using DTechprinting.Comps;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace DTechprinting
{
    class Designator_ShardBuilding : Designator_Deconstruct
    {
		protected override DesignationDef Designation
		{
			get
			{
				return Base.DefOf.ShardBuilding;
			}
		}

		public Designator_ShardBuilding()
		{
			this.defaultLabel = "techprint".Translate();
			this.defaultDesc = "techprintDesc".Translate();
			this.icon = ContentFinder<Texture2D>.Get("Things/Item/Special/Techshard/Techshard_c", true);
			this.soundDragSustain = SoundDefOf.Designate_DragStandard;
			this.soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
			this.useMouseIcon = true;
			this.soundSucceeded = SoundDefOf.Designate_Deconstruct;
		}

		public override void DesignateThing(Thing t)
		{
			base.Map.designationManager.RemoveAllDesignationsOn(t, false);
			base.Map.designationManager.AddDesignation(new Designation(t, this.Designation));
		}

		public override AcceptanceReport CanDesignateThing(Thing t)
		{
			Building building = t.GetInnerIfMinified() as Building;
			CompShardable comp = building.TryGetComp<CompShardable>();
			if (comp == null || comp.shard == null || !Base.ThingIsPrintable(building.def))
				return false;
			return base.CanDesignateThing(t);
		}
	}
}
