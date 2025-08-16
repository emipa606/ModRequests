using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace Entomophagy
{
    class MentalState_BugForaging : MentalState
    {
        public override RandomSocialMode SocialModeMax()
        {
            return RandomSocialMode.Quiet;
        }

        public override void MentalStateTick()
        {
            if (this.pawn.IsHashIntervalTick(150))
            {
                pawn.needs.food.CurLevel += .01f;
                base.MentalStateTick();
                if ((pawn.Map != null && !JoyUtility.EnjoyableOutsideNow(pawn.Map)) || HealthAIUtility.ShouldBeTendedNowByPlayer(pawn))
                {
                    //Log.Message("Not a good time for foraging");
                    RecoverFromState();
                    return;
                }
            }
        }

        public override void PostEnd()
        {
            pawn.needs.mood.thoughts.memories.TryGainMemory(def.moodRecoveryThought, null);
            List<Pawn> otherPawns = new List<Pawn>();
            Pawn otherPawn;
            if (pawn.Map != null)
            {
                otherPawns = pawn.Map.mapPawns.AllPawnsSpawned;
            }
            else if (pawn.IsCaravanMember())
            {
                otherPawns = pawn.GetCaravan().PawnsListForReading;
            }
            for (int i = 0; i < otherPawns.Count; i++)
            {
                otherPawn = otherPawns[i];
                if (!otherPawn.NonHumanlikeOrWildMan() && otherPawn.needs != null)
                {
                    otherPawn.needs.mood.thoughts.memories.TryGainMemory(Entomophagy_DefOf.ObservedForaging, pawn);
                }
            }
        }
    }
}
