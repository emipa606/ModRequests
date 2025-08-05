using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.AI;
using BillDoorsFramework;

namespace BDsPlasmaWeaponVanilla
{
    public class JobGiver_ExtinguishFireWithWeapon : ThinkNode_JobGiver
    {
        public override float GetPriority(Pawn pawn)
        {
            return 1f;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.Faction == Faction.OfPlayer && pawn.Drafted && pawn.drafter.FireAtWill && !pawn.WorkTagIsDisabled(WorkTags.Firefighting))
            {
                ThingWithComps weapon = pawn.equipment.Primary;
                if (weapon != null && weapon.def.weaponTags != null &&
                    (!weapon.def.weaponTags.NullOrEmpty() && weapon.def.weaponTags.Contains("BDP_FireExtinguisher") || (weapon.def.weaponTags.Contains("BDP_FireExtinguisherSecondary") && weapon.TryGetComp<CompSecondaryVerb>().IsSecondaryVerbSelected))
                    && pawn.CurrentEffectiveVerb.Available())
                {
                    return JobMaker.MakeJob(JobDefOf.BDP_Extinguish);
                }
            }
            return null;
        }
    }

    public class JobDriver_ExtinguishFireWithWeapon : JobDriver
    {

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil toil = new Toil();
            toil.initAction = delegate
            {
                collideWithPawns = true;
                searchFire(pawn);
            };
            toil.tickAction = delegate
            {
                if ((Find.TickManager.TicksGame + pawn.thingIDNumber) % 4 == 0)
                {
                    collideWithPawns = true;
                    searchFire(pawn);
                }
            };
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            yield return toil;
        }

        private void searchFire(Pawn pawn)
        {
            bool validator(Thing x)
            {
                return AttackTargetFinder.CanSee(pawn, x);
            }
            Thing fire = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(RimWorld.ThingDefOf.Fire), PathEndMode.OnCell, TraverseParms.For(pawn), pawn.CurrentEffectiveVerb.verbProps.range, validator);
            if (fire != null)
            {
                pawn.TryStartAttack(fire);
            }
        }
    }

}
