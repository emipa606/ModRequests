
using RimWorld;
using Verse;
using Verse.Sound;
namespace mechanoid
{
    public class CompCauseHediff_AoE_WithFactionPikeman : ThingComp
    {
        private Sustainer activeSustainer;

        private bool lastIntervalActive;

        private CompProperties_CauseHediff_AoE_WithFactionPikeman Props => (CompProperties_CauseHediff_AoE_WithFactionPikeman)props;

        private CompPowerTrader PowerTrader => parent.TryGetComp<CompPowerTrader>();

        private bool IsPawnAffected(Pawn target)
        {
            if (PowerTrader != null && !PowerTrader.PowerOn)
            {
                return false;
            }
            if (target.Dead || target.health == null)
            {
                return false;
            }
            if (Props.affectMyFaction)
            {
                if (target.Faction != parent.Faction) { return false; }

            }
            else { if (target.Faction == parent.Faction) { return false; } }
            if (target.Position.DistanceTo(parent.Position) <= Props.range)
            {
                if (Props.onlyTargetMechs)
                {
                    return target.RaceProps.IsMechanoid;
                }
                return true;
            }
            return false;
        }

        public override void CompTick()
        {
            MaintainSustainer();
            if (parent.IsHashIntervalTick(Props.checkInterval))
            {
                CompPowerTrader compPowerTrader = parent.TryGetComp<CompPowerTrader>();
                if (compPowerTrader == null || compPowerTrader.PowerOn)
                {
                    lastIntervalActive = false;
                    foreach (Pawn item in parent.Map.mapPawns.AllPawnsSpawned)
                    {
                        if (IsPawnAffected(item))
                        {
                            Hediff hediff = item.health.hediffSet.GetFirstHediffOfDef(Props.hediff);
                            if (hediff == null)
                            {
                                hediff = item.health.AddHediff(Props.hediff, item.health.hediffSet.GetBrain());
                                hediff.Severity = 1f;
                                HediffComp_Link hediffComp_Link = hediff.TryGetComp<HediffComp_Link>();
                                if (hediffComp_Link != null)
                                {
                                    hediffComp_Link.drawConnection = false;
                                    hediffComp_Link.other = parent;
                                }
                            }
                            HediffComp_Disappears hediffComp_Disappears = hediff.TryGetComp<HediffComp_Disappears>();
                            if (hediffComp_Disappears == null)
                            {
                                Log.ErrorOnce("CompCauseHediff_AoE has a hediff in props which does not have a HediffComp_Disappears", 78945945);
                            }
                            else
                            {
                                hediffComp_Disappears.ticksToDisappear = Props.checkInterval + 1;
                            }
                            lastIntervalActive = true;
                        }
                    }
                }
            }
        }

        private void MaintainSustainer()
        {
            if (lastIntervalActive && Props.activeSound != null)
            {
                if (activeSustainer == null || activeSustainer.Ended)
                {
                    activeSustainer = Props.activeSound.TrySpawnSustainer(SoundInfo.InMap(new TargetInfo(parent)));
                }
                activeSustainer.Maintain();
            }
            else if (activeSustainer != null)
            {
                activeSustainer.End();
                activeSustainer = null;
            }
        }

        public override void PostDraw()
        {
            if (Find.Selector.SelectedObjectsListForReading.Contains(parent))
            {
                foreach (Pawn item in parent.Map.mapPawns.AllPawnsSpawned)
                {
                    if (IsPawnAffected(item))
                    {
                        GenDraw.DrawLineBetween(item.DrawPos, parent.DrawPos);
                    }
                }
            }
        }
    }
}