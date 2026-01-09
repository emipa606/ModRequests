using HugsLib.Utils;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace KriilMod_CD
{
	public class Designator_BaseTrainCombat : Designator
	{
		public DesignationDef defOf = null;

		public override int DraggableDimensions => 2;

		public Designator_BaseTrainCombat()
		{
			soundDragSustain = SoundDefOf.Designate_DragStandard;
			soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
			useMouseIcon = true;
			soundSucceeded = SoundDefOf.Designate_Hunt;
			hotKey = KeyBindingDefOf.Misc9;
		}

		public override void DesignateSingleCell(IntVec3 loc)
		{
			Thing designatable = GetDesignatable(loc);
			DesignateThing(designatable);
		}

		public Thing GetDesignatable(IntVec3 loc)
		{
			List<Thing> thingList = loc.GetThingList(base.Map);
			foreach (Thing item in thingList)
			{
				if (CanDesignateThing(item).Accepted)
				{
					return item;
				}
			}
			return null;
		}

		public override AcceptanceReport CanDesignateThing(Thing t)
		{
			if (t != null && IsCombatDummy(t) && !HugsLibUtility.HasDesignation(t, defOf))
			{
				return true;
			}
			return false;
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			if (!c.InBounds(Map) || c.Fogged(Map))
			{
				return false;
			}
			if (!CanDesignateThing(GetDesignatable(c)).Accepted)
			{
				return "MessageMustDesignateCombatDummy".Translate();
			}
			return true;
		}

		public override void DesignateThing(Thing t)
		{
			if (t != null)
			{
				HugsLibUtility.ToggleDesignation(t, defOf, true);
			}
		}

		public override void SelectedUpdate()
		{
			GenUI.RenderMouseoverBracket();
			GenDraw.DrawNoBuildEdgeLines();
		}

		public override void RenderHighlight(List<IntVec3> dragCells)
		{
			DesignatorUtility.RenderHighlightOverSelectableThings(this, dragCells);
		}

		public bool IsCombatDummy(Thing t)
		{
			return typeof(Building_CombatDummy).IsAssignableFrom(t.def.thingClass);
		}
	}
}
