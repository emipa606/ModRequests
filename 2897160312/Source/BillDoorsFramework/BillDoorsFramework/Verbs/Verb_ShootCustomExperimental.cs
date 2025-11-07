/*
namespace BillDoorsFramework
{
    public class Verb_ShootCustomExperimental : Verb_Shoot
    {
        List<availableFunc> AvailableValidators = new List<availableFunc>();

        List<action> WarmUpSuffixes = new List<action>();

        List<tryCastShotAction> TryCastShotExtraActions = new List<tryCastShotAction>();

        delegate bool availableFunc(Verb verb);

        delegate void action(Verb verb);

        delegate void tryCastShotAction(Verb verb, Func<bool> tryCastShot);
        public Verb_ShootCustomExperimental()
        {
            List<CompVerbCustomModules> comps = EquipmentSource.GetComps<CompVerbCustomModules>().ToList();
            if (comps.NullOrEmpty())
            {
                return;
            }
            for (int i = 0; i < comps.Count; i++)
            {
                AvailableValidators.Add(comps[i].AvailableValidators);
                WarmUpSuffixes.Add(comps[i].WarmUpSuffixes);
                TryCastShotExtraActions.Add(comps[i].TryCastShotAction);
            }
        }

        public override void WarmupComplete()
        {
            base.WarmupComplete();
            if (WarmUpSuffixes.NullOrEmpty())
            {
                return;
            }
            for (int i = 0; i < WarmUpSuffixes.Count; i++)
            {
                WarmUpSuffixes[i](this);
            }
        }

        public override bool Available()
        {
            if (AvailableValidators.NullOrEmpty())
            {
                return base.Available();
            }
            for (int i = 0; i < AvailableValidators.Count; i++)
            {
                if (!AvailableValidators[i](this))
                {
                    return false;
                }
            }
            return base.Available();
        }

        protected override bool TryCastShot()
        {
            if (base.TryCastShot())
            {
                if (TryCastShotExtraActions.NullOrEmpty())
                {
                    return true;
                }
                for (int i = 0; i < TryCastShotExtraActions.Count; i++)
                {
                    TryCastShotExtraActions[i](this, base.TryCastShot);
                }
                return true;
            }
            return false;
        }
    }

    public static class Verb_ShootCustomUtility
    {
        public static bool AvailableMechBattery(Verb verb)
        {
            DefModExtension_ShootUsingMechBattery DataMechBattery = verb.EquipmentSource.def.GetModExtension<DefModExtension_ShootUsingMechBattery>();
            Need_MechEnergy battery = verb.CasterPawn.needs.TryGetNeed<Need_MechEnergy>();
            if (battery != null)
            {
                return battery.CurLevel > DataMechBattery.energyConsumption;
            }
            return true;
        }

        public static bool AvailableNotUnderRoof(Verb verb)
        {
            CompSecondaryVerb compSecondaryVerb = verb.EquipmentSource.TryGetComp<CompSecondaryVerb>();
            ModExtension_VerbNotUnderRoof DataNotUnderRoof = verb.EquipmentSource.def.GetModExtension<ModExtension_VerbNotUnderRoof>();
            return !(verb.Caster.Position.Roofed(verb.Caster.Map) && (compSecondaryVerb == null || (compSecondaryVerb.IsSecondaryVerbSelected && DataNotUnderRoof.appliesInSecondaryMode) || (!compSecondaryVerb.IsSecondaryVerbSelected && DataNotUnderRoof.appliesInPrimaryMode)));
        }


        public static void WarmUpRandomizeProjectile(Verb verb)
        {
            DefModExtension_ShootUsingRandomProjectile DataRandProj = verb.EquipmentCompSource.parent.def.GetModExtension<DefModExtension_ShootUsingRandomProjectile>();
            if (DataRandProj != null)
            {
                int currenBurstRandomIndex = Rand.Range(0, DataRandProj.projectiles.Count - 1);
                verb.verbProps.defaultProjectile = DataRandProj.projectiles[currenBurstRandomIndex];
            }
        }

        public static void TryCastShotMechBattery(Verb verb)
        {
            DefModExtension_ShootUsingMechBattery DataMechBattery = verb.EquipmentSource.def.GetModExtension<DefModExtension_ShootUsingMechBattery>();
            if (ModLister.BiotechInstalled && DataMechBattery != null)
            {
                Need_MechEnergy battery = verb.CasterPawn.needs.TryGetNeed<Need_MechEnergy>();
                if (battery != null)
                {
                    battery.CurLevel -= DataMechBattery.energyConsumption;
                }
            }
        }

        public static void TryCastShotShotgun(Verb verb, Func<bool> tryCastShot)
        {
            ModExtension_Verb_Shotgun DataShotgun = verb.EquipmentSource.def.GetModExtension<ModExtension_Verb_Shotgun>();
            if (DataShotgun != null)
            {
                if (DataShotgun.ShotgunPellets > 1)
                {
                    for (int i = 1; i < DataShotgun.ShotgunPellets; i++)
                    {
                        tryCastShot();
                    }
                }
                if (DataShotgun.extraProjectile != null && DataShotgun.extraProjectileCount > 0)
                {
                    ThingDef originalProjectile = verb.verbProps.defaultProjectile;
                    verb.verbProps.defaultProjectile = DataShotgun.extraProjectile;
                    for (int i = 0; i < DataShotgun.extraProjectileCount; i++)
                    {
                        tryCastShot();
                    }
                    verb.verbProps.defaultProjectile = originalProjectile;
                }
            }
        }
    }

    public class CompVerbCustomModules : ThingComp
    {
        public virtual bool AvailableValidators(Verb verb)
        {
            return true;
        }

        public virtual void WarmUpSuffixes(Verb verb)
        {
        }

        public virtual void TryCastShotAction(Verb verb, Func<bool> tryCastShot)
        {
        }
    }
}

*/