using RimWorld;
using Verse;

namespace RBM_Minotaur
{
    // Spawns a stack of an item on the Pawn's location
    public class CompAbilityEffect_SpawnStack : CompAbilityEffect
    {
        public new CompProperties_AbilitySpawnStack Props
        {
            get
            {
                return (CompProperties_AbilitySpawnStack)this.props;
            }
        }

        //The ability effect: spawns a stack of milk on the pawn.
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (MinotaurSettings.debugMessages) { Log.Message("RBM Is Running: (AbilityLactate) public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)"); }
            Pawn pawn = this.parent.pawn;
            base.Apply(target, dest);

            Thing thing = ThingMaker.MakeThing(Props.spawnedThingDef);
            thing.stackCount = MinotaurSettings.lactateMilkAmount;

            GenSpawn.Spawn(thing, pawn.Position, pawn.Map);
        }

        //Disables gizmo based on settings or life stage.
        public override bool GizmoDisabled(out string reason)
        {
            if (MinotaurSettings.debugMessages) { Log.Message("RBM Is Running: (AbilityLactate) public override bool GizmoDisabled(out string reason)"); }

            if (this.parent.pawn.gender == Gender.Male && !MinotaurSettings.milkableMales)
            {
                reason = "Males cannot be milked due to settings.";
                return true;
            }

            if (this.parent.pawn.gender == Gender.Female && !MinotaurSettings.milkableFemales)
            {
                reason = "Females cannot be milked due to settings.";
                return true;
            }

            if (!this.parent.pawn.ageTracker.Adult)
            {
                reason = "Pawn is not yet an adult.";
                return true;
            }
            if (base.GizmoDisabled(out reason))
            {
                return true;
            }
            return false;
        }
    }

    public class CompProperties_AbilitySpawnStack : CompProperties_AbilityEffect
    {
        public CompProperties_AbilitySpawnStack()
        {
            this.compClass = typeof(CompAbilityEffect_SpawnStack);
        }

        public ThingDef spawnedThingDef;

    }

}