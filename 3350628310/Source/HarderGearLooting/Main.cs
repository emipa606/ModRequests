// Decompiled with JetBrains decompiler
// Type: ChoosePawnSettings.Main
// Assembly: ChoosePawnSettings, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C1B08650-EEBD-4A03-AA90-A0087EECE8E0
// Assembly location: D:\SteamLibrary\steamapps\workshop\content\294100\2583236920\1.5\Assemblies\ChoosePawnSettings.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

#nullable disable
namespace HarderGearLooting
{
    [StaticConstructorOnStartup]
    public static class HarderGearLooting
    {
        private static List<FactionDef> factions = GetApplicableFactions();

        private static HediffDef deathAcidifier = DefDatabase<HediffDef>.GetNamed("DeathAcidifier");
        private static HediffDef acidifier = DefDatabase<HediffDef>.GetNamed("Acidifier");

        private static HashSet<FactionDef> biocodingFactions;
        private static HashSet<FactionDef> acidifierFactions;

        static HarderGearLooting()
        {
            SetDefaultFactionToggles();

            if (deathAcidifier == null)
            {
                Log.Error("Could not find ThingDef named DeathAcidifier. HarderGearLooting is disabled");
                return;
            }
            if (acidifier == null)
            {
                Log.Error("Could not find ThingDef named Acidifier. HarderGearLooting is disabled");
                return;
            }

            GenerateFactionSets();

            if (HarderGearLootingSettings.useAcidifier)
            {
                HarderGearLootingMod.harmonyInstance.Patch(
                    original: AccessTools.Method(typeof(Pawn), nameof(Pawn.Strip)),
                    prefix: new HarmonyMethod(typeof(HarderGearLooting), nameof(ActivateAcidifier_Prefix))
                );
            }

            if (HarderGearLootingSettings.expandBiocoding || HarderGearLootingSettings.useAcidifier)
            {

                HarderGearLootingMod.harmonyInstance.Patch(
                    original: AccessTools.Method(typeof(PawnGenerator), nameof(PawnGenerator.GeneratePawn), new Type[] { typeof(PawnGenerationRequest) }),
                    postfix: new HarmonyMethod(typeof(HarderGearLooting), nameof(HarderGearLooting.GeneratePawn_Postfix))
                );
            }


        }

        public static void GenerateFactionSets()
        {
            biocodingFactions = new HashSet<FactionDef>();
            acidifierFactions = new HashSet<FactionDef>();

            foreach (FactionDef factionDef in factions)
            {

                if (HarderGearLootingSettings.expandBiocoding &&
                    HarderGearLootingSettings.biocodingFactions.ContainsKey(factionDef.defName) &&
                    HarderGearLootingSettings.biocodingFactions[factionDef.defName])
                {
                    biocodingFactions.Add(factionDef);
                }

                if (HarderGearLootingSettings.useAcidifier &&
                    HarderGearLootingSettings.acidifierFactions.ContainsKey(factionDef.defName) &&
                    HarderGearLootingSettings.acidifierFactions[factionDef.defName])
                {
                    acidifierFactions.Add(factionDef);
                }
            }
        }


        public static bool IsSpacerApparel(Thing apparel)
        {
            return apparel.def.IsApparel && (apparel.def.techLevel == TechLevel.Spacer || apparel.def.techLevel == TechLevel.Ultra || apparel.def.techLevel == TechLevel.Archotech);
        }


        private static void GeneratePawn_Postfix(Pawn __result)
        {
            if (__result.Faction == null) return;

            bool shouldTryBiocode = biocodingFactions.Contains(__result.Faction.def);
            if (shouldTryBiocode && __result.equipment != null)
            {
                foreach (ThingWithComps thing in __result.equipment.AllEquipmentListForReading)
                {
                    bool shouldCode = false;

                    if (thing.def.IsWeapon)
                    {

                        if (thing.def.techLevel == TechLevel.Industrial && Rand.Chance(HarderGearLootingSettings.biocodeIndustrialWeaponsChance))
                        {

                            shouldCode = true;
                        }
                        bool isSpacer = thing.def.techLevel == TechLevel.Spacer || thing.def.techLevel == TechLevel.Ultra || thing.def.techLevel == TechLevel.Archotech;
                        if (isSpacer && Rand.Chance(HarderGearLootingSettings.biocodeSpacerWeaponsChance))
                        {

                            shouldCode = true;
                        }
                    }
                    if (shouldCode)
                    {
                        Biocode(thing, __result);
                    }
                }
            }

            if (shouldTryBiocode && __result.apparel != null)
            {
                foreach (Apparel thing in __result.apparel.WornApparel)
                {
                    if((thing.def.techLevel == TechLevel.Spacer
                        || thing.def.techLevel == TechLevel.Ultra
                        || thing.def.techLevel == TechLevel.Archotech) 
                        && Rand.Chance(HarderGearLootingSettings.biocodeSpacerArmorChance))
                    {
                        Biocode(thing, __result);
                    }
                }

            }

            if (acidifierFactions.Contains(__result.Faction.def))
            {
                Hediff pawnAcidifier = null;
                while (__result.health.hediffSet.HasHediff(deathAcidifier))
                {
                    pawnAcidifier = __result.health.hediffSet.GetFirstHediffOfDef(deathAcidifier);
                    __result.health.RemoveHediff(pawnAcidifier);
                }

                BodyPartRecord torso;
                if (pawnAcidifier != null)
                {
                    torso = pawnAcidifier.Part;
                }
                else {

                    torso = __result.health.hediffSet.GetBodyPartRecord(BodyPartDefOf.Torso);
                }
                __result.health.AddHediff(acidifier, torso);

                if (HarderGearLootingSettings.acidifierOnDownChance > 0f)
                {
                    __result.health.overrideDeathOnDownedChance = HarderGearLootingSettings.acidifierOnDownChance;
                }
            }
        }

