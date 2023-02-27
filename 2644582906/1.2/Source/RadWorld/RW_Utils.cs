using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace RadWorld
{
	[StaticConstructorOnStartup]
	public static class RW_Utils
	{
		public static Dictionary<HediffDef, BodyPartDef> hediffsPerBodyParts = new Dictionary<HediffDef, BodyPartDef>
		{
			{RW_DefOf.RW_RadiationResistanceHediff, BodyPartDefOf.Body},
			{RW_DefOf.RW_ExtraArm, BodyPartDefOf.Torso},
			{RW_DefOf.RW_ExtraLeg, BodyPartDefOf.Torso},
			{RW_DefOf.RW_DeformedEyes, BodyPartDefOf.Eye},
			{RW_DefOf.RW_DeformedLung, DefDatabase<BodyPartDef>.GetNamed("Lung")},
			{RW_DefOf.RW_DeadNerves, BodyPartDefOf.Head},
			{RW_DefOf.RW_EnlargedStomach, BodyPartDefOf.Stomach},
			{RW_DefOf.RW_ReducedStomach, BodyPartDefOf.Stomach},
			{RW_DefOf.RW_IrradiatedBlood, BodyPartDefOf.Body},
			{RW_DefOf.RW_IrradiatedBrain, BodyPartDefOf.Brain},
			{RW_DefOf.RW_EnlargedEars, DefDatabase<BodyPartDef>.GetNamed("Ear")},
			{RW_DefOf.RW_DeformedEars, DefDatabase<BodyPartDef>.GetNamed("Ear")},
			{RW_DefOf.RW_DeformedMouth, BodyPartDefOf.Jaw},
			{RW_DefOf.RW_RadiationBurnScar, null},
			{RW_DefOf.RW_EnlargedMuscles, BodyPartDefOf.Body},
			{RW_DefOf.RW_WeakenedMuscles, BodyPartDefOf.Body},
			{RW_DefOf.RW_ScalySkin, BodyPartDefOf.Body},
		};

		public static Dictionary<HediffDef, BodyPartRecord> GetFreeHediffCandidatesFor(Pawn pawn, Dictionary<HediffDef, BodyPartDef> hediffsPerBodyParts)
		{
			Dictionary<HediffDef, BodyPartRecord> hediffsWithBodyPartRecords = new Dictionary<HediffDef, BodyPartRecord>();
			foreach (var hediffData in hediffsPerBodyParts)
			{
				var hediffs = pawn.health.hediffSet.hediffs.Where(x => x.def == hediffData.Key);
				if (hediffData.Value != null)
				{
					var parts = pawn.RaceProps.body.AllParts.Where(x => x.def == hediffData.Value).ToList();
					var otherHediffs = hediffsPerBodyParts.Where(x => x.Value == hediffData.Value && x.Key != hediffData.Key).Select(x => x.Key).ToList();

					foreach (var hediff in hediffs)
					{
						if (parts.Contains(hediff.Part))
						{
							parts.Remove(hediff.Part);
						}
					}

					foreach (var otherHediff in otherHediffs)
					{
						var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(otherHediff);
						if (hediff != null && parts.Contains(hediff.Part))
						{
							parts.Remove(hediff.Part);
						}
					}

					if (parts.Any())
					{
						hediffsWithBodyPartRecords[hediffData.Key] = parts.RandomElement();
					}
				}
				else
				{
					hediffsWithBodyPartRecords[hediffData.Key] = null;
				}
			}
			return hediffsWithBodyPartRecords;
		}

		public static string ModPackageId = "Declinedkilr.RadWorld";
		static RW_Utils()
        {
			foreach (var biomeDef in DefDatabase<BiomeDef>.AllDefs)
            {
				var options = biomeDef.GetModExtension<BiomeOptions>();
				if (options is null)
                {
					if (biomeDef.modExtensions is null)
                    {
						biomeDef.modExtensions = new List<DefModExtension>();
                    }
					biomeDef.modExtensions.Add(new BiomeOptions()
					{
						nuclearFalloutModifier = 1f
					});
				}
				var list = Traverse.Create(biomeDef).Field("allowedPackAnimals").GetValue<List<ThingDef>>();
				list.Add(RW_DefOf.RW_IrradiatedBear);
				Traverse.Create(biomeDef).Field("allowedPackAnimals").SetValue(list);
            }

			foreach (var thingDef in DefDatabase<ThingDef>.AllDefs.Where(x => x.race?.Humanlike ?? false))
            {
				thingDef.comps.Add(new CompProperties { compClass = typeof(CompMutation) });
            }

			foreach (var factionDef in DefDatabase<FactionDef>.AllDefs.Where(x => x.pawnGroupMakers != null))
			{
				foreach (var pawnGroupMaker in factionDef.pawnGroupMakers)
                {
					if (pawnGroupMaker.kindDef == PawnGroupKindDefOf.Trader && pawnGroupMaker.carriers != null)
                    {
						pawnGroupMaker.carriers.Add(new PawnGenOption()
						{
							kind = PawnKindDef.Named("RW_IrradiatedBear"),
							selectionWeight = 6
						});
					}
                }
			}
		}

		public static float GetNuclearModifier(this BiomeDef biomeDef)
        {
			var options = biomeDef.GetModExtension<BiomeOptions>();
			if (options != null)
			{
				return options.nuclearFalloutModifier;
			}
			return 0;
		}

		public static float GetRadiationImpactMultiplier(this Pawn pawn)
		{
			if (pawn.Map != null)
            {
				if (GenRadial.RadialDistinctThingsAround(pawn.Position, pawn.Map, 6.9f, true).Any(x => x.def == RW_DefOf.RW_RadiationCollector && x.TryGetComp<CompPowerTrader>().PowerOn))
                {
					return 0f;
                }
				if (GenRadial.RadialDistinctThingsAround(pawn.Position, pawn.Map, 14.9f, true).Any(x => x.def == RW_DefOf.RW_NanotechPurifier && x.TryGetComp<CompPowerTrader>().PowerOn))
				{
					return 0f;
				}
			}
			if (RadWorldMod.settings.disableCustomRadiationSystem)
            {
				return pawn.GetStatValue(StatDefOf.ToxicSensitivity);
			}
			else
            {
				var resistance = 1f - pawn.GetStatValue(RW_DefOf.RW_RadiationResistance);
				return Mathf.Clamp01(resistance);
			}
		}
		public static bool IsCavernBiome(this BiomeDef biomeDef)
        {
			return biomeDef == RW_DefOf.RW_CollapsedCavern || biomeDef == RW_DefOf.RW_LushCavern || biomeDef == RW_DefOf.RW_SickCavern 
				|| biomeDef == RW_DefOf.RW_InfestedCavern || biomeDef == RW_DefOf.RW_BarrenCavern || biomeDef == RW_DefOf.RW_SurfaceCavern || biomeDef == RW_DefOf.RW_Cavern;
		}

		public static bool IsMutant(this Pawn pawn)
        {
			return pawn.kindDef?.GetModExtension<PawnGenOptions>()?.isMutant ?? false;
        }
	}
}
