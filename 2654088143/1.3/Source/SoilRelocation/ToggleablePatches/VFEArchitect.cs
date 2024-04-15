using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace SR.ToggleablePatches
{
    internal class VFEArchitect
    {
        [ToggleablePatch]
        internal static ToggleablePatch<BuildableDef> PackedDirtCostsDirt = new()
        {
            Name = "VFE Architect Packed Dirt Costs Dirt",
            Enabled = SoilRelocationSettings.VFEArchitectPackedDirtCostsDirtEnabled,
            TargetDefName = "VFEArch_PlayerPackedDirt",
            TargetModID = "VanillaExpanded.VFEArchitect",
            Patch = (patch, def) =>
            {
                if (def.costList == null)
                    def.costList = new List<ThingDefCountClass>();
                def.costList.Add(new ThingDefCountClass { count = 1, thingDef = SoilDefs.SR_Soil }); //Add an additional cost of 1 soil just to remove the exploit of free dirt.
            },
            Unpatch = (patch, def) =>
            {
                def.CostList.RemoveAll(tdcc => tdcc.thingDef == SoilDefs.SR_Soil);
            },
        };
    }
}