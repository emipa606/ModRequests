using System.Collections.Generic;
using Verse;

namespace VexedThings
{
    public class SyntheticPawnsExtension : DefModExtension
    {
        public bool corpseWillNotRot = true;
        public bool corpseIsEdible = false;
        public bool canBeStunnedByEMP = true;
        public bool canContractIllness = false;
        public bool woundsCanBecomeInfected = false;
        public bool disableBreathVapors = true;
        public List<JobDef> disableListedActions = new List<JobDef>();
        public List<HediffDef> disableListedHediffs = new List<HediffDef>();
    }

    public static class SyntheticPawnsExtensionEZPZ
    {
        public static SyntheticPawnsExtension GetSynthExtension(this Pawn pawn)
        {
            return pawn.def.GetModExtension<SyntheticPawnsExtension>();
        }
    }
}
