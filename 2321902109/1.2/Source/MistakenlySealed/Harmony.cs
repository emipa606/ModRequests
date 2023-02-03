using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace MistS
{
    [StaticConstructorOnStartup]
    static class Patch_Core
    {
        static Patch_Core()
        {
            var harmony = new HarmonyLib.Harmony("bep.mistakenlysealed");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

	// 味方への誤射阻止
	[HarmonyPatch(typeof(DamageWorker), "ExplosionDamageThing")]
    [HarmonyPatch(new Type[] { typeof(Explosion), typeof(Thing), typeof(List<Thing>), typeof(List<Thing>), typeof(IntVec3) })]
    internal static class Patches_EnemyKidnap
	{
		public static bool Prefix(Explosion explosion, Thing t, List<Thing> damagedThings, List<Thing> ignoredThings, IntVec3 cell, DamageWorker __instance)
		{
			if (__instance.def.GetModExtension<DoHitAllies>()?.DontHitAllies ?? false == true)
            {
                Pawn pawn = t as Pawn;
                if (pawn != null)
                {
                    Thing attacker = explosion.instigator;
                    if (attacker != null)
                    {
                        if (attacker.Faction != null && pawn.Faction != null)
                        {
                            if (attacker.Faction == pawn.Faction)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }
	}

}
