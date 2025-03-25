using RimWorld;
using Verse;

namespace SimpleSlaveryCollars
{
    public class RoleRequirement_SlaveStage : RoleRequirement
    {
        public override string GetLabel(Precept_Role role) => (string)this.labelKey.Translate((NamedArgument)Find.ActiveLanguageWorker.WithIndefiniteArticle(role.ideo.memberName, Gender.None));

        public override bool Met(Pawn pawn, Precept_Role role)
        {
            if (pawn.IsSlaveOfColony && (SlaveUtility.TimeAsSlave(pawn) < SlaveUtility.SlaveStage4 || SlaveUtility.IsSteadfast(pawn)))
                return false;
            return true;
        }
    }
}
