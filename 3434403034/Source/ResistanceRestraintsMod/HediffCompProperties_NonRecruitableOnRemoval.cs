using Verse;
using RimWorld;
using System;

namespace ResistanceRestraintsMod
{
    public class HediffCompProperties_NonRecruitableOnRemoval : HediffCompProperties
    {
        public HediffCompProperties_NonRecruitableOnRemoval()
        {
            this.compClass = typeof(HediffComp_NonRecruitableOnRemoval);
        }
    }

    public class HediffComp_NonRecruitableOnRemoval : HediffComp
    {
        public HediffCompProperties_NonRecruitableOnRemoval Props => (HediffCompProperties_NonRecruitableOnRemoval)this.props;

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();

            Pawn pawn = this.Pawn;
            if (pawn == null || pawn.guest == null) return;


            // Make the prisoner unwaveringly loyal again
            pawn.guest.Recruitable = false;

            // Reset their resistance to an extreme value (optional, but reinforces non-recruitable state)
            pawn.guest.resistance = 9999f;

            // Ensure they remain a prisoner
            if (!pawn.guest.IsPrisoner)
            {
                pawn.guest.SetGuestStatus(Faction.OfPlayer, GuestStatus.Prisoner);
            }
        }
    }
}
