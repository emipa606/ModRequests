using RimWorld;
using System;
using System.Linq;
using Verse;

namespace HarderGearLooting
{
    internal class HediffCompProperties_DissolveHeadAndImplantsOnDeath: HediffCompProperties
    {
        public HediffCompProperties_DissolveHeadAndImplantsOnDeath()
        {
            compClass = typeof(HediffComp_DissolveHeadAndImlantsOnDeath);
        }
    }

    internal class HediffComp_DissolveHeadAndImlantsOnDeath: HediffComp
    {
        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            BodyPartRecord head = this.Pawn.health.hediffSet.GetBodyPartRecord(BodyPartDefOf.Head);
            if (head == null)
            {
                return;
            }
            Hediff_MissingPart hediffMissingPart = (Hediff_MissingPart)HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, this.Pawn);
            hediffMissingPart.IsFresh = true;
            hediffMissingPart.lastInjury = HealthUtility.GetHediffDefFromDamage(DamageDefOf.AcidBurn, this.Pawn, head);
            hediffMissingPart.Part = head;
            this.Pawn.health.hediffSet.AddDirect((Hediff)hediffMissingPart);

            while (this.Pawn.health.hediffSet.HasHediff<Hediff_Implant>())
            {
                Hediff implant = this.Pawn.health.hediffSet.GetFirstHediff<Hediff_Implant>();
                this.Pawn.health.RemoveHediff(implant);
            }

            base.Notify_PawnDied(dinfo, culprit);
        }
    }
}
