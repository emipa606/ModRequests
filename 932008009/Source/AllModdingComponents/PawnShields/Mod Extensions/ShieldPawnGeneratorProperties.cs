﻿using System.Collections.Generic;
using Verse;

namespace PawnShields
{
    /// <summary>
    /// Contains extra information for the patched pawn generator to give a appropiate shield to the pawn.
    /// </summary>
    public class ShieldPawnGeneratorProperties : DefModExtension
    {
        /// <summary>
        /// How much "money" the pawn got pay for their shield.
        /// </summary>
        public FloatRange shieldMoney = FloatRange.Zero;

        /// <summary>
        /// The shields with any of these tags can be used.
        /// </summary>
        public List<string> shieldTags;

        public override IEnumerable<string> ConfigErrors()
        {
            if (shieldTags.NullOrEmpty())
                yield return nameof(shieldTags) + " cannot be null or empty";
        }
    }
}
