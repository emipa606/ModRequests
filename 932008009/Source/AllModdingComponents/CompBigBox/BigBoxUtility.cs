﻿using Verse;

namespace DefModExtension_BigBox
{
    public static class BigBoxUtility
    {
        // Avoiding Def.GetModExtension<T> and implementing a specific non-generic version of it here.
        // That method is slow because the `isinst` instruction with generic type arg operands is very slow,
        // while `isinst` instruction against non-generic type operand like used below is fast.
        public static DefModExtension_BigBox GetModExtensionBigBox(this Def def)
        {
            var modExtensions = def.modExtensions;
            if (modExtensions == null)
                return null;
            for (int i = 0, count = modExtensions.Count; i < count; i++)
            {
                if (modExtensions[i] is DefModExtension_BigBox modExtension)
                    return modExtension;
            }
            return null;
        }
    }
}
