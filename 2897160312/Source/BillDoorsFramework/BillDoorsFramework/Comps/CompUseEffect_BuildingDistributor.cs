using RimWorld;
using System.Collections.Generic;
using Verse;

namespace BillDoorsFramework
{
    public class CompUseEffect_BuildingDistributor : CompUseEffect
    {
        public new CompProperties_BuildingDistributor Props
        {
            get { return (CompProperties_BuildingDistributor)this.props; }
        }

        bool activated = false;

        int triggerTick = 0;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref activated, "activated", false);
            Scribe_Values.Look(ref triggerTick, "triggerTick", 0);
        }

        public override void DoEffect(Pawn usedBy)
        {
            activated = true;
            triggerTick = Find.TickManager.TicksGame + Props.delayTick;
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            activated = Props.defaultActivated;
        }

        public override void CompTick()
        {
            base.CompTick();
            if (activated && Find.TickManager.TicksGame >= triggerTick)
            {
                Activate();
                CompExplosive compExplosive = parent.TryGetComp<CompExplosive>();
                if (compExplosive != null)
                {
                    compExplosive.StartWick();
                }
                else
                {
                    parent.Destroy(DestroyMode.KillFinalize);
                }
            }
        }

        public override void ReceiveCompSignal(string signal)
        {
            if (signal == "FlickedOff")
            {
                DoEffect(null);
            }
        }

        public void Activate(IntVec3 position, Map map)
        {
            int num = GenRadial.NumCellsInRadius(Props.radius);
            List<IntVec3> affectedCells = new List<IntVec3>();
            for (int i = 0; i < num; i++)
            {
                IntVec3 intVec = position + GenRadial.RadialPattern[i];
                affectedCells.Add(intVec);
            }
            for (int j = 0; j < Props.distributeAmount; j++)
            {
                int randIndex = Rand.Range(0, num - 1);
                FireProjectile(affectedCells[randIndex], position, map);
                num--;
                affectedCells.RemoveRange(randIndex, 1);
            }
        }

        public void Activate()
        {
            Activate(parent.Position, parent.Map);
        }

        public void FireProjectile(IntVec3 target, IntVec3 position, Map map)
        {
            if (Props.spawnedThingProjectile.category == ThingCategory.Projectile)
            {
                LocalTargetInfo targetInfo = new LocalTargetInfo(target);
                ShootLine resultingLine = new ShootLine(position, target);
                Projectile projectile = (Projectile)GenSpawn.Spawn(Props.spawnedThingProjectile, resultingLine.Source, map);
                projectile.Launch(parent, targetInfo, targetInfo, ProjectileHitFlags.All);
            }
            else
            {
                Thing thing = ThingMaker.MakeThing(Props.spawnedThingProjectile);
                thing.SetFaction(parent.Faction);
                if (!thing.def.MadeFromStuff)
                {
                    GenPlace.TryPlaceThing(thing, parent.Position, parent.Map, ThingPlaceMode.Near);
                }
            }
        }

        public override string CompInspectStringExtra()
        {
            if (activated)
            {
                return ((triggerTick - Find.TickManager.TicksGame) / 60).ToString();
            }
            return null;
        }

        public override AcceptanceReport CanBeUsedBy(Pawn p)
        {
            if (activated)
            {
                return "BDF_AlreadyTriggered".Translate();
            }
            return base.CanBeUsedBy(p);
        }
    }

    public class CompProperties_BuildingDistributor : CompProperties_UseEffect
    {
        public CompProperties_BuildingDistributor()
        {
            this.compClass = typeof(CompUseEffect_BuildingDistributor);
        }

        public ThingDef spawnedThingProjectile;

        public float radius = 10;

        public int distributeAmount = 1;

        public int delayTick = 25000;

        public bool defaultActivated = false;
    }
}
