using System.Collections.Generic;

using RimWorld;
using Verse;

namespace KB_PocketDimension
{
    [DefOf]
    public static class PocketDimensionDefOf
    {
        public static WorldObjectDef KB_WorldObject_PocketDimension;

        public static ThingDef KB_PocketDimensionBox;
        public static ThingDef KB_PocketDimensionExit;
        public static ThingDef KB_PocketDimensionHopper;
        public static ThingDef KB_PocketDimensionWall;

        public static RoofDef KB_PocketDimensionRoof;

        public static JobDef KB_EnterPocket;
        public static JobDef KB_ExitPocket;
        public static JobDef KB_CarryToPocket;
        public static JobDef KB_PocketDimension_Job_SupplyBoxComponents;
        public static JobDef KB_PocketDimension_Job_PressButton;

        public static DesignationDef KB_PocketDimension_Designation_PressButton;

        public static RecordDef KB_PocketDimension_Record_ButtonsPressed;

        public static WorkGiverDef KB_PocketDimension_WorkGiver_SupplyBoxComponents;

        public static GameConditionDef KB_PocketDimensionCondition;

        public static BiomeDef KB_PocketDimensionBiome;

        public static TerrainDef KB_PocketDimensionFloorWood;
        public static TerrainDef KB_PocketDimensionFloorStone;
        public static TerrainDef KB_PocketDimensionFloorMetal;

        public static MapGeneratorDef KB_PocketDimensionMapGenerator;

        public static LetterDef KB_PocketDimensionBreachedLetter;
    }
}
