using System.Collections.Generic;
using System.Linq;
using HugsLib;
using Verse;

namespace ReTill
{
    public class ReTill : ModBase
    {
        public static bool VanillaPlantsExpandedInstalled { get; private set; }

        public override void DefsLoaded()
        {
            VanillaPlantsExpandedInstalled = ReTillDefOf.VCE_TilledSoil != null;

            foreach (var def in TilledSoilDefs)
            {
                if ((def.costList != null && def.costList.Count != 0) || def.costStuffCount != 0)
                {
                    Logger.Error("tilled soil def {0} unexpectedly requires materials", def.defName);
                }
            }

            if (!TilledSoilDefs.Any())
            {
                Logger.Warning("no supported tilled soil defs found");
            }
        }

        public static IEnumerable<TerrainDef> TilledSoilDefs
        {
            get
            {
                if (ReTillDefOf.VCE_TilledSoil != null)
                {
                    yield return ReTillDefOf.VCE_TilledSoil;
                }
                if (ReTillDefOf.GT_SoilTilled != null)
                {
                    yield return ReTillDefOf.GT_SoilTilled;
                }
                if (ReTillDefOf.FungalGravel != null)
                {
                    yield return ReTillDefOf.FungalGravel;
                }
                if (ReTillDefOf.Farmland != null)
                {
                    yield return ReTillDefOf.Farmland;
                }
                if (ReTillDefOf.FarmlandRich != null)
                {
                    yield return ReTillDefOf.FarmlandRich;
                }
            }
        }

        public static bool IsTilledSoilDef(Def def)
        {
            return def != null && TilledSoilDefs.Contains(def);
        }
    }
}
