using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace ResumableJobProgress
{
	class HarmonyPatches
	{
		[HarmonyPatch(typeof(Pawn_JobTracker), "CleanupCurrentJob")]
		public class CleanupCurrentJob_Patch
		{
			[HarmonyPrefix]
			static bool Prefix(Pawn_JobTracker __instance, JobCondition condition)
			{
				var pawn = __instance.curDriver?.pawn;
				if (pawn is null || pawn.Faction != Faction.OfPlayer || pawn.NonHumanlikeOrWildMan())
				{
					return true;
				}
				/*
				if (__instance.job is null)
				{
					return true;
				}
				*/
				if (condition == JobCondition.InterruptOptional || condition == JobCondition.InterruptForced)
				{
					var job = __instance.curJob;
					if (job?.def is JobDef jobDef)
					{
						var driver = __instance.curDriver;
						if (driver is JobDriver_DoBill driverDoBill)
						{
							RecipeDef recipe = Utility.KeyFromRecipe(job.RecipeDef); // 石材の切り出しは全て同一の製法として扱う
							if (recipe is not null && !recipe.UsesUnfinishedThing)
							{
								var workLeft = AccessTools.FieldRefAccess<JobDriver_DoBill, float>(driverDoBill, "workLeft");
								if (workLeft > 0)
								{
									if (recipe.allowMixingIngredients)
									{
										if (job.targetA.Thing is Building_WorkTable workTable)
										{
											var ingredients = (from x in driver.job.placedThings select x.thing.def).ToList();
//											ingredients.Add(job.targetB.Thing.def);
											ResumeDictionary.Instance.SetResumeBillWorkLeft(recipe, workTable, ingredients, workLeft);
										}
									}
									else
									{
										float efficiency = (recipe.efficiencyStat is null) ? 1f : pawn.GetStatValue(recipe.efficiencyStat);
										if (recipe.workTableEfficiencyStat is not null)
										{
											if (job.targetA.Thing is Building_WorkTable workTable)
											{
												efficiency *= workTable.GetStatValue(recipe.workTableEfficiencyStat);
											}
										}
										var baseWorkAmount = recipe.WorkAmountTotal(null);
										ResumeDictionary.Instance.SetResumeWorkLeft(job.targetB.Thing, recipe, workLeft, efficiency, baseWorkAmount);
									}
									// 中断した時点でスキル経験を精算
									if (recipe.workSkill is not null)
									{
										float xp = driverDoBill.ticksSpentDoingRecipeWork * 0.1f * recipe.workSkillLearnFactor;
										pawn.skills.GetSkill(recipe.workSkill).Learn(xp);
									}
								}
							}
						}
						else if (driver is JobDriver_SmoothFloor || driver is JobDriver_SmoothWall)
						{
							var cell = job.targetA.Cell;
//							var workLeft = AccessTools.FieldRefAccess<JobDriver_SmoothWall, float>(driver_FloorAndWall, "workLeft");
							var workLeft = Traverse.Create(driver).Field<float>("workLeft").Value;
							if (workLeft > 0)
							{
								ResumeDictionary.Instance.SetResumeWorkLeft(pawn.Map, cell, jobDef, workLeft);
							}
						}
//						else if (driver is JobDriver_PlantCut || driver is JobDriver_PlantCut_Designated || __instance is IsDisabledPlantHarvest || __instance is IsDisabledPlantHarvest_Designated)
						else if (driver is JobDriver_PlantWork driverPlantWork)
						{
							if (Settings.IsDisabledHarvest)
							{
								if (driver is JobDriver_PlantHarvest || driver is JobDriver_PlantHarvest_Designated)
								{
									return true;
								}
							}
							var plant = job.targetA.Thing;
//							ref var workDone = ref AccessTools.FieldRefAccess<JobDriver_PlantWork, float>(driverPlantWork, "workDone"); ;
							if (driverPlantWork.uninstallWorkLeft > 0)	// Use uninstallWorkLeft instead of workLeft, because uninstallWorkLeft is public.
							{
								StatDef statDef = plant.def.plant.harvestedThingDef.IsDrug ? StatDefOf.DrugHarvestYield : StatDefOf.PlantHarvestYield;
								float efficiency = pawn.GetStatValue(statDef);
								ResumeDictionary.Instance.SetResumeWorkDone(plant, jobDef, Mathf.Min(driverPlantWork.uninstallWorkLeft, plant.def.plant.harvestWork), efficiency);	// Use uninstallWorkLeft instead of workLeft, because uninstallWorkLeft is public.
							}
						}
						else if (driver is JobDriver_PlantSow)
						{
							if (job.targetA.Thing is Plant plant)
							{
								var sowWorkDone = Traverse.Create(driver).Field<float>("sowWorkDone").Value;
								if (sowWorkDone > 0)
								{
									ResumeDictionary.Instance.SetResumeSowWorkDone(plant, sowWorkDone);
								}
							}
						}
						else if (driver is JobDriver_Replant)
						{
							if (pawn.jobs.curJob.targetB.Thing is Blueprint blueprint)
							{
								var ticksLeft = driver.ticksLeftThisToil;
								if (ticksLeft > 0)
								{
									ResumeDictionary.Instance.SetResumeWorkLeft(blueprint, jobDef, ticksLeft);
								}
							}
						}
						else if (driver is JobDriver_Deconstruct && !OtherModCompatibility.IsSmartDecon() && !Settings.IsDisabledDeconstruct
							|| driver is JobDriver_Uninstall && !OtherModCompatibility.IsSmartDecon() && !Settings.IsDisabledUninstall
							|| driver is JobDriver_ExtractTree)
						{
							var thing = job.targetA.Thing;
//							var workLeft = AccessTools.FieldRefAccess<float>(driver.GetType(), "workLeft")(driver);
							var workLeft = Traverse.Create(driver).Field<float>("workLeft").Value;
							if (workLeft > 0)
							{
								ResumeDictionary.Instance.SetResumeWorkLeft(thing, jobDef, workLeft);
							}
						}
					}
				}
				return true;
			}
		}

		[HarmonyPatch(typeof(Bill), nameof(Bill.Notify_BillWorkStarted))]
		public class Notify_BillWorkStarted_Patch
		{
			[HarmonyPostfix]
			static void Postfix(Pawn billDoer, RecipeDef ___recipe)
			{
				if (billDoer.Faction != Faction.OfPlayer || billDoer.NonHumanlikeOrWildMan())
				{
					return;
				}
				RecipeDef recipe = Utility.KeyFromRecipe(___recipe); // 石材の切り出しは全て同一の製法として扱う
				if (recipe is not null && !recipe.UsesUnfinishedThing)
				{
					var driver = billDoer.jobs.curDriver;
					float workLeft;
					if (recipe.allowMixingIngredients)
					{
						if (driver.job.targetA.Thing is Building_WorkTable workTable)
						{
							var ingredients = (from x in driver.job.placedThings select x.thing.def).ToList();
//							ingredients.Add(driver.job.targetB.Thing.def);
							if (ResumeDictionary.Instance.PopResumeBillWorkLeft(recipe, workTable, ingredients, out workLeft))
							{
								Traverse.Create(driver).Field("workLeft").SetValue(workLeft);
							}
						}
					}
					else
					{
						if (ResumeDictionary.Instance.PopResumeWorkLeft(driver.job.targetB.Thing, recipe, out workLeft, out _))
						{
							Traverse.Create(driver).Field("workLeft").SetValue(workLeft);
						}
					}
				}
			}
		}

/*
		[HarmonyPatch(typeof(Toils_Recipe), nameof(Toils_Recipe.FinishRecipeAndStartStoringProduct))]
		public class FinishRecipeAndStartStoringProduct_Patch
		{
			[HarmonyPostfix]
			static void Postfix(Toil __result)
			{
				Action clearResumeWorkData = () =>
				{
					var pawn = __result.actor;
					var recipe = pawn.CurJob.RecipeDef;
					if (recipe.allowMixingIngredients)
					{
						if (pawn.CurJob.targetA.Thing is Building_WorkTable workTable)
						{
							var ingredients = (from x in pawn.CurJob.placedThings select x.thing.def).ToList();
							ingredients.Add(pawn.CurJob.targetB.Thing.def);
							_ = ResumeDictionary.Instance.PopResumeBillWorkLeft(recipe, workTable, ingredients, out _);
						}
					}
					else
					{
						_ = ResumeDictionary.Instance.PopResumeWorkLeft(pawn.CurJob.targetB.Thing, recipe, out _, out _);
					}
				};
				__result.initAction = Delegate.Combine(clearResumeWorkData, __result.initAction) as Action;
			}
		}
*/

/*
		[HarmonyPatch(typeof(JobDriver_AffectFloor), "MakeNewToils")]
		public class MakeNewToils_Patch
		{
			[HarmonyTranspiler]
			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				foreach (var instruction in instructions)
				{
					if (instruction.opcode == OpCodes.Stfld)
					{
						Log.Message("MakeNewToils.GetType(): " + instruction.operand.GetType().FullName);
						Log.Message("MakeNewToils.ToString(): " + instruction.operand.ToString());
					}
					yield return instruction;
				}
			}

			[HarmonyPostfix]
			static void Postfix(JobDriver_AffectFloor __instance, IEnumerable<Toil> __result)
			{
				__result.Last().initAction = delegate
				{
					ref var workLeft = ref AccessTools.FieldRefAccess<JobDriver_AffectFloor, float>(__instance, "workLeft");
//					var baseWorkAmount = AccessTools.PropertyGetter(typeof(JobDriver_AffectFloor), "BaseWorkAmount");
					var baseWorkAmount = (int)AccessTools.Field(typeof(JobDriver_AffectFloor), "BaseWorkAmount").GetValue(__instance);
					var resumedWorkLeft = ResumeDictionary.Instance.GetCellWorkLeft(__instance.pawn.Map, __instance.job.targetA.Cell, __instance.job.def);
					workLeft = resumedWorkLeft < 0 ? baseWorkAmount : resumedWorkLeft;
				};
			}
		}
*/

		[HarmonyPatch(typeof(JobDriver), "SetupToils")]
		public class SetUpToils_Patch
		{
			[HarmonyPostfix]
//			static void Postfix(JobDriver __instance, ref List<Toil> ___toils, Pawn ___pawn, Job ___job)
			static void Postfix(JobDriver __instance, ref List<Toil> ___toils)
			{
				if (__instance.pawn.Faction != Faction.OfPlayer || __instance.pawn.NonHumanlikeOrWildMan())
				{
					return;
				}
				if (__instance is JobDriver_ExtractTree)
				{
					___toils[1].initAction = () =>
					{
						var traverse = Traverse.Create(__instance);
						var totalNeededWork = traverse.Property<float>("TotalNeededWork").Value;
						Job job = __instance.job;
						if (!ResumeDictionary.Instance.PopResumeWorkLeft(job.targetA.Thing, job.def, out float workLeft, out _))
						{
							workLeft = totalNeededWork;
						}
						traverse.Field("totalNeededWork").SetValue(totalNeededWork);
						traverse.Field("workLeft").SetValue(workLeft);
					};
				}
				else if (__instance is JobDriver_RemoveBuilding)
				{
					if (__instance is JobDriver_Deconstruct && (OtherModCompatibility.IsSmartDecon() || Settings.IsDisabledDeconstruct))
					{
						return;
					}
					if (__instance is JobDriver_Uninstall && (OtherModCompatibility.IsSmartDecon() || Settings.IsDisabledUninstall))
					{
						return;
					}
					___toils[1].initAction = () =>
					{
						var traverse = Traverse.Create(__instance);
						var totalNeededWork = traverse.Property<float>("TotalNeededWork").Value;
						Job job = __instance.job;
						if (!ResumeDictionary.Instance.PopResumeWorkLeft(job.targetA.Thing, job.def, out float workLeft, out _))
						{
							workLeft = totalNeededWork;
						}
						traverse.Field("totalNeededWork").SetValue(totalNeededWork);
						traverse.Field("workLeft").SetValue(workLeft);
					};
				}
//				else if (__instance is JobDriver_PlantCut || __instance is JobDriver_PlantCut_Designated || __instance is IsDisabledPlantHarvest || __instance is IsDisabledPlantHarvest_Designated)
				else if (__instance is JobDriver_PlantWork instance)
				{
					if (__instance is JobDriver_PlantHarvest || __instance is JobDriver_PlantHarvest_Designated)
					{
						if (Settings.IsDisabledHarvest)
						{
							return;
						}
					}
					___toils[5].initAction = () =>
					{
						Job job = __instance.job;
						Plant plant = (Plant)job.targetA.Thing;
//						ref var workDone = ref AccessTools.FieldRefAccess<JobDriver_PlantWork, float>(instance, "workDone");
//						workDone = ResumeDictionary.Instance.GetResumeWorkDone(plant, job.def, out _);
						if (!ResumeDictionary.Instance.GetResumeWorkDone(plant, job.def, out __instance.uninstallWorkLeft, out _))	// Use uninstallWorkLeft instead of workLeft, because uninstallWorkLeft is public.
						{
							__instance.uninstallWorkLeft = 0f;
						}
					};
//					float workDone = AccessTools.FieldRefAccess<JobDriver_PlantWork, float>(instance, "workDone");
					___toils[5].WithProgressBar(TargetIndex.A, () => __instance.uninstallWorkLeft / __instance.job.targetA.Thing.def.plant.harvestWork, interpolateBetweenActorAndTarget: true);	// Use uninstallWorkLeft instead of workLeft, because uninstallWorkLeft is public.
					float xpPerTick = AccessTools.FieldRefAccess<JobDriver_PlantWork, float>(instance, "xpPerTick");
					___toils[5].tickAction = () =>
					{
						Pawn actor = __instance.GetActor();
						actor.skills?.Learn(SkillDefOf.Plants, xpPerTick);
						Job job = __instance.job;
						Plant plant = (Plant)job.targetA.Thing;

						var property = plant.def.plant;
//						ref float workDone = ref AccessTools.FieldRefAccess<JobDriver_PlantWork, float>(instance, "workDone");

						__instance.uninstallWorkLeft += JobDriver_PlantWork.WorkDonePerTick(actor, plant);	// Use uninstallWorkLeft instead of workLeft, because uninstallWorkLeft is public.
						if (__instance.uninstallWorkLeft >= property.harvestWork)
						{
							if (property.harvestedThingDef is not null)
							{
								StatDef statDef = property.harvestedThingDef.IsDrug ? StatDefOf.DrugHarvestYield : StatDefOf.PlantHarvestYield;
								float efficiency = actor.GetStatValue(statDef);

								ResumeDictionary.Instance.SetResumeWorkDone(plant, job.def, Mathf.Min(__instance.uninstallWorkLeft, property.harvestWork), efficiency);	// Use uninstallWorkLeft instead of workLeft, because uninstallWorkLeft is public.
								_ = ResumeDictionary.Instance.PopResumeWorkDone(plant, job.def, out _, out efficiency, property.harvestWork);

								if (actor.RaceProps.Humanlike && property.harvestFailable && !plant.Blighted && Rand.Value > efficiency)
								{
									MoteMaker.ThrowText((actor.DrawPos + plant.DrawPos) / 2f, plant.Map, "TextMote_HarvestFailed".Translate(), 3.65f);
								}
								else
								{
									int yield = plant.YieldNow();
									if (efficiency > 1f)
									{
										yield = GenMath.RoundRandom(yield * efficiency);
									}
									if (yield > 0)
									{
										Thing crop = ThingMaker.MakeThing(property.harvestedThingDef);
										crop.stackCount = yield;
										if (actor.Faction != Faction.OfPlayer)
										{
											crop.SetForbidden(value: true);
										}
										Find.QuestManager.Notify_PlantHarvested(actor, crop);
										GenPlace.TryPlaceThing(crop, actor.Position, plant.Map, ThingPlaceMode.Near);
										actor.records.Increment(RecordDefOf.PlantsHarvested);
									}
								}
							}
							property.soundHarvestFinish.PlayOneShot(actor);
//							var PlantDestructionMode = AccessTools.Field(typeof(JobDriver_PlantWork), "PlantDestructionMode");
//							plant.PlantCollected(actor, (PlantDestructionMode)PlantDestructionMode.GetValue(__instance));
//							var plantDestructionMode = Traverse.Create(__instance).Field<PlantDestructionMode>("PlantDestructionMode").Value;
//							plant.PlantCollected(actor, plantDestructionMode);
							// bad solution
							if (__instance is JobDriver_PlantCut)
							{
								plant.PlantCollected(actor, PlantDestructionMode.Cut);
							}
							else if (__instance is JobDriver_PlantHarvest)
							{
								plant.PlantCollected(actor, PlantDestructionMode.Chop);
							}
							else
							{
								plant.PlantCollected(actor, PlantDestructionMode.Smash);
							}
//							plant.PlantCollected(actor, PlantDestructionMode.Smash);
							__instance.uninstallWorkLeft = 0f;	// Use uninstallWorkLeft instead of workLeft, because uninstallWorkLeft is public.
							__instance.ReadyForNextToil();
						}
					};
				}
				else if (__instance is JobDriver_PlantSow)
				{
					___toils[1].initAction = Delegate.Combine(___toils[1].initAction, new Action(() =>
					{
						Plant plant = (Plant)__instance.job.targetA.Thing;
						if (ResumeDictionary.Instance.PopResumeSowWorkDone(plant, out float sowWorkDone))
						{
							var traverse = Traverse.Create(__instance).Field("sowWorkDone").SetValue(sowWorkDone);
						}
						ResumeDictionary.Instance.ClearOtherSowPlants(plant);
					})) as Action;
				}
				else if (__instance is JobDriver_Replant)
				{
					___toils[7].initAction = Delegate.Combine(___toils[7].initAction, new Action(() =>
					{
						var blueprint = __instance.job.targetB.Thing as Blueprint;
//						ref var workDone = ref AccessTools.FieldRefAccess<float>(__instance.GetType(), "workDone")(__instance);
//						workDone = ResumeDictionary.Instance.GetResumeWorkDone(plant, job.def, out _);
						if (ResumeDictionary.Instance.PopResumeWorkLeft(blueprint, __instance.job.def, out float tickLeft, out _))
						{
							__instance.ticksLeftThisToil = Mathf.CeilToInt(tickLeft);
						}
					})) as Action;
				}
				else if (__instance is JobDriver_SmoothFloor || __instance is JobDriver_SmoothWall)
				{
					var traverse = Traverse.Create(__instance);
					var baseWorkAmount = traverse.Property<int>("BaseWorkAmount").Value;
					___toils.Last().initAction = () =>
					{
						Pawn actor = __instance.GetActor();
						Job job = __instance.job;
						if (!ResumeDictionary.Instance.PopResumeWorkLeft(actor.Map, job.targetA.Cell, job.def, out float workLeft, out _))
						{
							workLeft = baseWorkAmount;
						}
						traverse.Field("workLeft").SetValue(workLeft);
					};
				}
			}
		}

		[HarmonyPatch(typeof(Root_Play), nameof(Root_Play.Start))]
		public class Start_Patch
		{
			[HarmonyPrefix]
			static bool Prefix()
			{
				ResumeDictionary.Instance.ClearTempData();
				return true;
			}
		}

		[HarmonyPatch(typeof(CameraDriver), nameof(CameraDriver.Expose))]
		public class Expose_Patch
		{
			[HarmonyPrefix]
			static bool Pretfix()
			{
				ResumeDictionary.Instance.ExposeData();
				return true;
			}
		}

		[HarmonyPatch(typeof(Game), nameof(Game.FinalizeInit))]
		public class FinalizeInit_Patch
		{
			[HarmonyPrefix]
			static bool Prefix()
			{
				ResumeDictionary.Instance.ResolveCompressedThings();
				return true;
			}
		}
	}
}
