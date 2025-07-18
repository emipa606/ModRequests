using RimWorld;
using Verse;
using Verse.AI;

namespace Kraltech_Industries;

public class CompTargetEffect_EmpressDecree : CompTargetEffect
{
    public override void DoEffectOn(Pawn user, Thing target)
    {
        var pawn = (Pawn)target;
        if (!pawn.Dead)
        {
            var hediff = HediffMaker.MakeHediff(HediffDefOf.EmpressDecree, pawn);
            BodyPartRecord part = null;
            pawn.RaceProps.body.GetPartsWithTag(BodyPartTagDefOf.ConsciousnessSource).TryRandomElement(out part);
            var battleLogEntry_ItemUsed = new BattleLogEntry_ItemUsed(user, target, parent.def, RulePackDefOf.Event_ItemUsed);
            hediff.combatLogEntry = new WeakReference<LogEntry>(battleLogEntry_ItemUsed);
            hediff.combatLogText = battleLogEntry_ItemUsed.ToGameStringFromPOV(null);
            pawn.health.AddHediff(hediff, part);
            Find.BattleLog.Add(battleLogEntry_ItemUsed);
            if (pawn.InMentalState)
            {
                pawn.MentalState.RecoverFromState();
                pawn.jobs.EndCurrentJob(JobCondition.Ongoing | JobCondition.Incompletable);
            }
        }
    }
}