using System;
using System.Collections.Generic;
using Crystosentient.Dictionary;
using RimWorld;
using Verse;
using Verse.AI;

namespace Crystosentient
{
	// Token: 0x02000010 RID: 16
	public class JobGiver_PickUpOpportunisticWeapon : ThinkNode_JobGiver
	{
		// Token: 0x1700000E RID: 14
		// (get) Token: 0x0600005F RID: 95 RVA: 0x000041A0 File Offset: 0x000023A0
		private float MinMeleeWeaponDPSThreshold
		{
			get
			{
				List<Tool> tools = DefOfRace.GDCRYST_RACE_AmethystCleavlingSmall.tools;
				float num = 0f;
				for (int i = 0; i < tools.Count; i++)
				{
					bool flag = tools[i].linkedBodyPartsGroup == DefOfBodyPartGroup.GDCRYST_BODYGROUP_AmethystCleavlingSmallWholeBody;
					if (flag)
					{
						num = tools[i].power / tools[i].cooldownTime;
						break;
					}
				}
				return num + 2f;
			}
		}

		// Token: 0x06000060 RID: 96 RVA: 0x0000421C File Offset: 0x0000241C
		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_PickUpOpportunisticWeapon jobGiver_PickUpOpportunisticWeapon = (JobGiver_PickUpOpportunisticWeapon)base.DeepCopy(resolve);
			jobGiver_PickUpOpportunisticWeapon.preferBuildingDestroyers = this.preferBuildingDestroyers;
			jobGiver_PickUpOpportunisticWeapon.pickUpUtilityItems = this.pickUpUtilityItems;
			return jobGiver_PickUpOpportunisticWeapon;
		}

