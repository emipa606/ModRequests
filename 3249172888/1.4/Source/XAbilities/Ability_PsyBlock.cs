using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using VanillaPsycastsExpanded;
using Verse;
using VFECore.Abilities;
using Ability = VFECore.Abilities.Ability;

namespace VPEArchocaster
{
    public class Ability_PsyBlock : Ability
    {

        public override void Cast(GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            var target = targets[0].Thing as Pawn;


            Hediff existing_hediff = target.health.hediffSet.GetFirstHediffOfDef(
                VPEAC_DefOf.VPEAC_PsyBlocked);

            if (existing_hediff == null)
            {
                var hediff_PsyBlocked = 
                    HediffMaker.MakeHediff(
                        VPEAC_DefOf.VPEAC_PsyBlocked, target, target.health.hediffSet.GetBrain());
                target.health.AddHediff(hediff_PsyBlocked);
            } else
            {
                target.health.RemoveHediff(existing_hediff);
            }

            
            //MoteMaker.MakeAttachedOverlay(target, VPEP_DefOf.VPEP_BrainCutSlash, Vector3.zero);
        }
    }
}
