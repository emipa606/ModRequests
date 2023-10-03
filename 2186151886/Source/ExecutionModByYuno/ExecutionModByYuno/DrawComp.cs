using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;
using Verse.AI;

namespace ExecutionModByYuno
{
   public  class YunoExecutionDrawComp : ThingComp
   {
        Building_SpotExecution ParentClass
        {
            get
            {
                return (Building_SpotExecution)this.parent;
            }
        }


        public void decreaseRad()
        {
            this.ParentClass.SetWatchRadius =( 2 > this.ParentClass.WatchRadius - 1 ? 2 : this.ParentClass.WatchRadius - 1);
        }


        public void increaseRad()
        {
            this.ParentClass.SetWatchRadius = (26 < this.ParentClass.WatchRadius + 1 ? 26 : this.ParentClass.WatchRadius + 1);
        }

		public void CancelExecution()
		{
			ParentClass.ResetSpot();
		}


		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			foreach (Gizmo baseGizmo in base.CompGetGizmosExtra())
			{
				yield return baseGizmo;
			}
			yield return new Command_Action
			{
				action = new Action(this.increaseRad),
				defaultLabel = "Increase radius",
				defaultDesc = "Increase specator radius",
				hotKey = KeyBindingDefOf.Misc5,
				icon = ContentFinder<Texture2D>.Get("UI/Commands/bloat", true)
			};
			yield return new Command_Action
			{
				action = new Action(this.decreaseRad),
				defaultLabel = "Decrease radius",
				defaultDesc = "Decrease specator radius",
				hotKey = KeyBindingDefOf.Misc6,
				icon = ContentFinder<Texture2D>.Get("UI/Commands/shrink", true)
			};	
			yield return new Command_Action
			{
				action = new Action(this.CancelExecution),
				defaultLabel = "Cancel execution",
				defaultDesc = "Cancels execution in case something is wrong",
				hotKey = KeyBindingDefOf.Misc7,
				icon = ContentFinder<Texture2D>.Get("UI/Commands/cancelExecution", true)
			};
			yield break;
		}

		private List<IntVec3> CellsToDraw = new List<IntVec3>();


		public override void PostDrawExtraSelectionOverlays()
        {
            base.PostDrawExtraSelectionOverlays();
            Map currentMap = Find.CurrentMap;
            CellRect cellRect = CellRect.CenteredOn(this.parent.Position, 1).ExpandedBy(this.ParentClass.WatchRadius);
            CellRect cellRect2 = cellRect.ContractedBy(1);
            GenDraw.DrawFieldEdges(this.ValidCellsAround(this.parent.Position, currentMap, cellRect), Color.gray);
            GenDraw.DrawFieldEdges(this.ValidCellsAround(this.parent.Position, currentMap, cellRect2));
        }


		public List<IntVec3> ValidCellsAround(IntVec3 pos, Map map, CellRect rect)
		{
			CellsToDraw.Clear();
			bool flag = !pos.InBounds(map);
			List<IntVec3> result;
			if (flag)
			{
				result = CellsToDraw;
			}
			else
			{
				Region region = pos.GetRegion(map, RegionType.Set_Passable);
				bool flag2 = region == null;
				if (flag2)
				{
					result = CellsToDraw;
				}
				else
				{
					RegionTraverser.BreadthFirstTraverse(region, (Region from, Region r) => r.door == null, delegate (Region r)
					{
						foreach (IntVec3 item in r.Cells)
						{
							bool flag3 = this.InDistOfRect(item, pos, rect);
							if (flag3)
							{
								CellsToDraw.Add(item);
							}
						}
						return false;
					}, 13, RegionType.Set_Passable);
					result = CellsToDraw;
				}
			}
			return result;
		}


		public bool InDistOfRect(IntVec3 pos, IntVec3 otherLoc, CellRect rect)
		{
			float num = (float)pos.x;
			float num2 = (float)pos.z;
			return num <= (float)rect.maxX && num >= (float)rect.minX && num2 <= (float)rect.maxZ && num2 >= (float)rect.minZ;
		}

	}

	public class YunoExecutionDrawCompProp : CompProperties
	{
		public YunoExecutionDrawCompProp()
		{
			this.compClass = typeof(YunoExecutionDrawComp);
		}
	}
}
