using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace MonsterMash
{
    [StaticConstructorOnStartup]
    public class MM_ReflectionData
    {
        public static bool enabled_ADogSaid = ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "A Dog Said... Animal Prosthetics");
        public static bool enabled_AlphaAnimals = ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "Alpha Animals");

        static MM_ReflectionData()
        {
            if (enabled_ADogSaid)
            {
                if (DefDatabase<RecipeDef>.AllDefs.FirstOrDefault(d => d.defName == "InstallBionicAnimalHeart") is RecipeDef heartAnimal)
                {
                    RecipeDef thermalCore = DefDatabase<RecipeDef>.AllDefs.FirstOrDefault(d => d.defName == "MM_InstallThermalCore");
                    List<BodyPartDef> appliedOnFixedBodyParts = thermalCore.appliedOnFixedBodyParts;
                    List<ThingDef> recipeUsers = thermalCore.recipeUsers;

                    foreach (BodyPartDef partDef in heartAnimal.appliedOnFixedBodyParts)
                    {
                        if (!appliedOnFixedBodyParts.Contains(partDef))
                        {
                            appliedOnFixedBodyParts.Add(partDef);
                        }
                    }
                    foreach (ThingDef thingDef in thermalCore.recipeUsers)
                    {
                        if (!recipeUsers.Contains(thingDef))
                        {
                            recipeUsers.Add(thingDef);
                        }
                    }
                    DefDatabase<RecipeDef>.AllDefs.FirstOrDefault(d => d.defName == "MM_InstallThermalCore").appliedOnFixedBodyParts = appliedOnFixedBodyParts;
                    DefDatabase<RecipeDef>.AllDefs.FirstOrDefault(d => d.defName == "MM_InstallThermalCore").recipeUsers = recipeUsers;
                }

                if (DefDatabase<RecipeDef>.AllDefs.FirstOrDefault(d => d.defName == "InstallBionicEyeAnimal") is RecipeDef eyeAnimal)
                {
                    RecipeDef opticalTrageting = DefDatabase<RecipeDef>.AllDefs.FirstOrDefault(d => d.defName == "MM_InstallOpticalTargetingSystem");
                    List<BodyPartDef> appliedOnFixedBodyParts = opticalTrageting.appliedOnFixedBodyParts;
                    List<ThingDef> recipeUsers = opticalTrageting.recipeUsers;

                    foreach (BodyPartDef partDef in eyeAnimal.appliedOnFixedBodyParts)
                    {
                        if (!appliedOnFixedBodyParts.Contains(partDef))
                        {
                            appliedOnFixedBodyParts.Add(partDef);
                        }
                    }
                    foreach (ThingDef thingDef in eyeAnimal.recipeUsers)
                    {
                        if (!recipeUsers.Contains(thingDef))
                        {
                            recipeUsers.Add(thingDef);
                        }
                    }
                    DefDatabase<RecipeDef>.AllDefs.FirstOrDefault(d => d.defName == "MM_InstallOpticalTargetingSystem").appliedOnFixedBodyParts = appliedOnFixedBodyParts;
                    DefDatabase<RecipeDef>.AllDefs.FirstOrDefault(d => d.defName == "MM_InstallOpticalTargetingSystem").recipeUsers = recipeUsers;
                }

                if (DefDatabase<RecipeDef>.AllDefs.FirstOrDefault(d => d.defName == "InstallJawAnimalBionic") is RecipeDef jawAnimal)
                {
                    RecipeDef firingAssembly = DefDatabase<RecipeDef>.AllDefs.FirstOrDefault(d => d.defName == "MM_InstallThermalLanceFiringAssembly");
                    List<BodyPartDef> appliedOnFixedBodyParts = firingAssembly.appliedOnFixedBodyParts;
                    List<ThingDef> recipeUsers = firingAssembly.recipeUsers;

                    foreach (BodyPartDef partDef in jawAnimal.appliedOnFixedBodyParts)
                    {
                        if (!appliedOnFixedBodyParts.Contains(partDef))
                        {
                            appliedOnFixedBodyParts.Add(partDef);
                        }
                    }
                    foreach (ThingDef thingDef in firingAssembly.recipeUsers)
                    {
                        if (!recipeUsers.Contains(thingDef))
                        {
                            recipeUsers.Add(thingDef);
                        }
                    }
                    DefDatabase<RecipeDef>.AllDefs.FirstOrDefault(d => d.defName == "MM_InstallThermalLanceFiringAssembly").appliedOnFixedBodyParts = appliedOnFixedBodyParts;
                    DefDatabase<RecipeDef>.AllDefs.FirstOrDefault(d => d.defName == "MM_InstallThermalLanceFiringAssembly").recipeUsers = recipeUsers;
                }
            }


            if (enabled_ADogSaid && enabled_AlphaAnimals)
            {
                if (DefDatabase<RecipeDef>.AllDefs.FirstOrDefault(d => d.defName == "MM_ADS_InstallSimpleProstheticTentacleAnimal") is RecipeDef tentacleAnimal)
                {
                    List<BodyPartDef> appliedOnFixedBodyParts = tentacleAnimal.appliedOnFixedBodyParts;
                    List<ThingDef> recipeUsers = tentacleAnimal.recipeUsers;

                    List<BodyPartDef> newParts = DefDatabase<BodyPartDef>.AllDefs.Where(d => d.defName == "AA_JellyfishTentacle" || d.defName == "AA_ManTrapTentacle" || d.defName == "AA_SquidTentacle").ToList();
                    List<ThingDef> newUsers = DefDatabase<ThingDef>.AllDefs.Where(d => d.defName == "AA_Aerofleet" || d.defName == "AA_ColossalAerofleet" || d.defName == "AA_GreenGoo" || d.defName == "AA_InfectedAerofleet" || d.defName == "AA_Mantrap" || d.defName == "AA_OcularJelly" || d.defName == "AA_RedGoo" || d.defName == "AA_RedSpore" || d.defName == "AA_SandSquid").ToList();

                    foreach (BodyPartDef partDef in newParts)
                    {
                        if (!appliedOnFixedBodyParts.Contains(partDef))
                        {
                            appliedOnFixedBodyParts.Add(partDef);
                        }
                    }
                    foreach (ThingDef thingDef in newUsers)
                    {
                        if (!recipeUsers.Contains(thingDef))
                        {
                            recipeUsers.Add(thingDef);
                        }
                    }

                    DefDatabase<RecipeDef>.AllDefs.FirstOrDefault(d => d.defName == "MM_ADS_InstallSimpleProstheticTentacleAnimal").appliedOnFixedBodyParts = appliedOnFixedBodyParts;
                    DefDatabase<RecipeDef>.AllDefs.FirstOrDefault(d => d.defName == "MM_ADS_InstallSimpleProstheticTentacleAnimal").recipeUsers = recipeUsers;
                    DefDatabase<RecipeDef>.AllDefs.FirstOrDefault(d => d.defName == "MM_ADS_InstallBionicTentacleAnimal").appliedOnFixedBodyParts = appliedOnFixedBodyParts;
                    DefDatabase<RecipeDef>.AllDefs.FirstOrDefault(d => d.defName == "MM_ADS_InstallBionicTentacleAnimal").recipeUsers = recipeUsers;
                }

                if (DefDatabase<RecipeDef>.AllDefs.FirstOrDefault(d => d.defName == "MM_InstallOpticalTargetingSystem") is RecipeDef eyeAnimal)
                {
                    List<BodyPartDef> appliedOnFixedBodyParts = eyeAnimal.appliedOnFixedBodyParts;
                    List<ThingDef> recipeUsers = eyeAnimal.recipeUsers;

                    List<BodyPartDef> newParts = DefDatabase<BodyPartDef>.AllDefs.Where(d => d.defName == "AA_PlantVibrationSensor" || d.defName == "AA_VibrationReceptor" || d.defName == "AA_EcholocationOrgan").ToList();
                    List<ThingDef> newUsers = DefDatabase<ThingDef>.AllDefs.Where(d => d.defName == "AA_Mantrap" || d.defName == "AA_GreatDevourer" || d.defName == "AA_SandSquid").ToList();

                    foreach (BodyPartDef partDef in newParts)
                    {
                        if (!appliedOnFixedBodyParts.Contains(partDef))
                        {
                            appliedOnFixedBodyParts.Add(partDef);
                        }
                    }
                    foreach (ThingDef thingDef in newUsers)
                    {
                        if (!recipeUsers.Contains(thingDef))
                        {
                            recipeUsers.Add(thingDef);
                        }
                    }

                    DefDatabase<RecipeDef>.AllDefs.FirstOrDefault(d => d.defName == "MM_InstallOpticalTargetingSystem").appliedOnFixedBodyParts = appliedOnFixedBodyParts;
                    DefDatabase<RecipeDef>.AllDefs.FirstOrDefault(d => d.defName == "MM_InstallOpticalTargetingSystem").recipeUsers = recipeUsers;
                }
            }
        }
    }
}
