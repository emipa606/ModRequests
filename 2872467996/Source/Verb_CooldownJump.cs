using RimWorld;
using Verse;

namespace MVCF.Verbs
{
	[DefOf]
	public static class Valk_DefOf
	{
        public static ThingDef PawnJumper_ValkyrieFlight;
	}
    public class Verb_CooldownJump : RimWorld.Verb_Jump
    {
		public int cooldown = 0;
        protected override float EffectiveRange => verbProps.range;
		
		public override bool Available()
		{
			if (Find.TickManager.TicksGame > this.cooldown)
			{
				return true;
			}
			return false;
		}
        protected override bool TryCastShot()
        {
            if (!ModLister.RoyaltyInstalled)
            {
                Log.ErrorOnce(
                    "Items with jump capability are a Royalty-specific game system. If you want to use this code please check ModLister.RoyaltyInstalled before calling it. See rules on the Ludeon forum for more info.",
                    550187797);
                return false;
            }
			
            var casterPawn = CasterPawn;
            var cell = currentTarget.Cell;
            var map = casterPawn.Map;
            var pawnFlyer = PawnFlyer.MakeFlyer(Valk_DefOf.PawnJumper_ValkyrieFlight, casterPawn, cell);
            if (pawnFlyer != null)
            {
                GenSpawn.Spawn(pawnFlyer, cell, map);
				this.cooldown = Find.TickManager.TicksGame + 3600;	//60s cooldown
                return true;
            }
			
            return false;
        }
    }
}