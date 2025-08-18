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

namespace SaferPasteDispenser
{
	class HarmonyPatches
	{
		internal class TakeMealFromDispenser_Patch
		{
			internal static void Patch(Harmony harmony)
			{
				var original = AccessTools.Method(typeof(Toils_Ingest), nameof(Toils_Ingest.TakeMealFromDispenser));
//				var postfix = new HarmonyMethod(typeof(TakeMealFromDispenser_Patch), Utility.IsReplimat() ? nameof(TakeMealFromDispenser_Patch.PostfixReplimat) : nameof(TakeMealFromDispenser_Patch.Postfix));
				var postfix = new HarmonyMethod(typeof(TakeMealFromDispenser_Patch), "Postfix" + (SaferPasteUtility.IsReplimat() ? "Replimat" : ""));
				harmony.Patch(original, postfix: postfix);
			}

			static void Postfix(Toil __result, TargetIndex ind)
			{
				__result.initAction = delegate
				{
					Pawn actor = __result.actor;
					var dispenser = (Building_NutrientPasteDispenser)actor.jobs.curJob.GetTarget(ind).Thing;
					Thing paste = dispenser.TryDispenseSaferFood(actor);
					SetActorJob(paste, ind, actor);
				};
			}

			static void PostfixReplimat(Toil __result, TargetIndex ind, Pawn eater)
			{
				if (eater.jobs.curJob.GetTarget(ind).Thing.IsReplimat())
				{
					return;
				}
				__result.initAction = delegate
				{
					Pawn actor = __result.actor;
					var dispenser = (Building_NutrientPasteDispenser)actor.jobs.curJob.GetTarget(ind).Thing;
					Thing paste = dispenser.TryDispenseSaferFood(actor);
					SetActorJob(paste, ind, actor);
				};
			}

			private static void SetActorJob(Thing paste, TargetIndex ind, Pawn actor)
			{
				if (paste == null)
				{
					actor.jobs.curDriver.EndJobWith(JobCondition.Incompletable);
				}
				else
				{
					actor.carryTracker.TryStartCarry(paste);
					actor.CurJob.SetTarget(ind, actor.carryTracker.CarriedThing);
				}
			}
		}

		/*
		[HarmonyPatch(typeof(Toils_Ingest), nameof(Toils_Ingest.TakeMealFromDispenser))]
		public class TakeMealFromDispenser_Patch
		{
			[HarmonyPostfix]
			static void Postfix(Toil __result, TargetIndex ind, Pawn eater)
			{
				if (eater.jobs.curJob.GetTarget(ind).Thing.IsReplimat())
				{
					return;
				}
				__result.initAction = delegate
				{
					Pawn actor = __result.actor;
					var dispenser = (Building_NutrientPasteDispenser)actor.jobs.curJob.GetTarget(ind).Thing;
					// NPDTiers patch
//					Thing paste = dispenser.TryDispenseFood(actor);
					Thing paste = dispenser.IsNPDTiers() ? dispenser.TryDispenseFood() : dispenser.TryDispenseSaferFood(actor);
					SetActorJob(paste, ind, actor);
				};
			}

			private static void SetActorJob(Thing paste, TargetIndex ind, Pawn actor)
			{
				if (paste == null)
				{
					actor.jobs.curDriver.EndJobWith(JobCondition.Incompletable);
				}
				else
				{
					actor.carryTracker.TryStartCarry(paste);
					actor.CurJob.SetTarget(ind, actor.carryTracker.CarriedThing);
				}
			}
		}
		*/

		/*
		[HarmonyPatch(typeof(Toils_Ingest), nameof(Toils_Ingest.TakeMealFromDispenser))]
		public class TakeMealFromDispenser_Patch
		{
			[HarmonyTranspiler]
			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				Action action = delegate
				{
					Pawn actor = toil.actor;
					Thing thing = ((Building_NutrientPasteDispenser)actor.jobs.curJob.GetTarget(ind).Thing).TryDispenseFood();
					if (thing == null)
					{
						actor.jobs.curDriver.EndJobWith(JobCondition.Incompletable);
					}
					else
					{
						actor.carryTracker.TryStartCarry(thing);
						actor.CurJob.SetTarget(ind, actor.carryTracker.CarriedThing);
					}
				};

				MethodInfo initAction = null;
				var done = false;
				foreach (var instruction in instructions)
				{
					if (!done && instruction.opcode == OpCodes.Ldftn)
					{
						yield return new CodeInstruction(OpCodes.Ldftn, action);
					}
					else
					{
						yield return instruction;
					}
				}
			}
		}
		*/

		/*
		[HarmonyPatch]
		public class AnonymousMethod_Patch
		{
			[HarmonyTargetMethod]
			static MethodBase TargetMethod()
			{
				var delegates = typeof(Toils_Ingest).GetNestedTypes().Where(x => x.BaseType == typeof(MulticastDelegate));
				return typeof(Delegate).GetMethod(...).MakeGenericMethod(typeof(float));
			}

			[HarmonyTranspiler]
			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				{
					MethodInfo initAction = null;
					foreach (var instruction in instructions)
					{
						if (instruction.opcode == OpCodes.Ldftn)
						{
							initAction = (MethodInfo)instruction.operand;
							Log.Message("class:" + initAction.DeclaringType.ToString() + " / method:" + initAction.ToString());
							break;
						}
					}
					var Info = AccessTools.Method(initAction.DeclaringType, initAction.Name);

					var harmony = new Harmony("denev.SaferPasteDispenser");
					harmony.Patch(original: Info, transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(TakeMealFromDispenser_Patch.Transpiler)));
				}

				{
					MethodInfo TryDispenseFoodInfo = AccessTools.Method(typeof(Building_NutrientPasteDispenser), nameof(Building_NutrientPasteDispenser.TryDispenseFood));
					var done = false;
					foreach (var instruction in instructions)
					{
						if (!done && instruction.Calls(TryDispenseFoodInfo))
						{
							yield return new CodeInstruction(OpCodes.Ldarg_1);
							yield return new CodeInstruction(OpCodes.Call, ((Func<Thing, Pawn, Thing>)ExtensionMethod.TryDispenseFood).Method);
							done = true;
						}
						else
						{
							yield return instruction;
						}
					}
				}
			}
		}
		*/
	}
}
