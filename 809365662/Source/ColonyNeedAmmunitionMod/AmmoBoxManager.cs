using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ColonyNeedAmmunitionMod
{

    public enum AmmunitionType {
        None,
        LowCalibar,
        ShotgunShell,
        HighCalibar,
        SniperRifle,
        Arrow,
        Energy
    }

    public class AmmoBoxManager
    {
        private static AmmoBoxManager self = null;
        private Dictionary<string, AmmunitionType> typesinst = null;
        private bool initGunsAmmoTypes() {
            typesinst = new Dictionary<string, AmmunitionType>();

            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs) {
                if (def.IsRangedWeapon)
                {
                    DamageDef damage = def.Verbs.First().projectileDef.projectile.damageDef;
                    if (damage == DamageDefOf.Arrow)
                    {
                        typesinst.Add(def.defName, AmmunitionType.Arrow);
                        continue;
                    }
                    else if (damage == DamageDefOf.Bullet) {
                        if (def.techLevel == TechLevel.Spacer)
                        {
                            typesinst.Add(def.defName, AmmunitionType.Energy);
                            continue;
                        }
                        if (40 <= def.Verbs.First().range)
                        {
                            typesinst.Add(def.defName, AmmunitionType.SniperRifle);
                            continue;
                        }
                        if (def.Verbs.First().range < 24 && 14 <= def.Verbs.First().projectileDef.projectile.damageAmountBase)
                        {
                            typesinst.Add(def.defName, AmmunitionType.ShotgunShell);
                            continue;
                        }
                        if (IsHighCalbar(def))
                        {
                            typesinst.Add(def.defName, AmmunitionType.HighCalibar);
                            continue;
                        }
                        else
                        {
                            typesinst.Add(def.defName, AmmunitionType.LowCalibar);
                            continue;
                        }
                    }
                }
            }

            return typesinst.Count != 0;
        }

        private bool IsHighCalbar(ThingDef def) {
            float damage, warm, cool, burstCnt, burstInt;
            damage = def.Verbs.First().projectileDef.projectile.damageAmountBase;
            warm = def.Verbs.First().warmupTicks;
            cool = def.Verbs.First().defaultCooldownTicks;
            burstCnt = def.Verbs.First().burstShotCount;
            burstInt = def.Verbs.First().ticksBetweenBurstShots;

            float dps = (damage * burstCnt) / (warm + cool + burstCnt * burstInt);

            return 7 <= damage && 0.15 <= dps;
        }
        public Dictionary<string, AmmunitionType> GunsAmmoTypes {
            get {
                if (typesinst == null || typesinst.Count == 0) {
                    if (!initGunsAmmoTypes()) {
                        return null;
                    }
                }
                return typesinst;
            }
        }


        public static AmmoBoxManager getInstance() {
            if (self == null) {
                self = new AmmoBoxManager();
            }
            return self;
        }

        public List<Building_AmmoBox> boxes;

        public AmmoBoxManager() {
            boxes = new List<Building_AmmoBox>();
        }

        public void NotifyAmmoChangeInBox(Building_AmmoBox box, AmmunitionType type , int iChange) {

        }

        public static bool isAmmo(ThingDef def) {
            AmmunitionType ammo = ReferAmmunitionDef(def);
            if (ammo != AmmunitionType.None) return true;
            return false;
        }

        public AmmunitionType getAmmunitionType(ThingDef def) {
            if (!GunsAmmoTypes.Keys.Contains(def.defName)) {
                return AmmunitionType.None;
            }
            return GunsAmmoTypes[def.defName];
        }

        public static String getAmmunitionDefNameByType(AmmunitionType type) {
            switch (type) {
                case AmmunitionType.LowCalibar:
                    return "LowCalibarAmmo";
                case AmmunitionType.ShotgunShell:
                    return "ShotgunShell";
                case AmmunitionType.HighCalibar:
                    return "HighCalibarAmmo";
                case AmmunitionType.SniperRifle:
                    return "SniperRifleAmmo";
                case AmmunitionType.Arrow:
                    return "ArrowAmmo";
                case AmmunitionType.Energy:
                    return "EnergyAmmo";
            }
            return "";
        }

        public static AmmunitionType ReferAmmunitionDef(ThingDef def)
        {
            switch (def.defName)
            {
                case "LowCalibarAmmo":
                    return AmmunitionType.LowCalibar;
                case "ShotgunShell":
                    return AmmunitionType.ShotgunShell;
                case "HighCalibarAmmo":
                    return AmmunitionType.HighCalibar;
                case "SniperRifleAmmo":
                    return AmmunitionType.SniperRifle;
                case "ArrowAmmo":
                    return AmmunitionType.Arrow;
                case "EnergyAmmo":
                    return AmmunitionType.Energy;
            }
            return AmmunitionType.None;
        }

        public int CountAmmunition(AmmunitionType type) {
            int ret = 0;
            foreach (Building_AmmoBox box in boxes) {
                foreach (IntVec3 cell in box.AllSlotCells()) {
                    foreach (Thing mono in cell.GetThingList()) {
                        if (box.GetStoreSettings().AllowedToAccept(mono)) {
                            if (type == ReferAmmunitionDef(mono.def)) {
                                ret += mono.stackCount;
                            }
                        }
                    }
                }
            }
            return ret;
        }

        public bool Consume(AmmunitionType type) {
            foreach (Building_AmmoBox box in boxes)
            {
                if (box.ConsumeAmmo(type)) {
                    return true;
                }
            }
            return false;
        }

        private int ActualConsumeAmmo(AmmunitionType type, int amount) {
            int consumed = 0;

            return consumed;
        }
    }
}
