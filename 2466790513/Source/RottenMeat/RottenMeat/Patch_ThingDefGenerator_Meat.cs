using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using HarmonyLib;

namespace RottenMeat
{
    [StaticConstructorOnStartup]
    public class Patch_ThingDefGenerator_Meat
    {
        //the static constructor
        static Patch_ThingDefGenerator_Meat()
        {
            Patch_ImpliedMeatDefs();
        }
        
        //patches all meats' CompProperties_Rottable
        public static void Patch_ImpliedMeatDefs()
        {
            foreach (ThingDef currentThingDef in DefDatabase<ThingDef>.AllDefs.ToList<ThingDef>())
            {
                if (currentThingDef.category == ThingCategory.Pawn && currentThingDef.race.meatDef != null)
                {
                    ThingDef currentMeatDef = currentThingDef.race.meatDef;
                    var currentCompRottable = currentMeatDef.GetCompProperties<CompProperties_Rottable>();
                    if (currentCompRottable != null)
                    {
                        currentCompRottable.rotDestroys = false;
                    }
                }
            }
        }
    }
}