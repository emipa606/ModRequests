using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Clutter_StructureWall
{
	[DefOf]
	public static class ClutterStructureDefOf
	{
		static ClutterStructureDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(ClutterStructureDefOf));
		}

		public static DesignationCategoryDef Structure;
	}
}
