using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace RottenMeat
{
    public class SpecialThingFilterWorker_Desiccated : SpecialThingFilterWorker
    {
        //overrides matching things and returns true only if thing can rot, is not destroyed when rotten, and is desiccated
        public override bool Matches(Thing t)
        {
            CompRottable compRottable = t.TryGetComp<CompRottable>();
            return compRottable != null && !compRottable.PropsRot.rotDestroys && compRottable.Stage == RotStage.Dessicated;
        }
        public override bool CanEverMatch(ThingDef def)
        {
            CompProperties_Rottable compProperties = def.GetCompProperties<CompProperties_Rottable>();
            return compProperties != null && !compProperties.rotDestroys;
        }
    }
}
