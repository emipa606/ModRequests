using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using UnityEngine;
using VanillaPsycastsExpanded;
using Verse;
using Verse.Noise;
using VFECore.Abilities;
using static HarmonyLib.Code;
using Ability = VFECore.Abilities.Ability;

namespace VPEArchocaster
{
    public class Ability_TransferPsycasts : Ability
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            var target = targets[0].Thing as Pawn;


            //copying psy
            var ownPsylink = pawn.GetMainPsylinkSource();
            var ownPsycasts = pawn.Psycasts();

            var otherPsylink = target.GetMainPsylinkSource();
            var otherPsycasts = target.Psycasts();

            var sourceEtropy = pawn.psychicEntropy.currentEntropy;
            var sourceFocus = pawn.psychicEntropy.currentPsyfocus;


            //copying abilitties
            List<Ability> ownAbilities = [];
            foreach (var i in pawn.GetComp<CompAbilities>()?.LearnedAbilities)
            {
                if (i.def.Psycast() == null)
                    continue;
                ownAbilities.Add(i);
            }
            pawn.GetComp<CompAbilities>()?.LearnedAbilities.RemoveAll((VFECore.Abilities.Ability a) => a.def.Psycast() != null);

            List<Ability> otherAbilities = [];
            foreach (var i in target.GetComp<CompAbilities>()?.LearnedAbilities)
            {
                if (i.def.Psycast() == null)
                    continue;
                otherAbilities.Add(i);
            }
            target.GetComp<CompAbilities>()?.LearnedAbilities.RemoveAll((VFECore.Abilities.Ability a) => a.def.Psycast() != null);




            //removing psy
            pawn.health.RemoveHediff(ownPsylink);
            pawn.health.RemoveHediff(ownPsycasts);

            target.health.RemoveHediff(otherPsylink);
            target.health.RemoveHediff(otherPsycasts);


            // adding psy
            pawn.health.hediffSet.hediffs.Add(otherPsylink);
            pawn.health.hediffSet.hediffs.Add(otherPsycasts);

            target.health.hediffSet.hediffs.Add(ownPsylink);
            target.health.hediffSet.hediffs.Add(ownPsycasts);


            //adding config to caster
            pawn.psychicEntropy.currentEntropy = target.psychicEntropy.currentEntropy;
            pawn.psychicEntropy.currentPsyfocus = target.psychicEntropy.currentPsyfocus;

            //adding config to target
            target.psychicEntropy.currentEntropy = sourceEtropy;
            target.psychicEntropy.currentPsyfocus = sourceFocus;



            //adding abilities
            foreach (var i in ownAbilities)
            {
                target.GetComp<CompAbilities>()?.GiveAbility(i.def);
            }
            foreach (var i in otherAbilities)
            {
                pawn.GetComp<CompAbilities>()?.GiveAbility(i.def);
            }



            float target_entropy = target.psychicEntropy.MaxEntropy;
            target.psychicEntropy.currentEntropy += Math.Min(target_entropy*2, target_entropy+150);
            float caster_entropy = pawn.psychicEntropy.MaxEntropy;
            pawn.psychicEntropy.currentEntropy += Math.Min(caster_entropy * 2, caster_entropy + 150);
            
        }
    }
}
