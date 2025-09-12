using System;
using System.Runtime.CompilerServices;
using RimWorld;
using Verse;

namespace Necro
{
    
    public class HediffComp_ImmunizableNecrotion : HediffComp_Immunizable
    {
        
        public override float SeverityChangePerDay()
        {
            float num = base.SeverityChangePerDay();
            if (num < 0f && base.Pawn.Spawned && ModsConfig.BiotechActive)
            {
                if (base.Pawn.Position.IsPolluted(base.Pawn.Map) && base.Pawn.GetStatValue(NecroDefOf.NecrotionEnvironmentResistance, true, -1) < 1f)
                {
                    num = 0f;
                }
                else if (!base.Pawn.Position.Roofed(base.Pawn.Map))
                {
                    if (base.Pawn.Map.GameConditionManager.ActiveConditions.Any((GameCondition x) => x.def.conditionClass == typeof(GameCondition_Necrotion)) && base.Pawn.GetStatValue(NecroDefOf.NecrotionResistance, true, -1) < 1f)
                    {
                        num = 0f;
                    }
                }
            }
            return num;
        }
        
}
}
