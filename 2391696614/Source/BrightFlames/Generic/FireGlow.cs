using RimWorld;
using Verse;

namespace BrightFlames
{
	public class FireGlow : ThingWithComps
	{
		private Fire _fire;

		public void SetFire(Fire fire)
		{
			_fire = fire;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			if (_fire != null)
			{
				if (_fire.Destroyed || !_fire.Spawned) _fire = null;
			}
			Scribe_References.Look(ref _fire, "fire");
		}

		public override void Tick()
		{
			base.Tick();
			if (Find.TickManager.TicksGame % ModBaseBrightFlames.CheckInterval == 0)
			{
				if (_fire == null || _fire.Destroyed || !_fire.Spawned) Destroy();
			}
		}
	}
}
