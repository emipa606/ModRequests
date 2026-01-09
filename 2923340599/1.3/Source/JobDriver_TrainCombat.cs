using HugsLib.Utils;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace KriilMod_CD
{
	public class JobDriver_TrainCombat : JobDriver
	{
		public int jobStartTick = -1;

		public static readonly float trainCombatLearningFactor = 0.15f;

		public ThingWithComps startingEquippedWeapon;
		public ThingWithComps trainingWeapon;
        public ThingWithComps droppedWeapon;

        public Thing Dummy => job.GetTarget(TargetIndex.A).Thing;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref jobStartTick, "jobStartTick", 0);
			Scribe_Values.Look(ref startingEquippedWeapon, "startingEquippedWeapon");
			Scribe_Values.Look(ref trainingWeapon, "trainingWeapon");
		}

		public override string GetReport()
		{
			if (Dummy != null)
			{
				return job.def.reportString.Replace("TargetA", Dummy.LabelShort);
			}
			return base.GetReport();
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.GetTarget(TargetIndex.A), job);
		}

		public bool IsDummyUsable()
		{
			if (IsDummyBreaking()) return false;
			if (Dummy.Destroyed) return false;
			return true;
        }

		public bool IsDummyBreaking()
		{
			return Dummy.HitPoints <= (Dummy.MaxHitPoints * 0.5);
		}

		public bool IsJobPossible()
		{
			if (!IsDummyUsable()) return false;
			if (LearningSaturated()) return false;
			if (HugsLibUtility.HasDesignation(TargetThingA, CombatTrainingDefOf.TrainCombatDesignation)) return true;
			if (pawn.equipment.Primary == null)
            {
				return HugsLibUtility.HasDesignation(TargetThingA, CombatTrainingDefOf.TrainCombatDesignationMeleeOnly);
			}
			if (HugsLibUtility.HasDesignation(TargetThingA, CombatTrainingDefOf.TrainCombatDesignationMeleeOnly)) return pawn.equipment.Primary.def.IsMeleeWeapon;
			if (HugsLibUtility.HasDesignation(TargetThingA, CombatTrainingDefOf.TrainCombatDesignationRangedOnly)) return pawn.equipment.Primary.def.IsRangedWeapon;
			return false;
        }

		public bool IsTimeLimitReached()
        {
			return Find.TickManager.TicksGame > jobStartTick + 5000;
		}

		public bool HasTrainingEnded()
		{

			if (IsTimeLimitReached()) return true;
			if (!IsJobPossible()) return true;
			return false;
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			AddFailCondition(() => pawn.WorkTagIsDisabled(WorkTags.Violent));

			this.FailOnSomeonePhysicallyInteracting(TargetIndex.A);
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			jobStartTick = Find.TickManager.TicksGame;

			startingEquippedWeapon = pawn.equipment.Primary;
			trainingWeapon = null;
			//if (startingEquippedWeapon == null || !startingEquippedWeapon.def.IsWithinCategory(CombatTrainingDefOf.TrainingWeapons))
			//{
			//	trainingWeapon = GetNearestTrainingWeapon(startingEquippedWeapon);
			//	if (trainingWeapon != null && !trainingWeapon.IsForbidden(pawn))
			//	{
			//		if (Map.reservationManager.CanReserve(pawn, trainingWeapon))
			//		{
			//			pawn.Reserve(trainingWeapon, job);
			//			job.SetTarget(TargetIndex.B, trainingWeapon);
			//			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.B);
			//			yield return CreateEquipToil(TargetIndex.B);
			//		}
			//		if (Map.reservationManager.CanReserve(pawn, startingEquippedWeapon))
			//		{
			//			pawn.Reserve(startingEquippedWeapon, job);
			//			job.SetTarget(TargetIndex.C, startingEquippedWeapon);
			//		}
			//	}
			//}
			Toil endOfTraining = Toils_General.Label();
			Toil gotoCastPos = Toils_Combat.GotoCastPosition(TargetIndex.A, TargetIndex.None, closeIfDowned: true, 0.95f).EndOnDespawnedOrNull(TargetIndex.A);
			Toil ifTrainingDoneJumpToReequip = Toils_Jump.JumpIf(endOfTraining, HasTrainingEnded);
			Toil castVerb = Toils_Combat.CastVerb(TargetIndex.A, canHitNonTargetPawns: false);
			castVerb.AddFinishAction(delegate
			{
				LearnAttackSkill();
			});
			Toil trainingRoomImpressivenessMoodBoost = Toils_General.Do(delegate
			{
				TryGainCombatTrainingRoomThought();
			});
			Toil dropTrainingWeapon = Toils_General.Do(delegate
			{
				pawn.equipment.TryDropEquipment(pawn.equipment.Primary, out _, pawn.Position, forbid: false);
			});
			Toil reequipSwappedStartingWeapon = Toils_General.Do(delegate
			{
				pawn.inventory.innerContainer.Remove(startingEquippedWeapon);
				pawn.equipment.AddEquipment(startingEquippedWeapon);
			});
			Toil jobEndedLabel = Toils_General.Label();


			yield return Toils_Combat.TrySetJobToUseAttackVerb(TargetIndex.A);
			yield return gotoCastPos;
			yield return Toils_Jump.JumpIfTargetNotHittable(TargetIndex.A, gotoCastPos);
			yield return trainingRoomImpressivenessMoodBoost;
			yield return ifTrainingDoneJumpToReequip;
			yield return castVerb;
			yield return Toils_Jump.Jump(ifTrainingDoneJumpToReequip);

			yield return endOfTraining;

			if (trainingWeapon != null) yield return dropTrainingWeapon;

			yield return Toils_Jump.JumpIf(reequipSwappedStartingWeapon, () => pawn.inventory.Contains(startingEquippedWeapon));
			yield return Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.C);
			yield return CreateEquipToil(TargetIndex.C);
			yield return Toils_Jump.Jump(jobEndedLabel);

			yield return reequipSwappedStartingWeapon;

			yield return jobEndedLabel;

			
		}

		public float CalculateXp(Verb verb, Pawn pawn)
		{
			return trainCombatLearningFactor * 200f * verb.verbProps.AdjustedFullCycleTime(verb, pawn);
		}

		public void TryGainCombatTrainingRoomThought()
		{
			Room room = pawn.GetRoom();
			if (room != null)
			{
				int scoreStageIndex = RoomStatDefOf.Impressiveness.GetScoreStageIndex(room.GetStat(RoomStatDefOf.Impressiveness));
				if (CombatTrainingDefOf.TrainedInImpressiveCombatTrainingRoom.stages[scoreStageIndex] != null)
				{
					pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtMaker.MakeThought(CombatTrainingDefOf.TrainedInImpressiveCombatTrainingRoom, scoreStageIndex));
				}
			}
		}

		public bool LearningSaturated()
		{
			Verb verbToUse = pawn.jobs.curJob.verbToUse;
			bool result = false;
			SkillRecord skillRecord = (!verbToUse.verbProps.IsMeleeAttack) ? pawn.skills.GetSkill(SkillDefOf.Shooting) : pawn.skills.GetSkill(SkillDefOf.Melee);
			if (skillRecord.LearningSaturatedToday || skillRecord.Level >= 20)
			{
				result = true;
			}
			return result;
		}

		public void LearnAttackSkill()
		{
			Verb verbToUse = pawn.jobs.curJob.verbToUse;
			float xp = CalculateXp(verbToUse, pawn);
			if (verbToUse.verbProps.IsMeleeAttack)
			{
				pawn.skills.Learn(SkillDefOf.Melee, xp);
				CombatTrainingTracker.TrackPawnMeleeSkill(pawn, pawn.skills.GetSkill(SkillDefOf.Melee));
			}
			else
			{
				pawn.skills.Learn(SkillDefOf.Shooting, xp);
				CombatTrainingTracker.TrackPawnShootingSkill(pawn, pawn.skills.GetSkill(SkillDefOf.Shooting));
			}
		}

        //public ThingWithComps GetNearestTrainingWeaponOfType(ThingDef weaponType)
        //{
        //	ThingRequest req = ThingRequest.ForDef(weaponType);
        //	int regionsSeen;
        //	return (ThingWithComps)GenClosest.RegionwiseBFSWorker(base.TargetA.Thing.Position, pawn.Map, req, PathEndMode.OnCell, TraverseParms.For(pawn), (Thing x) => pawn.CanReserve(x), null, 0, 12, 50f, out regionsSeen, RegionType.Set_Passable, ignoreEntirelyForbiddenRegions: true);
        //}

        //public ThingWithComps GetNearestTrainingWeaponMelee()
        //{
        //	return GetNearestTrainingWeaponOfType(CombatTrainingDefOf.MeleeWeapon_TrainingKnife);
        //}

        //public ThingWithComps GetNearestTrainingWeaponRanged()
        //{
        //	ThingWithComps thingWithComps = null;
        //	if (!pawn.Faction.def.techLevel.IsNeolithicOrWorse())
        //	{
        //		ThingDef gun_TrainingBBGun = CombatTrainingDefOf.Gun_TrainingBBGun;
        //		thingWithComps = GetNearestTrainingWeaponOfType(gun_TrainingBBGun);
        //	}
        //	if (thingWithComps == null)
        //	{
        //		ThingDef gun_TrainingBBGun = CombatTrainingDefOf.Bow_TrainingShort;
        //		thingWithComps = GetNearestTrainingWeaponOfType(gun_TrainingBBGun);
        //	}
        //	return thingWithComps;
        //}

        //public ThingWithComps GetNearestTrainingWeapon(Thing currentWeapon)
        //{
        //    ThingWithComps result = null;
        //    if (currentWeapon?.def.IsMeleeWeapon ?? false)
        //    {
        //        result = GetNearestTrainingWeaponMelee();
        //    }
        //    if (currentWeapon != null && !currentWeapon.def.IsMeleeWeapon)
        //    {
        //        result = GetNearestTrainingWeaponRanged();
        //    }
        //    if (currentWeapon == null && !HugsLibUtility.HasDesignation(base.TargetThingA, CombatTrainingDefOf.TrainCombatDesignation))
        //    {
        //        if (HugsLibUtility.HasDesignation(base.TargetThingA, CombatTrainingDefOf.TrainCombatDesignationMeleeOnly))
        //        {
        //            result = GetNearestTrainingWeaponMelee();
        //        }
        //        else if (HugsLibUtility.HasDesignation(base.TargetThingA, CombatTrainingDefOf.TrainCombatDesignationRangedOnly))
        //        {
        //            result = GetNearestTrainingWeaponRanged();
        //        }
        //    }
        //    if (currentWeapon == null && HugsLibUtility.HasDesignation(base.TargetThingA, CombatTrainingDefOf.TrainCombatDesignation))
        //    {
        //        ThingRequest req = ThingRequest.ForGroup(ThingRequestGroup.Weapon);
        //        result = (ThingWithComps)GenClosest.RegionwiseBFSWorker(base.TargetA.Thing.Position, pawn.Map, req, PathEndMode.OnCell, TraverseParms.For(pawn), (Thing x) => CombatTrainingDefOf.TrainingWeapons.DescendantThingDefs.Contains(x.def) && pawn.CanReserve(x), null, 0, 12, 50f, out int _, RegionType.Set_Passable, ignoreEntirelyForbiddenRegions: true);
        //    }
        //    return null;
        //}

        public Toil CreateEquipToil(TargetIndex index)
		{
			LocalTargetInfo equipment = pawn.jobs.curJob.GetTarget(index);
			Toil toil = new Toil
			{
				initAction = delegate
				{
					ThingWithComps weaponPile = (ThingWithComps)(Thing)equipment;
					ThingWithComps weapon;
					if (weaponPile.def.stackLimit > 1 && weaponPile.stackCount > 1)
					{
						weapon = (ThingWithComps)weaponPile.SplitOff(1);
					}
					else
					{
						weapon = weaponPile;
						weapon.DeSpawn();
					}
					if (pawn.equipment.Primary != null)
					{
						//TODO FIX storedPrimaryWeapon = equipped = null
						pawn.equipment.TryTransferEquipmentToContainer(pawn.equipment.Primary, pawn.inventory.innerContainer);
					}
					
					pawn.equipment.AddEquipment(weapon);
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
			toil.FailOnDespawnedNullOrForbidden(index);
			return toil;
		}

	}
}
