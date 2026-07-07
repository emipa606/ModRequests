using Verse;
using RimWorld;
using Verse.AI;
using System.Collections.Generic;
using UnityEngine;



public class VengefulRageSettings : ModSettings
{

	public bool canTriggerVengefulRage = true;
	public float triggerChance = 0.5f;

	public override void ExposeData()
	{
		Scribe_Values.Look(ref canTriggerVengefulRage, "canTriggerVengefulRage");
		Scribe_Values.Look(ref triggerChance, "triggerChance", 0.5f);
		base.ExposeData();
	}
}

public class VengefulRageMod : Mod
{
	public static VengefulRageSettings settings;
	public VengefulRageMod(ModContentPack content) : base(content)
	{
		settings = GetSettings<VengefulRageSettings>();
	}
	public override void DoSettingsWindowContents(Rect inRect)
	{
		Listing_Standard listingStandard = new Listing_Standard();
		listingStandard.Begin(inRect);
		string triggerChanceLabel = $"Vengeful Rage Trigger Chance: {settings.triggerChance:P0}";
		listingStandard.Label(triggerChanceLabel, tooltip: "Adjust the probability that Vengeful Rage will be triggered upon a qualifying event. 0% means it will never trigger, while 100% means it will always trigger.");
		settings.triggerChance = listingStandard.Slider(settings.triggerChance, 0f, 1.0f);

		listingStandard.End();
		base.DoSettingsWindowContents(inRect);
	}

	public override string SettingsCategory()
	{
		return "VengefulRage";
	}
}
public class MentalState_VengefulRage : MentalState
{
	private Pawn _target;
	public Pawn Target
	{
		get => _target;
		set
		{
			_target = value;
		}
	}

	public override void PostStart(string reason)
	{
		base.PostStart(reason);
		Hediff hediff = HediffMaker.MakeHediff(HediffDef.Named("RevengeFocus"), pawn);
		pawn.health.AddHediff(hediff);
	}
	public override void PostEnd()
	{
		base.PostEnd();
		Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("RevengeFocus"));
		if (hediff != null)
		{
			// Remove the RevengeFocus hediff
			pawn.health.RemoveHediff(hediff);
		}
		if (pawn.needs.mood != null)
		{
			ThoughtDef catharsisThought = ThoughtDef.Named("Catharsis");
			pawn.needs.mood.thoughts.memories.TryGainMemory(catharsisThought);
		}
	}
	public override RandomSocialMode SocialModeMax()
	{
		return RandomSocialMode.Off;
	}
	public override void MentalStateTick()
	{
		base.MentalStateTick();
		if (_target != null && _target.Dead)
		{
			RecoverFromState();
		}
		if (_target == null)
		{
			Log.Message($"No target for VengefulRage. Should have been");
		}
		JobGiver_VengefulRage jobGiver = new JobGiver_VengefulRage();
		ThinkResult result = jobGiver.TryIssueJobPackage(this.pawn, default(JobIssueParams));
		if (result.Job != null)
		{
			this.pawn.jobs.TryTakeOrderedJob(result.Job);
		}
	}

	public bool IsTargetStillValidAndReachable()
	{
		if (_target != null && _target.SpawnedParentOrMe != null && (!(_target.SpawnedParentOrMe is Pawn) || _target.SpawnedParentOrMe == _target))
		{
			return pawn.CanReach(_target.SpawnedParentOrMe, PathEndMode.Touch, Danger.Deadly, canBashDoors: true);
		}
		return false;
	}


}

public class JobGiver_VengefulRage : ThinkNode_JobGiver
{
	protected override Job TryGiveJob(Pawn pawn)
	{
		if (!(pawn.MentalState is MentalState_VengefulRage mentalState_VengefulRage) || !mentalState_VengefulRage.IsTargetStillValidAndReachable())
		{
			return null;
		}
		Thing spawnedParentOrMe = mentalState_VengefulRage.Target.SpawnedParentOrMe;
		Job job;
		if (pawn.equipment.Primary != null && !pawn.equipment.Primary.def.IsMeleeWeapon)
		{
			job = JobMaker.MakeJob(JobDefOf.AttackStatic, spawnedParentOrMe);
		}
		else
		{
			job = JobMaker.MakeJob(JobDefOf.AttackMelee, spawnedParentOrMe);
		}
		job.canBashDoors = true;
		job.killIncappedTarget = true;
		return job;
	}
}
public class Condition_HasMentalState_VengefulRage : ThinkNode_Conditional
{
	protected override bool Satisfied(Pawn pawn)
	{
		return pawn.MentalState is MentalState_VengefulRage;
	}
}



