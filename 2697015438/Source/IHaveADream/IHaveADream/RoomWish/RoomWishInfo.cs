using RimWorld;
using HarmonyLib;
using Verse;

namespace HDream
{
    public class RoomWishInfo
    {
        public RoomRoleDef role;

        public RoomStatDef relatedStats;

        public float toNearestScoreStage = float.MinValue;

        public bool shoulBeOwner = false;

        private RoomStatScoreStage scoreStage = null;
        public RoomStatScoreStage ScoreStage
        {
            get
            {
                if(scoreStage == null)
                {
                    if (relatedStats == null) relatedStats = RoomStatDefOf.Impressiveness;
                    for (int i = 0; i < relatedStats.scoreStages.Count; i++)
                    {
                        if (relatedStats.scoreStages[i].minScore >= toNearestScoreStage)
                        {
                            if (relatedStats.scoreStages[i].minScore == toNearestScoreStage
                                || i == 0
                                || relatedStats.scoreStages[i].minScore - toNearestScoreStage < toNearestScoreStage - relatedStats.scoreStages[i - 1].minScore) 
                            {
                                scoreStage = relatedStats.scoreStages[i];
                            }
                            else scoreStage = relatedStats.scoreStages[i - 1];

                            return scoreStage;
                        }
                    }
                    scoreStage = relatedStats.scoreStages[relatedStats.scoreStages.Count - 1];
                }
                return scoreStage;
            }
        }

    }
}
