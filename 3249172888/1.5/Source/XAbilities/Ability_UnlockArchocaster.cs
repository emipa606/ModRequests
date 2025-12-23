

using RimWorld;
using RimWorld.Planet;
using VanillaPsycastsExpanded;
using Verse;
using Verse.Sound;
using Ability = VFECore.Abilities.Ability;

namespace VPEArchocaster
{
    public class Ability_UnlockArchocaster : Ability
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {

            PsycasterPathDef psycasterPathDef = VPEAC_DefOf.VPEAC_Archocaster;
            if (psycasterPathDef != null)
            {
                Hediff_PsycastAbilities hediff_PsycastAbilities = (targets[0].Thing as Pawn).Psycasts();
                if (hediff_PsycastAbilities != null && !hediff_PsycastAbilities.unlockedPaths.Contains(psycasterPathDef))
                {
                    hediff_PsycastAbilities.UnlockPath(psycasterPathDef);
                }
            }
            base.Cast(targets);


        }
    }
}
