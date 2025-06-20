using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Therapy
{
    public class JoyGiver_ReceiveTherapy : JoyGiver
    {
        public override float GetChance(Pawn pawn)
        {
            if (pawn.TimeTableIs(TimeAssignmentDefOf.Sleep)) return 0;
            if (pawn.TimeTableIs(TimeAssignmentDefOf.Meditate)) return 0;
            //Log.Message($"Therapy: {pawn.NameShortColored} has chance: {base.GetChance(pawn) + pawn.GetTherapyNeed()} IsPrisoner: {pawn.IsPrisonerOfColony}");
            if (pawn.needs.joy == null || pawn.IsPrisonerOfColony) return base.GetChance(pawn) + pawn.GetTherapyNeed();

            if (pawn.needs.joy.CurLevelPercentage < 0.1f) return base.GetChance(pawn);
            return Mathf.Max(0, base.GetChance(pawn) + pawn.GetTherapyNeed() - (1 - pawn.needs.joy.CurLevelPercentage) * 6);
        }

        public override Job TryGiveJob(Pawn pawn)
        {
            if (!InteractionUtility.CanInitiateInteraction(pawn))
            {
                //Log.Warning(pawn.NameStringShort + ": can't initiate interaction");
                return null;
            }
            var couch = MainUtility.FindRandomCouchWithChair(pawn, true, Danger.None);
            if (couch == null)
            {
                //Log.Warning(pawn.NameStringShort + ": no couch with chair");
                return null;
            }

            if (couch.CouchNoLongerUsable(pawn)) return null;
            return new Job(def.jobDef, couch);
        }
    }
}
