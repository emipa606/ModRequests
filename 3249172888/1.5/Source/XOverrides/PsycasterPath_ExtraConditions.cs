
using RimWorld;
using VanillaPsycastsExpanded;
using Verse;
using VFECore.Abilities;
using VPEArchocaster;
using Ability = VFECore.Abilities.Ability;

namespace VPEArchocaster
{
    public class PsycasterPath_ExtraConditions : PsycasterPathDef
    {
        public bool hardLocked;

        public override bool CanPawnUnlock(Pawn pawn)
        {
            return (!hardLocked)&&base.CanPawnUnlock(pawn);
        }
        
    }
}