using HarmonyLib;
using RimWorld;
using System;
using System.Reflection;
using Verse;

namespace Diabetes
{
	[StaticConstructorOnStartup]
	static class Nutrition_Patch
	{
		static Nutrition_Patch()
		{
			Harmony harmony = new Harmony("energistics.diabetes.nutrition");

			MethodInfo targetMethod = AccessTools.Method(typeof(Thing), nameof(Thing.Ingested));

			HarmonyMethod postfixMethod = new HarmonyMethod(typeof(Nutrition_Patch).GetMethod(nameof(Postfix)));

			harmony.Patch(targetMethod, postfixMethod, null);
		}

		public static void Postfix(Thing __instance, Pawn ingester)
		{
			float sugarValue = __instance.def.statBases.GetStatValueFromList(StatDefOf.Nutrition, 0f) * randomFactor.RandomInRange;

			HealHypoglycaemia(ref sugarValue, ingester.health.hediffSet.GetFirstHediffOfDef(TypeGetter.HediffDef(EHediffDef.Hypoglycaemia)));

			if (!ingester.health.hediffSet.HasHediff(TypeGetter.HediffDef(EHediffDef.Diabetes)))
			{
				return;
			}
			if (sugarValue <= 0f)
			{
				return;
			}

			HediffDef hediffDef = TypeGetter.HediffDef(EHediffDef.Hyperglycaemia);
			Hediff hediff = ingester.health.hediffSet.GetFirstHediffOfDef(hediffDef);
			if (hediff == null)
			{
				hediff = HediffMaker.MakeHediff(hediffDef, ingester);
				ingester.health.AddHediff(hediff);
			}

			hediff.Severity += sugarValue;
		}

		private static void HealHypoglycaemia(ref float sugarValue, Hediff hediff)
		{
			if (hediff == null)
			{
				return;
			}
			float value = hediff.Severity;
			hediff.Severity -= sugarValue;
			sugarValue = Math.Max(sugarValue - value, 0f);
		}

		public static FloatRange randomFactor = new FloatRange(0.10f, 0.35f);
	}
}
