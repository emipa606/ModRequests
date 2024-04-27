using System.Collections.Generic;
using Verse;

public class CompProperties_SelfhealHitpointsFull : CompProperties
{
	public int ticksPerHeal;

	public CompProperties_SelfhealHitpointsFull()
	{
		compClass = typeof(CompSelfhealHitpointsFull);
	}

	public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
	{
		foreach (string item in base.ConfigErrors(parentDef))
		{
			yield return item;
		}
		if (parentDef.tickerType == TickerType.Rare && ticksPerHeal % 250 != 0)
		{
			yield return "TickerType is set to Rare, but ticksPerHeal value is not multiple of " + 250;
		}
		if (parentDef.tickerType == TickerType.Long && ticksPerHeal % 2000 != 0)
		{
			yield return "TickerType is set to Long, but ticksPerHeal value is not multiple of " + 2000;
		}
		if (parentDef.tickerType == TickerType.Never)
		{
			yield return "has CompSelfhealHitpoints, but its TickerType is set to Never";
		}
	}
}
