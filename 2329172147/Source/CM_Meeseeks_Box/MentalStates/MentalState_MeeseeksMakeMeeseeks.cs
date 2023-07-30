using RimWorld;
using Verse;
using Verse.AI;

namespace CM_Meeseeks_Box
{
    public class MentalState_MeeseeksMakeMeeseeks : MentalState
    {
        public virtual bool doOnce => true;

        public override RandomSocialMode SocialModeMax()
        {
            return RandomSocialMode.Off;
        }
    }
}