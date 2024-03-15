using RimWorld;
using Verse;

namespace KB_PocketDimension
{
    class Dialog_RenamePocketDimensionEntranceBase : Dialog_Rename
    {
        private string buildingSeed;

        public Dialog_RenamePocketDimensionEntranceBase(Building_PocketDimensionEntranceBase building)
        {
            this.buildingSeed = building.dimensionSeed;
            curName = building.uniqueName ?? building.LabelNoCount;
        }
        protected override void SetName(string name)
        {
            Building_PocketDimensionBox box = PocketDimensionUtility.GetBox(buildingSeed);
            Building_PocketDimensionExit exit = PocketDimensionUtility.GetExit(buildingSeed);

            if (box != null)
                box.uniqueName = curName;
            if (exit != null)
                exit.uniqueName = "KB_PocketDimension_ExitName".Translate(curName);

            Messages.Message("KB_RenamePocketDimensionMessage".Translate(curName), MessageTypeDefOf.TaskCompletion);
        }
    }
}