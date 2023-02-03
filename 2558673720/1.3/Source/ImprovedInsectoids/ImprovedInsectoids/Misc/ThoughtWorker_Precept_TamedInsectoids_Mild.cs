using System.Linq;
using System.Collections.Generic;

using Verse;
using RimWorld;

namespace ImprovedInsectoids
{
    public class ThoughtWorker_Precept_TamedInsectoids_Mild : ThoughtWorker_Precept
    {
        protected override ThoughtState ShouldHaveThought(Pawn p)
        {
            if (!p.IsColonist)
            {
                return false;
            }

            IEnumerable<Pawn> tamedInsects = p.Map?.mapPawns?.SpawnedColonyAnimals?.Where(animal => animal.RaceProps.Insect) ?? new List<Pawn>();
            int count = tamedInsects.Count();
            if (count == 0)
            {
                return false;
            }
            if (count <= 3)
            {
                return ThoughtState.ActiveAtStage(0);
            }
            return ThoughtState.ActiveAtStage(1);
        }
    }
}
