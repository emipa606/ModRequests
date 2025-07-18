using RimWorld;
using Verse;

namespace Kraltech_Industries;

[DefOf]
public static class DamageDefOf
{
    public static DamageDef BlastFlame;

    public static DamageDef EMP;

    public static DamageDef Bullet;

    public static DamageDef Flame;

    public static DamageDef AntiHumanNanites;

    public static DamageDef ThunderWaveUlt;

    static DamageDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(DamageDefOf));
    }
}