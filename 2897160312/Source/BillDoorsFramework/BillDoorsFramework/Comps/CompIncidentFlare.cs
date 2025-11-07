using RimWorld;
using Verse;

namespace BillDoorsFramework
{
    public class CompIncidentFlare : CompUseEffect
    {
        private CompProperties_IncidentFlare Props => (CompProperties_IncidentFlare)props;

        public override void DoEffect(Pawn usedBy)
        {
            IncidentParms parms = new IncidentParms
            {
                target = parent.Map,
                forced = true,
                spawnCenter = parent.Position,
            };

            if (usedBy != null)
            {
                base.DoEffect(usedBy);
                parms.faction = usedBy.Faction;
            }
            else if (parent is Projectile projectile)
            {
                parms.faction = projectile.Launcher.Faction;
            }
            else
            {
                parms.faction = parent.Faction;
            }
            Props.incidentDef.Worker.TryExecute(parms);
        }

        public void DoEffect(Faction faction)
        {
            IncidentParms parms = new IncidentParms
            {
                target = parent.Map,
                forced = true,
                faction = faction,
            };
            if (Props.passCellInfo)
            {
                parms.spawnCenter = parent.Position;
            }
            if (Props.points > 0)
            {
                parms.points = Props.points;
            }
            else
            {
                parms.points = parent.Map.IncidentPointsRandomFactorRange.RandomInRange;
            }
            Props.incidentDef.Worker.TryExecute(parms);
        }
    }

    public class CompProperties_IncidentFlare : CompProperties_UseEffect
    {
        public IncidentDef incidentDef;

        public bool passCellInfo = true;

        public int points = -1;

        [NoTranslate]
        public string signal = "activate";

        public FactionDef faction;

        public CompProperties_IncidentFlare()
        {
            compClass = typeof(CompIncidentFlare);
        }
    }
}
