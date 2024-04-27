using RimWorld;
using Verse;

public class CompSelfDestroyOnceNotDormant : ThingComp
{
	public int ticksPassedSinceLastHeal;

	public CompProperties_SelfDestroyOnceNotDormant Props => (CompProperties_SelfDestroyOnceNotDormant)props;

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
			CompMAG_ArchotechHibernatable comp = parent.GetComp<CompMAG_ArchotechHibernatable>();
			if (comp != null && comp.State == HibernatableStateDefOf.Running)
			{
				parent.TakeDamage(new DamageInfo(DamageDefOf.Crush, 1E+11f));
			}
		}
	}
}
