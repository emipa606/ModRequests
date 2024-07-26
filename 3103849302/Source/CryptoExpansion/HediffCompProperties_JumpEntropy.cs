using RimWorld;
using Verse;

namespace CryptoExpansion;

public class HediffCompProperties_JumpEntropy : HediffCompProperties
{
  public float radius = 5f;
  public DamageDef damType = null;
  public int damAmount = -1;
  public float armorPenetration = -1f;
  public SoundDef explosionSound = null;
  public ThingDef postExplosionSpawnThingDef = null;
  public float postExplosionSpawnChance = 0.0f;
  public int postExplosionSpawnThingCount = 1;
  public GasType? postExplosionGasType = null;
  public bool applyDamageToExplosionCellsNeighbors = false;
  public ThingDef preExplosionSpawnThingDef = null;
  public float preExplosionSpawnChance = 0.0f;
  public int preExplosionSpawnThingCount = 1;
  public float chanceToStartFire = 0.0f;
  public bool damageFalloff = false;
  public float propagationSpeed = 1f;
  public float excludeRadius = 1.0f;
  public ThingDef postExplosionSpawnThingDefWater = null;
  public float screenShakeFactor = 1f;

  public HediffCompProperties_JumpEntropy()
  {
    compClass = typeof(HediffComp_JumpEntropy);
  }

  public override void ResolveReferences(HediffDef parent)
  {
    base.ResolveReferences(parent);
    if (damType != null)
      return;
    damType = DamageDefOf.Bomb;
  }
}
