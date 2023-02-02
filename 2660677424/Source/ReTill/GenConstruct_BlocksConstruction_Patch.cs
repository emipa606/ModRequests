using HarmonyLib;
using RimWorld;
using Verse;

namespace ReTill
{
    [HarmonyPatch(typeof(GenConstruct), nameof(GenConstruct.BlocksConstruction))]
    static class GenConstruct_BlocksConstruction_Patch
    {
        public static bool Postfix(bool __result, Thing constructible, Thing t)
        {
            if (__result && t.def.category == ThingCategory.Plant)
            {
                var blueprintDef = constructible.def;
                if (constructible is Frame)
                {
                    blueprintDef = blueprintDef.entityDefToBuild.blueprintDef;
                }
                else if (!(constructible is Blueprint))
                {
                    blueprintDef = blueprintDef.blueprintDef;
                }

                // allow constructing terrain that satisfies the fertility requirement of a plant under the plant
                return !(blueprintDef.entityDefToBuild is TerrainDef terrain && terrain.fertility >= t.def.plant.fertilityMin);
            }

            return __result;
        }
    }
}
