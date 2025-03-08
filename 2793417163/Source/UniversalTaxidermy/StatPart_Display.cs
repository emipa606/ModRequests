using RimWorld;

namespace EstupendoBase
{
	public class StatPart_Display : StatPart
	{
		public const float MarketValueFactor = 0.1f;

		public override void TransformValue(StatRequest req, ref float val)
		{
			if( req.HasThing && req.Thing is IDisplay impacted )
			{
				if( parentStat == StatDefOf.Mass )
				{
					val += impacted.GetMass();
					return;
				}
				if( parentStat == StatDefOf.MarketValue ) val *= MarketValueFactor;
				val *= impacted.GetImpactFactor();
				val += impacted.GetImpactOffset();
			}
		}

		public override string ExplanationPart(StatRequest req)
		{
			if( req.HasThing && req.Thing is IDisplay impacted )
			{
				float factor = impacted.GetImpactFactor();
				if( parentStat == StatDefOf.MarketValue ) factor *= MarketValueFactor;
				return "Display Impact: x" + factor.ToString("F2") + " + " + impacted.GetImpactOffset().ToString("F2");
			}
			return null;
		}
	}
}
