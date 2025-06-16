﻿using Verse;

namespace CompExtraSounds
{
    public static class ExtraSoundsUtility
    {
        // Avoiding ThingWithComps.GetComp<T> and implementing a specific non-generic version of it here.
        // That method is slow because the `isinst` instruction with generic type arg operands is very slow,
        // while `isinst` instruction against non-generic type operand like used below is fast.
        internal static CompExtraSounds GetCompExtraSounds(this ThingWithComps thing)
        {
            var comps = thing.AllComps;
            for (int i = 0, count = comps.Count; i < count; i++)
            {
                if (comps[i] is CompExtraSounds comp)
                    return comp;
            }
            return null;
        }

        // Avoiding Def.GetModExtension<T> and implementing a specific non-generic version of it here.
        // That method is slow because the `isinst` instruction with generic type arg operands is very slow,
        // while `isinst` instruction against non-generic type operand like used below is fast.
        public static DefModExtension_ExtraSounds GetModExtensionExtraSounds(this Def def)
        {
            var modExtensions = def.modExtensions;
            if (modExtensions == null)
                return null;
            for (int i = 0, count = modExtensions.Count; i < count; i++)
            {
                if (modExtensions[i] is DefModExtension_ExtraSounds modExtension)
                    return modExtension;
            }
            return null;
        }
    }
}
