using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace SR
{
    public class SoilRelocationSettings : ModSettings
    {
        public static bool SandbagsUseSandEnabled = true;
        public static bool FungalGravelUsesRawFungusEnabled = true;
        public static bool DubsSkylightsGlassUsesSandEnabled = true;
        public static bool JustGlassGlassUsesSandEnabled = true;
        public static bool GlassPlusLightsGlassUsesSandEnabled = true;
        public static bool VFEArchitectPackedDirtCostsDirtEnabled = true;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref SandbagsUseSandEnabled, "SandbagsUseSandEnabled");
            Scribe_Values.Look(ref FungalGravelUsesRawFungusEnabled, "FungalGravelUsesRawFungusEnabled");
            Scribe_Values.Look(ref DubsSkylightsGlassUsesSandEnabled, "DubsSkylightsGlassUsesSandEnabled");
            Scribe_Values.Look(ref JustGlassGlassUsesSandEnabled, "JustGlassGlassUsesSandEnabled");
            Scribe_Values.Look(ref GlassPlusLightsGlassUsesSandEnabled, "GlassPlusLightsGlassUsesSandEnabled");
            Scribe_Values.Look(ref VFEArchitectPackedDirtCostsDirtEnabled, "VFEArchitectPackedDirtCostsDirtEnabled");

            base.ExposeData();
        }
    }
}