using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace DireWolves
{
	[StaticConstructorOnStartup]
	internal static class HarmonyContainer
	{
		public static Harmony harmony;
		static HarmonyContainer()
		{
			harmony = new Harmony("DireWolves.Mod");
			harmony.PatchAll();
		}
	}
	[HarmonyPatch(typeof(Pawn), "HealthScale", MethodType.Getter)]
	public static class HealthScale_Patch
	{
		public static void Postfix(Pawn __instance, ref float __result)
		{
			if (__instance is DireWolf direWolf)
			{
				__result *= direWolf.GetHealthPackBonus();
			}
		}
	}

	[HarmonyPatch(typeof(VerbProperties), "AdjustedMeleeDamageAmount", new Type[] {typeof(Tool), typeof(Pawn), typeof(Thing), typeof(HediffComp_VerbGiver)})]
	public static class PatchAdjustedBaseMeleeDamageAmount
	{
		private static void Postfix(ref float __result, Tool tool, Pawn attacker, Thing equipment, HediffComp_VerbGiver hediffCompSource)
		{
			if (attacker is DireWolf direWolf)
			{
				__result *= direWolf.GetAttackPackBonus();
			}
		}
	}

	[HarmonyPatch(typeof(VerbProperties), "AdjustedMeleeDamageAmount", new Type[] { typeof(Tool), typeof(Pawn), typeof(ThingDef), typeof(ThingDef), typeof(HediffComp_VerbGiver) })]
	public static class AdjustedBaseMeleeDamageAmount_NewTmp
	{
		private static void Postfix(ref float __result, Tool tool, Pawn attacker, ThingDef equipment, ThingDef equipmentStuff, HediffComp_VerbGiver hediffCompSource)
		{
			if (attacker is DireWolf direWolf)
			{
				__result *= direWolf.GetAttackPackBonus();
			}
		}
	}

	[HarmonyPatch(typeof(JobGiver_Manhunter), "TryGiveJob")]
	internal static class JobGiver_Manhunter_Patch
	{
		private static void Postfix(Job __result, Pawn pawn)
		{
			if (__result != null && pawn is DireWolf direWolf)
			{
				if (__result.def == JobDefOf.AttackMelee && __result.targetA.Thing is Pawn victim && victim.Position.DistanceTo(pawn.Position) <= 25 && direWolf.CanHowl())
				{
					direWolf.DoHowl();
				}
			}
		}
	}

	
	[HarmonyPatch(typeof(JobGiver_ReactToCloseMeleeThreat), "TryGiveJob")]
	internal static class JobGiver_ReactToCloseMeleeThreat_Patch
	{
		private static void Postfix(Job __result, Pawn pawn)
		{
			if (__result != null && pawn is DireWolf direWolf)
			{
				if (__result.def == JobDefOf.AttackMelee && __result.targetA.Thing is Pawn victim && victim.Position.DistanceTo(pawn.Position) <= 25 && direWolf.CanHowl())
				{
					direWolf.DoHowl();
				}
			}
		}
	}

	[HarmonyPatch(typeof(JobGiver_AIFightEnemy), "TryGiveJob")]
	internal static class JobGiver_AIFightEnemy_Patch
	{
		private static void Postfix(Job __result, Pawn pawn)
		{
			if (__result != null && pawn is DireWolf direWolf)
			{
				if (__result.def == JobDefOf.AttackMelee && __result.targetA.Thing is Pawn victim && victim.Position.DistanceTo(pawn.Position) <= 25 && direWolf.CanHowl())
				{
					direWolf.DoHowl();
				}
			}
		}
	}

	[HarmonyPatch(typeof(FoodUtility), "IsAcceptablePreyFor")]
	internal static class IsAcceptablePreyFor_Patch
	{
		[HarmonyPriority(Priority.Last)]
		private static void Postfix(ref bool __result, Pawn predator, Pawn prey)
		{
			if (__result && predator is DireWolf wolf)
			{
				if (prey is DireWolf)
					__result = false;
				else if (prey.RaceProps.Humanlike || prey.Faction != null)
					__result = false;
            }
		}
	}
}
