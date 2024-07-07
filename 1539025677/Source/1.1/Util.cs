using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace CutPlantsBeforeBuilding {
    class Util {
        public static void DesignatePlants(IntVec3 c, Rot4 rot, BuildableDef buildableDef, ThingDef stuffDef, Map map) {
            if (DebugSettings.godMode || buildableDef.GetStatValueAbstract(StatDefOf.WorkToBuild, stuffDef) == 0f) {
                return;
            }

            ThingDef thingDef = buildableDef as ThingDef;
            IntVec2 size = IntVec2.One;
            if (thingDef != null) {
                size = thingDef.size;
            }
            DesignatePlants(c, rot, size, map);
        }

        public static void DesignatePlants(IntVec3 c,Rot4 rot,IntVec2 size, Map map) {
            foreach (IntVec3 p in GenAdj.OccupiedRect(c, rot, size)) {
                DesignatePlant(p,map);
            }
        }

        public static void DesignatePlant(IntVec3 c, Map map) {
            if (Main.autoDesignatePlantsCutMode) {
                Plant plant = FindBlockingPlant(c, map);
                if (plant != null) {
                    DesignatePlant(plant, map);
                }
            }
        }

        private static void DesignatePlant(Plant plant, Map map) {
            map.designationManager.RemoveAllDesignationsOn(plant, false);
            if (plant.HarvestableNow) {
                map.designationManager.AddDesignation(new Designation(plant, DesignationDefOf.HarvestPlant));
            } else {
                map.designationManager.AddDesignation(new Designation(plant, DesignationDefOf.CutPlant));
            }
        }

        //GenConstruct.FirstBlockingThing
        private static Plant FindBlockingPlant(IntVec3 c, Map map) {
            Plant plant = c.GetPlant(map);
            if (plant != null && plant.def.plant.harvestWork >= 200f) {
                return plant;
            }
            return null;
        }
    }
}
