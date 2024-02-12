using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace ArtifactsExpanded
{
    public class CompTargetEffect_PsychicInterference : CompTargetEffect
    {
        public override void DoEffectOn(Pawn user, Thing target)
        {
            Pawn targetPawn = target as Pawn;
            if (targetPawn == null)
            {
                return;
            }
            //proceeds if target is alive
            if (targetPawn.Dead)
            {
                return;
            }
            //proceeds if target is part of user's faction (if any), or if pulser is configured to affect all pawns
            Faction userFaction = user.Faction;
            Faction targetFaction = targetPawn.Faction;
            if (ArtifactsExpandedMod.modSettings.antiPsychicPulserAffectsAll || (userFaction != null && userFaction == targetFaction))
            {
                Hediff existingInterference = targetPawn.health?.hediffSet?.GetFirstHediffOfDef(HediffDefOf.PsychicInterference);
                if (existingInterference == null)
                {
                    Hediff newInterference = HediffMaker.MakeHediff(HediffDefOf.PsychicInterference, targetPawn, null);
                    newInterference.Severity = 1f;
                    targetPawn.health.AddHediff(newInterference, null, null, null);
                }
                else
                {
                    existingInterference.Severity = 1f;
                }
            }
        }
    }
}
