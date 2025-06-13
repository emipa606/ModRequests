using System.Linq;
using Hospitality.Utilities;
using RimWorld;
using Verse;
using GuestUtility = Hospitality.Utilities.GuestUtility;

namespace Hospitality
{
    public class IncidentWorker_HappyGuestJoins : IncidentWorker
    {
        public override bool CanFireNowSub(IncidentParms parms)
        {
            if (!base.CanFireNowSub(parms)) return false;

            Map map = (Map) parms.target;

            return GuestUtility.GetAllGuests(map).Any(IsHappyGuest);
        }

        private static bool IsHappyGuest(Pawn pawn)
        {
            if (pawn.royalty?.AllTitlesInEffectForReading.Count > 0) return false; // no royals
            if (pawn.CompGuest().WillOnlyJoinByForce) return false;

            return  pawn.GetVisitScore(out var score) && score >= 0.9f;
        }

        public override bool TryExecuteWorker(IncidentParms parms)
        {
            var map = (Map) parms.target;

            if (!GuestUtility.GetAllGuests(map).Where(IsHappyGuest).TryMaxBy(GetScore, out var happiestGuest)) return false;

            if (happiestGuest == null) return false;

            var title = "LetterLabelHappyGuestJoins".Translate(happiestGuest.Named("PAWN")).CapitalizeFirst();
            var letter = (ChoiceLetter_GuestJoinRequest)LetterMaker.MakeLetter(title, "HappyGuestJoins".Translate(happiestGuest.Faction.Name, happiestGuest.Named("PAWN")).AdjustedFor(happiestGuest), def.letterDef);
            letter.title = title;
            letter.radioMode = true;
            letter.guest = happiestGuest;
            letter.lookTargets = happiestGuest;
            letter.relatedFaction = happiestGuest.Faction;
            Find.LetterStack.ReceiveLetter(letter);
            return true;
        }

        private static float GetScore(Pawn pawn)
        {
            return pawn.GetFriendsInColony() - pawn.GetEnemiesInColony();
        }
    }
}
