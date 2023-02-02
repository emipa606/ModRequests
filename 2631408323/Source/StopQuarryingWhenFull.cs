using HugsLib;
using HugsLib.Settings;
using Quarry;
using Verse;

namespace StopQuarryingWhenFull
{
    public class StopQuarryingWhenFull : ModBase
    {
        public override string ModIdentifier => "me.lubar.StopQuarryingWhenFull";

        public static SettingHandle<int> FullPercentage { get; private set; }

        public override void DefsLoaded()
        {
            FullPercentage = Settings.GetHandle<int>(
                "fullPercentage",
                "StopQuarryingWhenFull_Percentage_title".Translate(),
                "StopQuarryingWhenFull_Percentage_desc".Translate(),
                75,
                v => int.TryParse(v, out int percent) && percent > 0 && percent <= 100
            );
        }

        public static int QuarryFullPercentage(Building_Quarry quarry)
        {
            int total = 0, filled = 0;

            foreach (var coord in quarry.OccupiedRect().ContractedBy(quarry.WallThickness).Cells)
            {
                total++;

                if (quarry.Map.thingGrid.CellContains(coord, ThingCategory.Item))
                {
                    filled++;
                }
            }

            return 100 * filled / total;
        }

        public static bool QuarryIsFull(Building_Quarry quarry)
        {
            return QuarryFullPercentage(quarry) >= FullPercentage;
        }
    }
}
