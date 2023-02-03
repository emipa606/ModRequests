namespace VFECore.Abilities
{
    using RimWorld;
    using RimWorld.Planet;
    using Verse;
	using Verse.AI;
    //using Ability = VFECore.Abilities.Ability;
    public class Ability_Flight : Ability
    {
        public override void Cast(LocalTargetInfo target)
        {
			/*CasterPawn.jobs.EndCurrentJob(JobCondition.InterruptForced, false);
            Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, target);
            CasterPawn.jobs.StartJob(job, JobCondition.InterruptForced);*/
			
			var map = Caster.Map;
            var flyer = (AbilityPawnFlyer_Valkyrie)PawnFlyer.MakeFlyer(VFE_DefOf_Abilities.VFEA_AbilityFlyer, CasterPawn, target.Cell);
            flyer.ability = this;
            flyer.target = target.Cell.ToVector3Shifted();
            GenSpawn.Spawn(flyer, Caster.Position, map);
            this.cooldown = Find.TickManager.TicksGame + this.GetCooldownForPawn();
        }
    }
	
	public class AbilityPawnFlyer_Valkyrie : AbilityPawnFlyer
    {
        protected override void RespawnPawn()
        {
            base.RespawnPawn();
            CasterPawn.jobs.EndCurrentJob(JobCondition.InterruptForced, false);
            Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, target);
            CasterPawn.jobs.StartJob(job, JobCondition.InterruptForced);
        }
    }
}