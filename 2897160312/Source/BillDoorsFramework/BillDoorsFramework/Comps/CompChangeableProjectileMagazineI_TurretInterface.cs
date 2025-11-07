using RimWorld;
using System;
using Verse;
using BillDoorsUnifiedHaulJob;
using Verse.Noise;

namespace BillDoorsFramework
{
    public class CompChangeableProjectileMagazineI_TurretInterface : ThingComp, IRefuelable
    {
        public ThingRequest CurrentRequest => GunRefuelable.CurrentRequest;

        public int RequestAmount => GunRefuelable.RequestAmount;

        public bool ShouldRefuelNow
        {
            get
            {
                if (TurretGun != null)
                {
                    return !(GunRefuelable.Loaded && (TurretGun.TargetCurrentlyAimingAt.IsValid || GunRefuelable.Full));
                }
                return !GunRefuelable.Loaded;
            }
        }

        public Predicate<Thing> FuelValidator => GunRefuelable.FuelValidator;

        public float SearchRadius => GunRefuelable.SearchRadius;

        public int RefuelWaitTick => GunRefuelable.RefuelWaitTick;

        public Thing FoundFuel { get => GunRefuelable.FoundFuel; set => GunRefuelable.FoundFuel = value; }

        public Thing Parent => parent;

        Building_TurretGun TurretGun => parent as Building_TurretGun;

        CompChangeableProjectileMagazineI GunRefuelable => ((ThingWithComps)TurretGun.gun).GetComp<CompChangeableProjectileMagazineI>();

        public void RefuelEffect(Thing fuel, Pawn pawn, params object[] parms)
        {
            GunRefuelable.RefuelEffect(fuel, pawn, parms);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            IRefuelableUtil.SpawnRegister(this);
        }

        public override void PostDeSpawn(Map map)
        {
            IRefuelableUtil.Deregister(map, this);
        }

        public string GetUniqueLoadID()
        {
            return $"CompChangableMagAdaptor_{parent.AllComps.IndexOf(this)}_" + parent.ThingID;
        }
    }
}
