using Verse;

namespace FriendlyFireTweaks
{
    public static class Extensions
    {
        internal static bool IsPawn(this Thing thing)
        {
            return thing is Pawn pawn && !pawn.IsAnimal();
        }

        internal static bool IsAnimal(this Thing thing)
        {
            return thing is Pawn pawn && pawn.RaceProps.Animal;
        }

        internal static bool IsBuilding(this Thing thing)
        {
            return thing.def.category == ThingCategory.Building && !thing.IsMineable();
        }

        internal static bool IsMineable(this Thing thing)
        {
            return thing.def.mineable;
        }

        internal static bool ShouldBeProtectedFrom(this Thing target, Thing shooter)
        {
            return Utility.ProtectedThingType(target) && Utility.ProtectedFaction(shooter, target);
        }

    }
}