using RimWorld;
using Verse;
using Verse.AI;

namespace ArchotechWeaponry.MentalState
{
    public class MentalState_BattleTrance : Verse.AI.MentalState
    {
        public override bool ForceHostileTo(Thing t) => true;
        
        public override bool ForceHostileTo(Faction f) => true;
        
        public override RandomSocialMode SocialModeMax()
        {
            return RandomSocialMode.Off;
        }
    }
}