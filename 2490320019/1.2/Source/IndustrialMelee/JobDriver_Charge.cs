using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace IndustrialMelee
{
    public class JobDriver_Charge : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            Toil gotoPawn = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return gotoPawn;
            Toil charge = new Toil();
            charge.initAction = delegate
            {
                var victim = this.TargetA.Pawn;
                pawn.meleeVerbs.TryMeleeAttack(victim, job.verbToUse);
                if (Rand.Chance(0.3f))
                {
                    List<BodyPartRecord> list = (from x in victim.RaceProps.body.AllParts
                                                 where !victim.health.hediffSet.PartIsMissing(x) && (x.def == BodyPartDefOf.Torso || x.def == BodyPartDefOf.Heart)
                                                 select x).ToList<BodyPartRecord>();
                    if (list.Count == 0)
                    {
                        return;
                    }
                    BodyPartRecord bodyPartRecord;
                    if (GenCollection.TryRandomElement<BodyPartRecord>(list, out bodyPartRecord))
                    {
                        var missingBodyPart = HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, victim, bodyPartRecord);
                        victim.health.AddHediff(missingBodyPart);
                    }
                }
                GenExplosion.DoExplosion(pawn.Position, pawn.Map, 7, DamageDefOf.Bomb, pawn, 20, ignoredThings: new List<Thing> { pawn });
                var comp = pawn.TryGetComp<CompAttackCooldown>();
                comp.SetCooldown(AttackType.Charge, Find.TickManager.TicksGame + 7200);
            };
            yield return charge;
        }
    }
}
