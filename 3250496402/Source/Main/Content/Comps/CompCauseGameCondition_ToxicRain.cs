using RimWorld;
using Verse;

namespace Kraltech_Industries;

public class CompCauseGameCondition_ToxicRain : CompCauseGameCondition
{
    public override void CompTick()
    {
        base.CompTick();
        if (Active && Find.TickManager.TicksGame % 3451 == 0)
        {
            var caravans = Find.WorldObjects.Caravans;
            for (var i = 0; i < caravans.Count; i++)
                if (Find.WorldGrid.ApproxDistanceInTiles(caravans[i].Tile, MyTile) < Props.worldRange)
                {
                    var pawnsListForReading = caravans[i].PawnsListForReading;
                    for (var j = 0; j < pawnsListForReading.Count; j++) GameCondition_ToxicRain.DoPawnToxicDamage(pawnsListForReading[j]);
                }
        }
    }
}