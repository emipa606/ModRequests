using RimWorld;
using Steamworks;
using Verse;

namespace VAGUE
{
    [DefOf]
    internal class VAGUE_InternalDefs
    {
        public static GeneDef VAGUE_largeMoodDecrease;
        public static GeneDef VAGUE_coagulatoryMechanites;

        static VAGUE_InternalDefs()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(VAGUE_InternalDefs));
        }
    }
}
