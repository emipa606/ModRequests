using RimWorld;
using Verse;

namespace Kraltech_Industries;

[DefOf]
public static class EffecterDefOf
{
    public static EffecterDef Plasma_Plasmawave;

    public static EffecterDef Charge_Chargedwave;

    public static EffecterDef PsychicExplosion;

    public static EffecterDef Deflect_Metal;

    public static EffecterDef Deflect_Metal_Bullet;

    public static EffecterDef Deflect_General;

    public static EffecterDef Deflect_General_Bullet;

    public static EffecterDef DamageDiminished_General;

    public static EffecterDef DamageDiminished_Metal;

    public static EffecterDef WarlordWrath;

    public static EffecterDef Shield_Break;

    public static EffecterDef Interceptor_BlockedProjectile;

    public static EffecterDef AntiOrganicBlast;

    public static EffecterDef Thunder_Explosion;

    public static EffecterDef Thunder_Strike;

    public static EffecterDef Thunder_Burst;

    static EffecterDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(EffecterDefOf));
    }
}