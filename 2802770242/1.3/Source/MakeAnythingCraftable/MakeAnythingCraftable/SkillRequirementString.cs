using Verse;

namespace MakeAnythingCraftable
{
    public class SkillRequirementString : IExposable
    {
        public string skill;

        public int minLevel;

        public void ExposeData()
        {
            Scribe_Values.Look(ref skill, "skill");
            Scribe_Values.Look(ref minLevel, "minLevel");
        }
    }
}
