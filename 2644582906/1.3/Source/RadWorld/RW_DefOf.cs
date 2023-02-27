using System;
using FactionBaseGeneration;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Noise;

namespace RadWorld
{
	[DefOf]
	public static class RW_DefOf
	{
		public static BiomeDef RW_CollapsedCavern;
		public static BiomeDef RW_LushCavern;
		public static BiomeDef RW_SickCavern;
		public static BiomeDef RW_InfestedCavern;
		public static BiomeDef RW_BarrenCavern;
		public static BiomeDef RW_SurfaceCavern;
		public static BiomeDef RW_Cavern;

		public static GameConditionDef RW_ToxicFallout;
		public static GameConditionDef RW_UndergroundCondition;

		public static MapGeneratorDef RW_CavernBase_Player;
		public static MapGeneratorDef RW_CavernBase_Faction;
		public static MapGeneratorDef RW_CavernEncounter;
		public static StatDef RW_RadiationResistance;
		public static StatDef RW_RadiationResistanceOffset;
		public static StatDef RW_RadiationResistancePoweredOffset;
		public static ThingDef RW_RadiationCollector;
		public static ThingDef RW_NanotechPurifier;
		public static HediffDef RW_Radiation;

		public static HediffDef RW_RadiationResistanceHediff;
		public static HediffDef RW_ExtraArm;
		public static HediffDef RW_ExtraLeg;
		public static HediffDef RW_DeformedEyes;
		public static HediffDef RW_DeformedLung;
		public static HediffDef RW_DeadNerves;
		public static HediffDef RW_EnlargedStomach;
		public static HediffDef RW_ReducedStomach;
		public static HediffDef RW_IrradiatedBlood;
		public static HediffDef RW_IrradiatedBrain;
		public static HediffDef RW_EnlargedEars;
		public static HediffDef RW_DeformedEars;
		public static HediffDef RW_DeformedMouth;
		public static HediffDef RW_RadiationBurnScar;
		public static HediffDef RW_EnlargedMuscles;
		public static HediffDef RW_WeakenedMuscles;
		public static HediffDef RW_ScalySkin;
		public static ThingDef RW_IrradiatedBear;

		public static FactionDef RW_MutantRough;
		public static TraitDef RW_MutantBlood;
		public static HediffDef RW_MutantBrain;
		public static FactionDef RW_VaultRough;
		public static LocationDef RW_VaultLocation;
		public static LocationDef RW_AbandonedVault;
		public static PawnKindDef RW_Vault_Villager;
		public static FactionDef RW_VaultNatives;
		public static PawnKindDef RW_IrradiatedMegaspider;
		public static ThingDef RW_GasMaskFilter;
	}
}
