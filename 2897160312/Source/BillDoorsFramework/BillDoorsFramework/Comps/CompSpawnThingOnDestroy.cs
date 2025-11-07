using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace BillDoorsFramework
{
    public class CompSpawnThingOnDestroy : ThingComp
    {
        public CompProperties_SpawnThingOnDestroy Props
        {
            get
            {
                return (CompProperties_SpawnThingOnDestroy)props;
            }
        }

        protected virtual Faction GetFaction()
        {
            Faction faction = parent is Projectile projectile ? projectile.Launcher.Faction : parent.Faction;
            if (faction == null && Props.playerFactionFallback)
            {
                faction = Faction.OfPlayer;
            }
            return faction;
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            Thing thing = ThingMaker.MakeThing(Props.def);
            thing.SetFaction(GetFaction());
            if (previousMap != null)
            {
                GenPlace.TryPlaceThing(thing, parent.Position, previousMap, ThingPlaceMode.Near);
                if (thing is Pawn pawn)
                {
                    LordMaker.MakeNewLord(pawn.Faction, new LordJob_DefendPoint(parent.Position, 5, 40), previousMap, new List<Pawn>() { pawn });
                }
            }
        }
    }

    public class CompProperties_SpawnThingOnDestroy : CompProperties
    {
        public CompProperties_SpawnThingOnDestroy()
        {
            compClass = typeof(CompSpawnThingOnDestroy);
        }

        public ThingDef def;

        public bool playerFactionFallback;
    }
}
