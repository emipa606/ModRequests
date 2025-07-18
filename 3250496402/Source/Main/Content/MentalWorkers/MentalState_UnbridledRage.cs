using RimWorld;
using Verse;
using Verse.AI;

namespace Kraltech_Industries;

public class MentalState_UnbridledRage : MentalState
{
    private const int NoLongerValidTargetCheckInterval = 120;

    public Pawn target;

    public override bool ForceHostileTo(Thing t)
    {
        Pawn pawn;
        return ((pawn = t as Pawn) == null || !pawn.RaceProps.Roamer) && t.Faction != null && ForceHostileTo(t.Faction);
    }

    public override bool ForceHostileTo(Faction f)
    {
        return f.def.humanlikeFaction || f == Faction.OfMechanoids || f.def.isPlayer;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref target, "target");
    }

    public override RandomSocialMode SocialModeMax()
    {
        return RandomSocialMode.Off;
    }

    public override void PreStart()
    {
        base.PreStart();
        TryFindNewTarget();
    }

    public override void MentalStateTick()
    {
        base.MentalStateTick();
        if (target != null && target.Dead) TryFindNewTarget();
        if (pawn.IsHashIntervalTick(120) && !IsTargetStillValidAndReachable())
        {
            if (!TryFindNewTarget())
            {
                TryFindNewTarget();
                return;
            }

            Messages.Message("MessageMurderousRageChangedTarget".Translate(pawn.NameShortColored, target.Label, pawn.Named("PAWN"), target.Named("TARGET")).Resolve().AdjustedFor(pawn), pawn, MessageTypeDefOf.NegativeEvent);
            base.MentalStateTick();
        }
    }

    public override TaggedString GetBeginLetterText()
    {
        TaggedString result;
        if (target == null)
        {
            Log.Error("No target. This should have been checked in this mental state's worker.");
            result = "";
        }
        else
        {
            result = def.beginLetter.Formatted(pawn.NameShortColored, target.NameShortColored, pawn.Named("PAWN"), target.Named("TARGET")).AdjustedFor(pawn).Resolve().CapitalizeFirst();
        }

        return result;
    }

    private bool TryFindNewTarget()
    {
        target = UnbridledRageMentalStateUtility.FindPawnToKill(pawn);
        return target != null;
    }

    public bool IsTargetStillValidAndReachable()
    {
        return target != null && target.SpawnedParentOrMe != null && (!(target.SpawnedParentOrMe is Pawn) || target.SpawnedParentOrMe == target) && pawn.CanReach(target.SpawnedParentOrMe, PathEndMode.Touch, Danger.Deadly, true);
    }
}