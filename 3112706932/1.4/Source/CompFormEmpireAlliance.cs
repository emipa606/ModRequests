using RimWorld;
using Verse;

namespace ImperialFunctionality
{
    public class CompProperties_FormEmpireAlliance : CompProperties_AbilityEffect
    {
        public CompProperties_FormEmpireAlliance()
        {
            this.compClass = typeof(CompFormEmpireAlliance);
        }
    }

    public class CompFormEmpireAlliance : CompAbilityEffect
    {
        public override bool ShouldHideGizmo => Faction.OfEmpire.RelationKindWith(Faction.OfPlayer) == FactionRelationKind.Ally 
            && (ModsConfig.IdeologyActive is false || Faction.OfEmpire.ideos.PrimaryIdeo == Faction.OfPlayer.ideos.PrimaryIdeo);

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            if (ModsConfig.IdeologyActive)
            {
                Faction.OfEmpire.ideos.SetPrimary(Faction.OfPlayer.ideos.PrimaryIdeo);
            }
            Faction.OfEmpire.TryAffectGoodwillWith(Faction.OfPlayer, 100 - Faction.OfPlayer.GoodwillWith(Faction.OfEmpire), canSendMessage: true, canSendHostilityLetter: false);
        }
    }
}
