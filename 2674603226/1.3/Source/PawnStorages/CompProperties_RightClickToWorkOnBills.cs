using System.Collections.Generic;
using Verse;

namespace PawnStorages
{
    public class CompProperties_RightClickToWorkOnBills : CompProperties
    {
        public List<RecipeDef> recipeToCallRightClick;
        public CompProperties_RightClickToWorkOnBills()
        {
            this.compClass = typeof(CompRightClickToWorkOnBills);
        }
    }
}
