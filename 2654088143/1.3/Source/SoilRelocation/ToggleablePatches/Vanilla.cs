using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace SR.ToggleablePatches
{
    internal class Vanilla
    {
        [ToggleablePatch]
        internal static ToggleablePatch<BuildableDef> SandbagsUseSandPatch = new()
        {
            Name = "Sandbags Use Sand",
            Enabled = SoilRelocationSettings.SandbagsUseSandEnabled,
            TargetDefName = "Sandbags",
            Patch = (patch, def) =>
            {
                if (def.costList == null)
                    def.costList = new List<ThingDefCountClass>();
                def.costList.Add(new ThingDefCountClass { count = 5, thingDef = SoilDefs.SR_Sand }); //Add an additional cost of 5 sand.
            },
            Unpatch = (patch, def) =>
            {
                def.costList.RemoveAll(tdcc => tdcc.thingDef == SoilDefs.SR_Sand);
            },
        };

        [ToggleablePatch]
        internal static ToggleablePatch<BuildableDef> FungalGravelUsesRawFungusPatch = new()
        {
            Name = "Fungal Gravel Uses Raw Fungus",
            Enabled = SoilRelocationSettings.FungalGravelUsesRawFungusEnabled,
            TargetDefName = "FungalGravel",
            TargetModID = "Ludeon.RimWorld.Ideology",
            Patch = (patch, def) =>
            {
                if (def.costList == null)
                    def.costList = new List<ThingDefCountClass>();
                def.costList.Add(new ThingDefCountClass { count = 5, thingDef = ThingDefs.RawFungus }); //Add an additional cost of 5 raw fungus.
            },
            Unpatch = (patch, def) =>
            {
                def.CostList.RemoveAll(tdcc => tdcc.thingDef == ThingDefs.RawFungus); //Try to find our patch..
            },
        };
    }
}