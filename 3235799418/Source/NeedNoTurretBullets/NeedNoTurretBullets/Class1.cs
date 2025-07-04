using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace NeedNoTurretBullets
{
    public class TurretBulletsPatchAll : Mod
    {
        public static Type turretGunCEType;
        public static MethodInfo tickMethod;
        public static MethodInfo postfixMethod;
        public static PropertyInfo compAmmoProp;
        public static PropertyInfo curMagCountProp;
        public static PropertyInfo magSizeProp;

        public TurretBulletsPatchAll(ModContentPack content)
            : base(content)
        {
            Harmony harmony = new Harmony("Mengya.NeedNoTurretBullets");

            if (IsCombatExtendedLoaded())
            {
                turretGunCEType = AccessTools.TypeByName("CombatExtended.Building_TurretGunCE");
                if (turretGunCEType != null)
                {
                    tickMethod = AccessTools.Method(turretGunCEType, "Tick");
                    postfixMethod = AccessTools.Method(typeof(TurretBulletsCEPatch), "Postfix");

                    compAmmoProp = AccessTools.Property(turretGunCEType, "CompAmmo");
                    if (compAmmoProp != null)
                    {
                        Type compAmmoType = compAmmoProp.PropertyType;
                        curMagCountProp = AccessTools.Property(compAmmoType, "CurMagCount");
                        magSizeProp = AccessTools.Property(compAmmoType, "MagSize");
                    }

                    if (tickMethod != null && postfixMethod != null)
                    {
                        harmony.Patch(tickMethod, postfix: new HarmonyMethod(postfixMethod));
                    }
                }
            }
            else
            {
                harmony.Patch(AccessTools.Method(typeof(CompRefuelable), "ConsumeFuel"),
                    prefix: new HarmonyMethod(typeof(TurretBulletsPatch), "TurretBulletsPatcher"));
            }
        }

        private bool IsCombatExtendedLoaded()
        {
            return AccessTools.TypeByName("CombatExtended.Building_TurretGunCE") != null;
        }
    }

    public static class TurretBulletsPatch
    {
        public static bool TurretBulletsPatcher(CompRefuelable __instance)
        {
            // 仅我方炮塔无限子弹
            if (__instance.parent.Faction == Faction.OfPlayer && __instance.parent.def.building.IsTurret)
            {
                return false; // 跳过燃料消耗
            }
            return true;
        }
    }

    public static class TurretBulletsCEPatch
    {
        public static void Postfix(object __instance)
        {
            if (TurretBulletsPatchAll.compAmmoProp == null ||
                TurretBulletsPatchAll.curMagCountProp == null ||
                TurretBulletsPatchAll.magSizeProp == null) return;

            object compAmmo = TurretBulletsPatchAll.compAmmoProp.GetValue(__instance);
            if (compAmmo == null) return;

            // 仅我方炮塔补充子弹
            if (__instance is ThingWithComps turret && turret.Faction == Faction.OfPlayer)
            {
                int curMagCount = (int)TurretBulletsPatchAll.curMagCountProp.GetValue(compAmmo);
                int magSize = (int)TurretBulletsPatchAll.magSizeProp.GetValue(compAmmo);

                // 确保子弹不会超过最大上限
                if (curMagCount < magSize)
                {
                    TurretBulletsPatchAll.curMagCountProp.SetValue(compAmmo, magSize);
                }
                else if (curMagCount > magSize)
                {
                    TurretBulletsPatchAll.curMagCountProp.SetValue(compAmmo, magSize); // 修正超量子弹
                }
            }
        }
    }
}
