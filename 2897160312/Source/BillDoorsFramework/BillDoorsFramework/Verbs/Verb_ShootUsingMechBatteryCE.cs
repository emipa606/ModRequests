using RimWorld;
using System.Collections.Generic;
using Verse;

namespace BillDoorsFramework
{
    public class Verb_ShootUsingMechBattery : Verb_Shoot
    {
        public DefModExtension_ShootUsingMechBattery Data
        {
            get
            {
                return EquipmentCompSource.parent.def.GetModExtension<DefModExtension_ShootUsingMechBattery>();
            }
        }

        public DefModExtension_ShootUsingRandomProjectileBase DataProj
        {
            get
            {
                return EquipmentCompSource.parent.def.GetModExtension<DefModExtension_ShootUsingRandomProjectileBase>();
            }
        }

        public ModExtension_Verb_Shotgun DataShotgun
        {
            get
            {
                return EquipmentCompSource.parent.def.GetModExtension<ModExtension_Verb_Shotgun>();
            }
        }

        int currenBurstRandomIndex;

        public override void WarmupComplete()
        {
            if (DataProj != null)
            {
                verbProps.defaultProjectile = DataProj.GetProjectile();
            }
            base.WarmupComplete();
        }

        public override bool Available()
        {
            if (ModLister.BiotechInstalled && Data != null)
            {
                Need_MechEnergy battery = CasterPawn.needs.TryGetNeed<Need_MechEnergy>();
                if (battery != null)
                {
                    return battery.CurLevel > Data.energyConsumption;
                }
            }
            return base.Available();
        }

        protected override bool TryCastShot()
        {
            if (base.TryCastShot())
            {
                if (ModLister.BiotechInstalled && Data != null)
                {
                    Need_MechEnergy battery = CasterPawn.needs.TryGetNeed<Need_MechEnergy>();
                    if (battery != null)
                    {
                        battery.CurLevel -= Data.energyConsumption;
                    }
                }
                if (DataProj != null && DataProj.randomWithinBurst)
                {
                    verbProps.defaultProjectile = DataProj.GetProjectile();
                }
                if (DataShotgun != null)
                {
                    if (DataShotgun.ShotgunPellets > 1)
                    {
                        for (int i = 1; i < DataShotgun.ShotgunPellets; i++)
                        {
                            base.TryCastShot();
                        }
                    }
                    if (DataShotgun.extraProjectile != null && DataShotgun.extraProjectileCount > 0)
                    {
                        ThingDef originalProjectile = verbProps.defaultProjectile;
                        verbProps.defaultProjectile = DataShotgun.extraProjectile;
                        for (int i = 0; i < DataShotgun.extraProjectileCount; i++)
                        {
                            base.TryCastShot();
                        }
                        verbProps.defaultProjectile = originalProjectile;
                    }
                }
                return true;
            }
            return false;
        }
    }
    public class DefModExtension_ShootUsingMechBattery : DefModExtension
    {
        public float energyConsumption = 0.001f;
    }

    public class Verb_ShootWithRandomProjectile : Verb_Shoot
    {
        public DefModExtension_ShootUsingRandomProjectileBase DataProj
        {
            get
            {
                return EquipmentCompSource.parent.def.GetModExtension<DefModExtension_ShootUsingRandomProjectileBase>();
            }
        }


        public override void WarmupComplete()
        {
            if (DataProj != null)
            {
                verbProps.defaultProjectile = DataProj.GetProjectile();
            }
            base.WarmupComplete();
        }

        protected override bool TryCastShot()
        {
            if (DataProj != null && DataProj.randomWithinBurst)
            {
                verbProps.defaultProjectile = DataProj.GetProjectile();
            }
            if (base.TryCastShot())
            {
                return true;
            }
            return false;
        }
    }

    public abstract class DefModExtension_ShootUsingRandomProjectileBase : DefModExtension
    {
        public abstract ThingDef GetProjectile();


        public bool randomWithinBurst = false;
    }

    //Backward Compatibility
    public class DefModExtension_ShootUsingRandomProjectile : DefModExtension_ShootUsingRandomProjectileBase
    {
        public override ThingDef GetProjectile()
        {
            return projectiles.RandomElement();
        }


        public List<ThingDef> projectiles = new List<ThingDef>();
    }

    public class DefModExtension_ShootUsingRandomProjectileNew : DefModExtension_ShootUsingRandomProjectileBase
    {
        public override ThingDef GetProjectile()
        {
            return projectiles.RandomElementByWeight(t => t.count).thingDef;
        }


        public List<ThingDefCountClass> projectiles = new List<ThingDefCountClass>();
    }
}
