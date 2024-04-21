using RimWorld;
using Verse;
using HarmonyLib;
using System;
using System.Linq;

[StaticConstructorOnStartup]
public static class Main
{
    static Main()
    {
        var usingCe = false;
        Type turretRefCe = null;
        var harmony = new Harmony("com.infinite.turrets");

        // Try locate CombatExtended
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var asm in assemblies) {
            if (asm.GetName().Name == "CombatExtended") {
                turretRefCe = asm.GetType("CombatExtended.Building_TurretGunCE");
                usingCe = true;
                break;
            }
        }

        // Vanilla game / non-CE turrets
        var refuelTargetMethod = AccessTools.Method(typeof(CompRefuelable), "CompTick", null, null);
        var refuelPrefix = new HarmonyMethod(typeof(VanillaPatch).GetMethod("Prefix"));
        harmony.Patch(refuelTargetMethod, refuelPrefix, null, null, null);

        // All CE turrets
        if (usingCe && turretRefCe != null) {
            Log.Message("[Infinite Turrets] Using CombatExtended patch");
            var ceTargetMethod = AccessTools.Method(turretRefCe, "Tick", null, null);
            var cePrefix = new HarmonyMethod(typeof(CePatch).GetMethod("Prefix"));
            harmony.Patch(ceTargetMethod, cePrefix, null, null, null);
        }
    }
}

static class VanillaPatch
{
    public static bool Prefix(CompRefuelable __instance)
    {
        var thingDef = __instance.parent.def;
        if (thingDef.designationCategory == DesignationCategoryDefOf.Security) {

            // Check 1: If it has defined a `turretGunDef`
            if (thingDef.building.turretGunDef != null) {
                __instance.Refuel(__instance.Props.fuelCapacity);
                return false;
            }

            // Check 2: If its thingClass is `Building_TurretGun`
            if (thingDef.thingClass.Name == "Building_TurretGun") {
                __instance.Refuel(__instance.Props.fuelCapacity);
                return false;
            }

            // Check 3: (Fallback) If its refuel label contains "shots/barrel/rearm"
            var label = __instance.Props.FuelGizmoLabel.ToLower();
            var label2 = __instance.Props.FuelLabel.ToLower();
            string[] match = { "barrel", "shots", "rearm" };
            if (match.Any(val => label.Contains(val) || label2.Contains(val))) {
                __instance.Refuel(__instance.Props.fuelCapacity);
                return false;
            }
        }
        return true;
    }
}

static class CePatch
{
    public static Type CEammoType = Array.Find(AppDomain.CurrentDomain.GetAssemblies(), asm => 
        asm.GetName().Name == "CombatExtended")
        .GetType("CombatExtended.AmmoDef");

    public static Type CEammoUser = Array.Find(AppDomain.CurrentDomain.GetAssemblies(), asm =>
        asm.GetName().Name == "CombatExtended")
        .GetType("CombatExtended.CompAmmoUser");

    public static bool Prefix(ref object compAmmo) 
    {
        compAmmo = Convert.ChangeType(compAmmo, CEammoUser);
        compAmmo?.GetType().GetMethod("ResetAmmoCount")?.Invoke(compAmmo, new object[] { null });
        return true;
    }
}