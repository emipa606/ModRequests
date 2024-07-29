using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace NoDebris {
	[StaticConstructorOnStartup]
	class NoDebris {
		static NoDebris() {
			foreach (TerrainDef def in DefDatabase<TerrainDef>.AllDefs)
				if (def.defName.EndsWith("_Smooth"))
					def.scatterType = null;
		}
	}
}
