using HarmonyLib;
using RimWorld;
using System.Linq;
using Verse;

namespace ArchotechPlus
{
	[StaticConstructorOnStartup]
	public static class HarmonyInit
	{
		public static Harmony harmonyInstance;
		static HarmonyInit()
		{
			harmonyInstance = new Harmony("ArchotechPlus.Mod");
			harmonyInstance.PatchAll();
		}
	}
	[HarmonyPatch(typeof(HediffSet), "AddDirect")]
	public static class HediffSet_Patch
	{
		public static void Prefix(HediffSet __instance, Hediff hediff, DamageInfo? dinfo = null, DamageWorker.DamageResult damageResult = null)
		{
			if (hediff is Hediff_MissingPart)
			{
				var hediffRegeneration = __instance.pawn.health.hediffSet.hediffs.FirstOrDefault(x => x is Hediff_Regeneration);
				if (hediffRegeneration != null)
                {
					var hediffComp = hediffRegeneration as Hediff_Regeneration;
					var previousHediff = __instance.pawn.health.hediffSet.hediffs.FirstOrDefault(x => x.Part == hediff.Part);
					if (previousHediff is Hediff_Implant implant)
                    {
						hediffComp.RememberImplant(implant, hediff.Part);
                    }
                }
			}
		}
	}

	[HarmonyPatch(typeof(Need_Joy), "FallPerInterval", MethodType.Getter)]
	public static class FallPerInterval_Patch
	{
		public static void Postfix(Need_Joy __instance, Pawn ___pawn, ref float __result)
		{
			var hediff = ___pawn.health?.hediffSet.GetFirstHediffOfDef(HediffDef.Named("ArchotechCortex"));
			if (hediff != null)
            {
				__result *= 0f;
            }
		}
	}
}