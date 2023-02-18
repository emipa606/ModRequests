using System;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystosentient.Biome
{
	// Token: 0x02000042 RID: 66
	public class PlaceWorker_Gem : PlaceWorker
	{
		// Token: 0x060000A7 RID: 167 RVA: 0x0000652C File Offset: 0x0000472C
		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
		{
			if (map.Biome.defName != "GDCRYST_BIOME_GemGardenAmethsyt")
			{
				return new AcceptanceReport("GDCRYST_GemAcceptance".Translate());
			}
			return true;
		}
	}
}
