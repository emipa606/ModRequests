using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace JAHV
{
    public class Comp_CombatBoost : ThingComp
    {
        public int cooldowntick;

        public int durationcount;

        public CompProperties_CombatBoost Props => (CompProperties_CombatBoost)props;

        public override void CompTick()
        {
            if (!parent.Spawned || !((Conquistador)parent).activeBoost)
            {
                return;
            }
            if (durationcount >= Props.duration * 60)
            {
                durationcount = 0;
                cooldowntick = Find.TickManager.TicksGame + Props.cooldown * 60;
                ((Conquistador)parent).activeBoost = false;
                return;
            }
            durationcount++;
            foreach (Pawn item in (parent.Faction == null) ? parent.Map.mapPawns.AllPawnsSpawned : parent.Map.mapPawns.SpawnedPawnsInFaction(parent.Faction))
            {
                if (item.RaceProps.Humanlike && !item.Dead && item.health != null && item != parent && item.Position.DistanceTo(parent.Position) <= Props.range && Props.targetingParameters.CanTarget(item))
                {
                    Hediff hediff = item.health.hediffSet.GetFirstHediffOfDef(Props.hediff);
                    if (hediff == null)
                    {
                        hediff = item.health.AddHediff(Props.hediff, item.health.hediffSet.GetBrain());
                        hediff.Severity = 1f;
                    }
                    HediffComp_Disappears hediffComp_Disappears = hediff.TryGetComp<HediffComp_Disappears>();
                    if (hediffComp_Disappears == null)
                    {
                        Log.Error("Comp_CombatBoost has a hediff in props which does not have a HediffComp_Disappears");
                    }
                    else
                    {
                        hediffComp_Disappears.ticksToDisappear = 5;
                    }
                }
            }
        }

        public override void PostDraw()
        {
            base.PostDraw();
            if (parent.Spawned && ((Conquistador)parent).activeBoost)
            {
                GenDraw.DrawCircleOutline(parent.Position.ToVector3(), Props.range, SimpleColor.Red);
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref cooldowntick, "cooldowntick", 0);
            Scribe_Values.Look(ref durationcount, "durationcount", 0);
        }
    }
}
