using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace EstupendoBase.Buildings
{
	public class PlaceWorker_PlinthArea : PlaceWorker
	{
		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
		{
			Map map = Find.CurrentMap;
			CellRect plinth = new CellRect(center.x, center.z, 1, 1);

			if( thing is Building_Plaque plaque )
			{
				plinth = plaque.GetRequiredDisplayRect();
			}

			Color GoodColour = new Color(0, 0.5f, 0, 0.25f);
			Color BadColour = new Color(0.5f, 0, 0, 0.25f);
			List<IntVec3> GoodCells = plinth.Cells.Where((IntVec3 cell) => cell.SupportsDisplay(map)).ToList();
			List<IntVec3> BadCells = plinth.Cells.Except(GoodCells).ToList();

			GenDraw.DrawFieldEdges(GoodCells, GoodColour);
			GenDraw.DrawFieldEdges(BadCells, BadColour);
		}
	}
}
