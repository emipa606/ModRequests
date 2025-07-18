using RimWorld;
using Verse;

namespace Kraltech_Industries;

public class CompTargetEffect_PsyRejuvenate : CompTargetEffect
{
    public override void DoEffectOn(Pawn user, Thing target)
    {
        var pawn = (Pawn)target;
        if (pawn.Faction == Faction.OfPlayer && !pawn.AnimalOrWildMan())
        {
            if (!pawn.Dead)
            {
                var psychicEntropy = pawn.psychicEntropy;
                if (psychicEntropy != null)
                {
                    psychicEntropy.OffsetPsyfocusDirectly(1f);
                    if (pawn.psychicEntropy != null) pawn.psychicEntropy.TryAddEntropy(-1000f);
                }
            }

            var hediff = HediffMaker.MakeHediff(HediffDefOf.PsyRejuvenate, pawn);
            BodyPartRecord part = null;
            pawn.RaceProps.body.GetPartsWithTag(BodyPartTagDefOf.ConsciousnessSource).TryRandomElement(out part);
            var battleLogEntry_ItemUsed = new BattleLogEntry_ItemUsed(user, target, parent.def, RulePackDefOf.Event_ItemUsed);
            hediff.combatLogEntry = new WeakReference<LogEntry>(battleLogEntry_ItemUsed);
            hediff.combatLogText = battleLogEntry_ItemUsed.ToGameStringFromPOV(null);
            pawn.health.AddHediff(hediff, part);
            Find.BattleLog.Add(battleLogEntry_ItemUsed);
        }
    }
}