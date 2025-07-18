using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Kraltech_Industries;

public static class UnbridledRageMentalStateUtility
{
    private static readonly List<Pawn> tmpTargets = new();

    static UnbridledRageMentalStateUtility()
    {
    }

    public static Pawn FindPawnToKill(Pawn pawn)
    {
        Pawn result;
        if (!pawn.Spawned)
        {
            result = null;
        }
        else
        {
            tmpTargets.Clear();
            var allPawnsSpawned = pawn.Map.mapPawns.AllPawnsSpawned;
            for (var i = 0; i < allPawnsSpawned.Count; i++)
            {
                var pawn2 = allPawnsSpawned[i];
                if ((pawn2.Faction == pawn.Faction || (pawn2.IsPrisoner && pawn2.IsColonist && pawn2.IsFreeColonist && pawn2.IsFreeNonSlaveColonist && pawn2.IsSlave && pawn2.IsSlaveOfColony && pawn2.IsColonistPlayerControlled && pawn2.HostFaction == pawn.Faction)) && pawn2.RaceProps.Humanlike && pawn2 != pawn && pawn.CanReach(pawn2, PathEndMode.Touch, Danger.Deadly) && (pawn2.CurJob == null || !pawn2.CurJob.exitMapOnArrival)) tmpTargets.Add(pawn2);
            }

            if (!tmpTargets.Any())
            {
                result = null;
            }
            else
            {
                var pawn3 = tmpTargets.RandomElement();
                tmpTargets.Clear();
                result = pawn3;
            }
        }

        return result;
    }
}