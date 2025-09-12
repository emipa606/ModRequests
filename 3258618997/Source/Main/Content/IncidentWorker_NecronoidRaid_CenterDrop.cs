using System;
using RimWorld;
using Verse;

namespace Necro
{
    
    internal class IncidentWorker_NecronoidRaid_CenterDrop : IncidentWorker
    {
        
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return false;
        }

        
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, (Map)parms.target);
            incidentParms.faction = Find.FactionManager.FirstFactionOfDef(FactionDefOf.Necronoid);
            incidentParms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
            incidentParms.raidArrivalMode = PawnsArrivalModeDefOf.CenterDrop;
            incidentParms.points *= 5f;
            incidentParms.customLetterDef = NecroDefOf.Necronoid_NegativeEvent;
            Find.Storyteller.incidentQueue.Add(IncidentDefOf.RaidEnemy, Find.TickManager.TicksGame, incidentParms, 0);
            return true;
        }

        
        public IncidentWorker_NecronoidRaid_CenterDrop()
        {
        }
    }
}
