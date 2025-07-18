using RimWorld;
using Verse;

namespace Kraltech_Industries;

public class HediffComp_ImmunizableToxicRain : HediffComp_Immunizable
{
    public override float SeverityChangePerDay()
    {
        var num = base.SeverityChangePerDay();
        if (num < 0f && Pawn.Spawned && ModsConfig.BiotechActive)
        {
            if (Pawn.Position.IsPolluted(Pawn.Map) && Pawn.GetStatValue(StatDefOf.ToxicEnvironmentResistance) < 1f)
                num = 0f;
            else if (!Pawn.Position.Roofed(Pawn.Map))
                if (Pawn.Map.GameConditionManager.ActiveConditions.Any(x => x.def.conditionClass == typeof(GameCondition_ToxicRain)) && Pawn.GetStatValue(StatDefOf.ToxicResistance) < 1f)
                    num = 0f;
        }

        return num;
    }
}