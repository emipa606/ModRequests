using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using VanillaPsycastsExpanded;
using Verse;
using Ability = VFECore.Abilities.Ability;

namespace VPEArchocaster
{
    public class Ability_ResetPsycasts : Ability
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            var target = targets[0].Thing as Pawn;

            target.Psycasts().Reset();
            target.psychicEntropy.currentEntropy += target.psychicEntropy.MaxEntropy + 50f;
            //MoteMaker.MakeAttachedOverlay(target, VPEP_DefOf.VPEP_BrainCutSlash, Vector3.zero);
        }
    }
}