        public static bool ShouldBeBiocodable(ThingDef def)
        {
            bool isSpacer = def.techLevel == TechLevel.Spacer || def.techLevel == TechLevel.Ultra || def.techLevel == TechLevel.Archotech;
            if (def.IsWeapon && (isSpacer || def.techLevel == TechLevel.Industrial)) {
                return true;
            }
            if (isSpacer && (def.IsApparel || ThingCategoryDefOf.Apparel.ContainedInThisOrDescendant(def))) {
                return true;
            }
            return false;
        }

        private static void Biocode(ThingWithComps thing, Pawn pawn)
        {
            CompBiocodable biocode = thing.TryGetComp<CompBiocodable>();
            if (biocode == null)
            {
                Log.Error($"{thing} is not biocodable even after patching.");
                return;
            }
            biocode.CodeFor(pawn);
        }

        public static void ActivateAcidifier_Prefix(Pawn __instance)
        {

            if (__instance.health.hediffSet.HasHediff(HardGearLooting_DefOf.Acidifier))
            {
                DamageInfo dinfo = new DamageInfo(DamageDefOf.AcidBurn, 10000f, 999f);
                __instance.Kill(dinfo);
            }
        }

        private static List<FactionDef> GetApplicableFactions()
        {
            return DefDatabase<FactionDef>.AllDefsListForReading.Where<FactionDef>((Func<FactionDef, bool>)(f =>
                 !(f.isPlayer || f.defName.ToLower().Contains("mechanoid") || f.defName.ToLower().Contains("insect"))
            )).ToList<FactionDef>();
        }

        private static Dictionary<string, bool> GetDefaultBiocodeToggles()
        {
            Dictionary<string, bool> result = new Dictionary<string, bool>();
            foreach (FactionDef factionDef in factions)
            {
                result[factionDef.defName] = true;
            }
            return result;
        }


        private static Dictionary<string, bool> GetDefaultAcidifierToggles()
        {
            Dictionary<string, bool> result = new Dictionary<string, bool>();
            foreach (FactionDef factionDef in factions)
            {
                result[factionDef.defName] = (factionDef.defName == "Empire" ||
                                                        factionDef.defName.Contains("Ancients") ||
                                                        factionDef.defName.Contains("Deserter")); // Default acidifiers for specific factions       
            }
            return result;
        }

        private static void SetDefaultFactionToggles()
        {
            Dictionary<string, bool> defaultBiocodingFactions = GetDefaultBiocodeToggles();
            Dictionary<string, bool> defaultAcidifierFactions = GetDefaultAcidifierToggles();

            if (HarderGearLootingSettings.biocodingFactions.NullOrEmpty())
            {
                HarderGearLootingSettings.biocodingFactions = defaultBiocodingFactions;
            }
            else
            {
                foreach (FactionDef faction in factions)
                {
                    if (!HarderGearLootingSettings.biocodingFactions.ContainsKey(faction.defName))
                    {
                        HarderGearLootingSettings.biocodingFactions[faction.defName] = defaultBiocodingFactions[faction.defName];
                    }
                }
            }

            if (HarderGearLootingSettings.acidifierFactions.NullOrEmpty())
            {
                HarderGearLootingSettings.acidifierFactions = defaultAcidifierFactions;
            }
            else
            {
                foreach (FactionDef faction in factions)
                {
                    if (!HarderGearLootingSettings.acidifierFactions.ContainsKey(faction.defName))
                    {
                        HarderGearLootingSettings.acidifierFactions[faction.defName] = defaultAcidifierFactions[faction.defName];
                    }
                }
            }
        }
    }
}
