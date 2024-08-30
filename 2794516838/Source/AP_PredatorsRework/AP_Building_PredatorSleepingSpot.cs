using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace AP_PredatorsRework
{
    public class AP_Building_PredatorSleepingSpot : Building_Bed
    {
        private void RemoveAllOwners(out Pawn pawn,bool destroyed = false)
        {
            pawn = null;
            for (int num = OwnersForReading.Count - 1; num >= 0; num--)
            {
                pawn = OwnersForReading[num];
                pawn.ownership.UnclaimBed();
            }
        }
        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            Pawn pred;
            RemoveAllOwners(out pred,mode == DestroyMode.KillFinalize);
            if(!pred.Faction.IsPlayer)
            {
                pred.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter);
            }
            ForPrisoners = false;
            Medical = false;
            District district = this.GetDistrict();
            base.DeSpawn(mode);
        }
    }
}
