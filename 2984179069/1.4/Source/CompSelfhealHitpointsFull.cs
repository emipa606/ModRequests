using Verse;

public class CompSelfhealHitpointsFull : ThingComp
{
	public int ticksPassedSinceLastHeal;

	public CompProperties_SelfhealHitpointsFull Props => (CompProperties_SelfhealHitpointsFull)props;

	public override void PostExposeData()
	{
		Scribe_Values.Look(ref ticksPassedSinceLastHeal, "ticksPassedSinceLastHeal", 0);
	}

	public override void CompTick()
	{
		Tick(1);
	}

	public override void CompTickRare()
	{
		Tick(250);
	}

	public override void CompTickLong()
	{
		Tick(2000);
	}

	private void Tick(int ticks)
	{
		ticksPassedSinceLastHeal += ticks;
		if (ticksPassedSinceLastHeal >= Props.ticksPerHeal)
		{
			ticksPassedSinceLastHeal = 0;
			if (parent.HitPoints < parent.MaxHitPoints)
			{
				parent.HitPoints = parent.MaxHitPoints;
			}
		}
	}
}
