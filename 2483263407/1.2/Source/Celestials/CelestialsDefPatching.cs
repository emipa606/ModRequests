using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using RimWorld;
using Verse;

namespace Celestials
{
    [StaticConstructorOnStartup]
    public static class CelestialsDefPatching
    {
        static CelestialsDefPatching()
        {
            PatchDefs();
        }

        public static void PatchDefs()
        {
            if (Prefs.DevMode)
            {
                Log.Message($"Patching Celestials to appropriate races...");
            }

            foreach(ThingDef thingDef in DefDatabase<ThingDef>.AllDefs)
            {
                if(thingDef?.race?.Humanlike ?? false)
                {
                    if (thingDef.comps == null)
                    {
                        thingDef.comps = new List<CompProperties>();
                    }
                    thingDef.comps.Add(new CompProperties_Celestials());

                    if (Prefs.DevMode)
                    {
                        Log.Message($"=> {thingDef.defName} Patched");
                    }
                }
            }
        }
    }
}
