using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Noise;

namespace RadWorld
{
	public class StatPart_RadiationApparelProtection : StatPart
	{
		public override string ExplanationPart(StatRequest req)
		{
			return "";
		}

		public override void TransformValue(StatRequest req, ref float value)
		{
			if (req.Thing is Pawn pawn && pawn.apparel?.WornApparel != null)
            {
				foreach (var apparel in pawn.apparel.WornApparel)
                {
					if (RadWorldMod.settings.disableCustomRadiationSystem)
                    {
						value -= apparel.GetStatValue(RW_DefOf.RW_RadiationResistanceOffset);
						if (apparel.TryGetComp<CompGasMaskReloadable>()?.RemainingCharges > 0)
						{
							value -= apparel.GetStatValue(RW_DefOf.RW_RadiationResistancePoweredOffset);
						}
					}
                    else
					{
						value += apparel.GetStatValue(RW_DefOf.RW_RadiationResistanceOffset);
						if (apparel.TryGetComp<CompGasMaskReloadable>()?.RemainingCharges > 0)
						{
							value += apparel.GetStatValue(RW_DefOf.RW_RadiationResistancePoweredOffset);
						}
					}
                }
            }
		}
	}
}
