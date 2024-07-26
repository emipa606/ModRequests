using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CryptoExpansion.HarmonyPatches;

[HarmonyPatch(typeof(PawnJumper), "LandingEffects")]
public static class PawnJumperPatch
{
  [HarmonyPostfix]
  public static void Postfix(PawnJumper __instance)
  {
    Pawn pawn = __instance.FlyingPawn;
    if (pawn.health?.hediffSet?.GetAllComps()?.Where(c => c is HediffComp_JumpEntropy { jumping: true }).FirstOrDefault() is not HediffComp_JumpEntropy comp)
      return;
    HediffCompProperties_JumpEntropy compProps = comp.Props;
    GenExplosion.DoExplosion(__instance.DestinationPos.ToIntVec3(),
      __instance.Map,
      compProps.radius,
      compProps.damType,
      pawn,
      damAmount: compProps.damAmount,
      armorPenetration: compProps.armorPenetration,
      explosionSound: compProps.explosionSound,
      postExplosionSpawnThingDef: compProps.postExplosionSpawnThingDef,
      postExplosionSpawnChance: compProps.postExplosionSpawnChance,
      postExplosionSpawnThingCount: compProps.postExplosionSpawnThingCount,
      postExplosionGasType: compProps.postExplosionGasType,
      applyDamageToExplosionCellsNeighbors: compProps.applyDamageToExplosionCellsNeighbors,
      preExplosionSpawnThingDef: compProps.preExplosionSpawnThingDef,
      preExplosionSpawnChance: compProps.preExplosionSpawnChance,
      preExplosionSpawnThingCount: compProps.preExplosionSpawnThingCount,
      chanceToStartFire: compProps.chanceToStartFire,
      damageFalloff: compProps.damageFalloff,
      propagationSpeed: compProps.propagationSpeed,
      excludeRadius: compProps.excludeRadius,
      postExplosionSpawnThingDefWater: compProps.postExplosionSpawnThingDefWater,
      screenShakeFactor: compProps.screenShakeFactor
    );
    comp.jumping = false;
    comp.Entropy = 0;
  }
}
