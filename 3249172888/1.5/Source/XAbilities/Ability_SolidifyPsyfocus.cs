using RimWorld;
using RimWorld.Planet;
using System;
using UnityEngine;
using UnityEngine.Networking;
using VanillaPsycastsExpanded;
using Verse;
using VFECore.Abilities;
using Ability = VFECore.Abilities.Ability;

namespace VPEArchocaster
{
    public class Ability_SolidifyPsyfocus : Ability
    {
       

        public override void Cast(GlobalTargetInfo[] targets)
        {
            base.Cast(targets);

            
            int count = 5 + (int)(5*Math.Pow(pawn.psychicEntropy.PsychicSensitivity, 1.5) /
                (0.1f + pawn.GetStatValue(VPE_DefOf.VPE_PsyfocusCostFactor)));


            while (count > 0)
            {

                Thing spawn_item = ThingMaker.MakeThing(VPEAC_DefOf.VPEAC_GlobalSettings.VPEAC_solidPsyfocus);
                IntVec3 item_position = pawn.Position;
                spawn_item.stackCount = Math.Min(VPEAC_DefOf.VPEAC_GlobalSettings.VPEAC_solidPsyfocus.stackLimit,
                    count);
                GenSpawn.Spawn(spawn_item, item_position, pawn.Map);

                count -= VPEAC_DefOf.VPEAC_GlobalSettings.VPEAC_solidPsyfocus.stackLimit;
            }

            //pawn.psychicEntropy.OffsetPsyfocusDirectly( -0.1f );

            //MoteMaker.MakeAttachedOverlay(target, VPEP_DefOf.VPEP_BrainCutSlash, Vector3.zero);
        }
    }
}
