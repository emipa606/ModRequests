using RimWorld;
using Verse;

namespace Kraltech_Industries;

[DefOf]
public static class HediffDefOf
{
    public static HediffDef DragonsAura;

    public static HediffDef DragonScales;

    public static HediffDef EmpressDecree;

    public static HediffDef BerserkerAura;

    public static HediffDef PsyRejuvenate;

    public static HediffDef EnhancedCombatModule;

    public static HediffDef ArmorBreakerEX;

    public static HediffDef SeverePsychicDrone;

    public static HediffDef RadiationPoisoning;

    public static HediffDef NeuroToxin;

    public static HediffDef HeatNullNanites;

    public static HediffDef ColdNullNanites;

    public static HediffDef AntiOrganicToxin;

    public static HediffDef LungRot;

    public static HediffDef RageVirus;

    static HediffDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(HediffDefOf));
    }
}