		// Token: 0x06000061 RID: 97 RVA: 0x00004254 File Offset: 0x00002454
		protected override Job TryGiveJob(Pawn pawn)
		{
			bool flag = pawn.equipment == null && pawn.apparel == null;
			Job result;
			if (flag)
			{
				result = null;
			}
			else
			{
				bool flag2 = pawn.RaceProps.Humanlike && pawn.WorkTagIsDisabled(WorkTags.Violent);
				if (flag2)
				{
					result = null;
				}
				else
				{
					bool flag3 = !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation);
					if (flag3)
					{
						result = null;
					}
					else
					{
						bool flag4 = pawn.GetRegion(RegionType.Set_Passable) == null;
						if (flag4)
						{
							result = null;
						}
						else
						{
							bool flag5 = pawn.equipment != null && !this.AlreadySatisfiedWithCurrentWeapon(pawn);
							if (flag5)
							{
								Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Weapon), PathEndMode.OnCell, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false, false, false), 8f, (Thing x) => pawn.CanReserve(x, 1, -1, null, false) && !x.IsBurning() && this.ShouldEquipWeapon(x, pawn), null, 0, 15, false, RegionType.Set_Passable, false);
								bool flag6 = thing != null;
								if (flag6)
								{
									return JobMaker.MakeJob(JobDefOf.Equip, thing);
								}
								bool flag7 = pawn.equipment.Primary != null && !SlaveRebellionUtility.WeaponUsableInRebellion(pawn.equipment.Primary);
								if (flag7)
								{
									return JobMaker.MakeJob(JobDefOf.DropEquipment, pawn.equipment.Primary);
								}
							}
							Pawn_EquipmentTracker equipment = pawn.equipment;
							bool flag8 = equipment == null;
							bool flag9;
							if (flag8)
							{
								flag9 = false;
							}
							else
							{
								ThingWithComps primary = equipment.Primary;
								bool flag10 = primary == null;
								bool? flag11;
								if (flag10)
								{
									flag11 = null;
								}
								else
								{
									ThingDef def = primary.def;
									flag11 = ((def != null) ? new bool?(def.IsRangedWeapon) : null);
								}
								bool? flag12 = flag11;
								bool flag13 = true;
								flag9 = (flag12.GetValueOrDefault() == flag13 & flag12 != null);
							}
							bool flag14 = flag9;
							if (flag14)
							{
								foreach (Apparel apparel in pawn.apparel.WornApparel)
								{
									bool flag15 = apparel.def == ThingDefOf.Apparel_ShieldBelt;
									if (flag15)
									{
										return JobMaker.MakeJob(JobDefOf.RemoveApparel, apparel);
									}
								}
							}
							bool flag16 = this.pickUpUtilityItems && pawn.apparel != null && this.WouldPickupUtilityItem(pawn);
							if (flag16)
							{
								Thing thing2 = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Apparel), PathEndMode.OnCell, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false, false, false), 8f, (Thing x) => pawn.CanReserve(x, 1, -1, null, false) && !x.IsBurning() && this.ShouldEquipUtilityItem(x, pawn), null, 0, 15, false, RegionType.Set_Passable, false);
								bool flag17 = thing2 != null;
								if (flag17)
								{
									return JobMaker.MakeJob(JobDefOf.Wear, thing2);
								}
							}
							result = null;
						}
					}
				}
			}
			return result;
		}

		// Token: 0x06000062 RID: 98 RVA: 0x000045BC File Offset: 0x000027BC
		private bool AlreadySatisfiedWithCurrentWeapon(Pawn pawn)
		{
			ThingWithComps primary = pawn.equipment.Primary;
			bool flag = primary == null;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				bool flag2 = this.preferBuildingDestroyers;
				if (flag2)
				{
					bool flag3 = !pawn.equipment.PrimaryEq.PrimaryVerb.verbProps.ai_IsBuildingDestroyer;
					if (flag3)
					{
						return false;
					}
				}
				else
				{
					bool flag4 = !primary.def.IsRangedWeapon || !SlaveRebellionUtility.WeaponUsableInRebellion(primary);
					if (flag4)
					{
						return false;
					}
				}
				result = true;
			}
			return result;
		}

		// Token: 0x06000063 RID: 99 RVA: 0x00004644 File Offset: 0x00002844
		private bool ShouldEquipWeapon(Thing newWep, Pawn pawn)
		{
			return (!newWep.def.IsRangedWeapon || !pawn.WorkTagIsDisabled(WorkTags.Shooting)) && EquipmentUtility.CanEquip(newWep, pawn) && this.GetWeaponScore(newWep) > this.GetWeaponScore(pawn.equipment.Primary) && SlaveRebellionUtility.WeaponUsableInRebellion(newWep);
		}

		// Token: 0x06000064 RID: 100 RVA: 0x0000469C File Offset: 0x0000289C
		private int GetWeaponScore(Thing wep)
		{
			bool flag = wep == null;
			int result;
			if (flag)
			{
				result = 0;
			}
			else
			{
				bool flag2 = wep.def.IsMeleeWeapon && wep.GetStatValue(StatDefOf.MeleeWeapon_AverageDPS, true) < this.MinMeleeWeaponDPSThreshold;
				if (flag2)
				{
					result = 0;
				}
				else
				{
					bool flag3 = this.preferBuildingDestroyers && wep.TryGetComp<CompEquippable>().PrimaryVerb.verbProps.ai_IsBuildingDestroyer;
					if (flag3)
					{
						result = 3;
					}
					else
					{
						bool isRangedWeapon = wep.def.IsRangedWeapon;
						if (isRangedWeapon)
						{
							result = 2;
						}
						else
						{
							result = 1;
						}
					}
				}
			}
			return result;
		}

		// Token: 0x06000065 RID: 101 RVA: 0x00004728 File Offset: 0x00002928
		private bool WouldPickupUtilityItem(Pawn pawn)
		{
			Pawn_EquipmentTracker equipment = pawn.equipment;
			return ((equipment != null) ? equipment.Primary : null) == null && pawn.apparel.FirstApparelVerb == null;
		}

		// Token: 0x06000066 RID: 102 RVA: 0x00004760 File Offset: 0x00002960
		private bool ShouldEquipUtilityItem(Thing thing, Pawn pawn)
		{
			Apparel apparel;
			return (apparel = (thing as Apparel)) != null && apparel.def.apparel.ai_pickUpOpportunistically && EquipmentUtility.CanEquip(apparel, pawn) && ApparelUtility.HasPartsToWear(pawn, apparel.def) && !pawn.apparel.WouldReplaceLockedApparel(apparel);
		}

		// Token: 0x04000034 RID: 52
		private bool preferBuildingDestroyers;

		// Token: 0x04000035 RID: 53
		private bool pickUpUtilityItems;
	}
}
