using RimWorld;
using Verse;

namespace Kraltech_Industries;

public class HediffComp_RadiationPoisoning : HediffComp_Immunizable
{
    public override float SeverityChangePerDay()
    {
        var num = base.SeverityChangePerDay();
        if (num < 0f && Pawn.Spawned && ModsConfig.BiotechActive)
        {
            if (Pawn.Position.IsPolluted(Pawn.Map) && Pawn.GetStatValue(StatDefOf_NanitesEX.RadiationEnvironmentResistance) < 1f)
                num = 0f;
            else if (!Pawn.Position.Roofed(Pawn.Map))
                if (Pawn.Map.GameConditionManager.ActiveConditions.Any(x => x.def.conditionClass == typeof(GameCondition_DisableElectricityAndCauseRadiation)) && Pawn.GetStatValue(StatDefOf_NanitesEX.RadiationResistance) < 1f)
                    num = 0f;
        }

        return num;
    }
}