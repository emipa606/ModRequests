// Nightvision NightVision Init_Hediffs.cs
// 
// 06 12 2018
// 
// 06 12 2018

using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace NightVision
{
    public partial class Initialiser
    {

        public void FindAllValidHediffs()
        {
            //Essentially we construct two collections: 
            //  the first contains all hediffs that affect sight/are applied to eyes/have our HediffComp_NightVision
            //  the second is a subset of the 1st with just those hediffs that apply to the eyes directly
            //  the subset is so that we can mark the eye hediffs, this differentiation lets us ensure that 
            //  two bionic eyes are required to get the full nightvis or photosensitivity where as for non-eye hediffs
            //  only one is required
            //  In both cases, we try and filter quite strictly



            //Find all hediffs that effect sight
            var allSightAffectingHediffs = new HashSet<HediffDef>(
                collection: DefDatabase<HediffDef>.AllDefsListForReading.FindAll(
                    match: hediffdef
                                => hediffdef.stages != null
                                   && hediffdef.stages.Exists(
                                       match: stage => stage.capMods != null
                                                       && stage.capMods.Exists(match: pcm => pcm.capacity == PawnCapacityDefOf.Sight)
                                   )
                )
            );

            //Comps: allows for adding Comp_NightVision to hediffdef via xml even if hediffdef does not affect sight
            allSightAffectingHediffs.UnionWith(
                other: DefDatabase<HediffDef>.AllDefsListForReading.FindAll(
                    match: hediffdef => hediffdef.HasComp(compClass: typeof(HediffComp_NightVision))
                )
            );

            //Recipes: only place where the target part is defined for bionic eyes, archotech eyes etc
            var allEyeHediffs = new HashSet<HediffDef>(
                collection: DefDatabase<RecipeDef>.AllDefsListForReading.FindAll(
                    match: recdef => recdef.addsHediff != null
                                     && recdef.appliedOnFixedBodyParts != null
                                     && recdef.appliedOnFixedBodyParts.Exists(
                                         match: bpd => bpd.tags != null && bpd.tags.Contains(item: Defs_Rimworld.EyeTag)
                                     )
                                     && recdef.AllRecipeUsers.Any(predicate: ru => ru.race?.Humanlike == true)
                ).Select(selector: recdef => recdef.addsHediff)
            );

            //HediffGivers: i.e. cataracts from HediffGiver_Birthday
            allEyeHediffs.UnionWith(
                other: DefDatabase<HediffGiverSetDef>.AllDefsListForReading.FindAll(match: hgsd => hgsd.hediffGivers != null).SelectMany(
                    selector: hgsd => hgsd.hediffGivers
                                .Where(
                                    predicate: hg => hg.partsToAffect != null
                                                     && hg.partsToAffect.Exists(match: bpd => bpd.tags.Contains(item: Defs_Rimworld.EyeTag))
                                ).Select(selector: hg => hg.hediff)
                )
            );


            allEyeHediffs.RemoveWhere(match: hdD => !typeof(HediffWithComps).IsAssignableFrom(c: hdD.hediffClass));

            allSightAffectingHediffs.RemoveWhere(match: hdD => !typeof(HediffWithComps).IsAssignableFrom(c: hdD.hediffClass));

            allSightAffectingHediffs.UnionWith(other: allEyeHediffs);

            Settings.Store.AllSightAffectingHediffs = allSightAffectingHediffs;
            Settings.Store.AllEyeHediffs = allEyeHediffs;

            InitialiseHediffLightMods(allSightAffectingHediffs.ToList(), allEyeHediffs.ToList());
        }
        /// <summary>
        /// Creates a hediff light modifier setting corresponding to sight affecting hediffs
        /// - has special behaviour for hediffs that effect the eye directly
        /// - takes lists to ensure data is copied
        /// </summary>
        /// <param name="sightAffectingHediffs"></param>
        /// <param name="eyeHediffs"></param>
        private void InitialiseHediffLightMods(List<HediffDef> sightAffectingHediffs, List<HediffDef> eyeHediffs)
        {
            var hediffLightMods = Settings.Store.HediffLightMods ?? new Dictionary<HediffDef, Hediff_LightModifiers>();

            var sightNotEyeHediffs = sightAffectingHediffs.Except(eyeHediffs);

            //Check to see if any non eye hediffs have the right comp
            foreach (HediffDef hediffDef in sightNotEyeHediffs)
            {
                if (!hediffLightMods.TryGetValue(key: hediffDef, value: out Hediff_LightModifiers value)
                 || value == null)
                {
                    if (hediffDef.HasComp(compClass: typeof(HediffComp_NightVision)))
                    {
                        hediffLightMods[key: hediffDef] = new Hediff_LightModifiers(hediffDef: hediffDef);
                    }
                }

                if (value != null && AutoQualifier.HediffCheck(hediffDef: hediffDef) != null)
                {
                    value.AutoAssigned = true;
                }
            }

            //Do the same thing as above but for eye hediffs; 
            foreach (HediffDef hediffDef in eyeHediffs)
            {
                if (!hediffLightMods.TryGetValue(key: hediffDef, value: out Hediff_LightModifiers value)
                 || value == null)
                {
                    if (hediffDef.CompPropsFor(compClass: typeof(HediffComp_NightVision)) is HediffCompProperties_NightVision)
                    {
                        hediffLightMods[key: hediffDef] = new Hediff_LightModifiers(hediffDef: hediffDef)
                        { AffectsEye = true };
                    }
                    //bionic eyes and such are automatically assigned night vision, this can be individually overridden in the mod settings
                    else if (AutoQualifier.HediffCheck(hediffDef: hediffDef) is VisionType autoOptions)
                    {
                        hediffLightMods[key: hediffDef] =
                            new Hediff_LightModifiers(hediffDef: hediffDef)
                            { AffectsEye = true, AutoAssigned = true, Setting = autoOptions };
                    }
                }
                else if (hediffDef.CompPropsFor(compClass: typeof(HediffComp_NightVision)) is HediffCompProperties_NightVision)
                {
                    // Ensure bool is correct for an eye hediff
                    value.AffectsEye = true;
                }

                if (value != null && AutoQualifier.HediffCheck(hediffDef: hediffDef) != null)
                {
                    value.AutoAssigned = true;
                }
            }

            Settings.Store.HediffLightMods = hediffLightMods;
        }





    }
}