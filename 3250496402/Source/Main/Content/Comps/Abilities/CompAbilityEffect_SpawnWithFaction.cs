using RimWorld;
using Verse;

namespace Kraltech_Industries;

public class CompAbilityEffect_SpawnWithFaction : CompAbilityEffect
{
    public new CompProperties_AbilitySpawnWithFaction Props => (CompProperties_AbilitySpawnWithFaction)props;

    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
    {
        base.Apply(target, dest);
        GenSpawn.Spawn(Props.thingDef, target.Cell, parent.pawn.Map).SetFaction(Faction.OfPlayer);
    }

    public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
    {
        bool result;
        if (target.Cell.Filled(parent.pawn.Map) || (target.Cell.GetFirstBuilding(parent.pawn.Map) != null && !Props.allowOnBuildings))
        {
            if (throwMessages) Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "AbilityOccupiedCells".Translate(), target.ToTargetInfo(parent.pawn.Map), MessageTypeDefOf.RejectInput, false);
            result = false;
        }
        else
        {
            result = true;
        }

        return result;
    }
}