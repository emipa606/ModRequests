using Verse;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using System;

namespace PneumaticTubes
{
    /// <summary>
    /// Rules to create the Standardized Extrusion pipe networks
    /// </summary>
    [StaticConstructorOnStartup]
    public class PipeStuff
    {
        /// <summary>
        /// The mod category in the mod setting file
        /// </summary>
        public const string ModIdentifier = "KayeshPneumaticTubes";

        /// <summary>
        /// Create the pipeline defs (and associated buildings) and add them into play
        /// </summary>
        static PipeStuff()
        {
            Log.Warning("PNEUMATIC TUBES: LOADING BEGINS");

            HashSet<ThingDef> addedDefs = new HashSet<ThingDef>();

            HashSet<ThingDef> pipeDefsToAdd = new HashSet<ThingDef>();
            HashSet<PipeSystem.PipeNetDef> pipeNetDefsToAdd = new HashSet<PipeSystem.PipeNetDef>();

            Log.Warning($"PNEUMATIC TUBES: {ModSettings.GetAllowedThingCategories().Count()} allowed categories");

            foreach (ThingCategoryDef thingCategoryDef in ModSettings.GetAllowedThingCategories())
            {

                //foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs.Where(t => ModSettings.IsAllowedThing(t)).OrderBy(t => t.label))
                foreach (ThingDef thingDef in ModSettings.dictionary[thingCategoryDef].Where(t => ModSettings.IsAllowedThing(t)).OrderBy(t => t.label))
                {
                    // Don't add chemfuel or deepchem; those're added by chemfuel expanded
                    //if (thingDef == ThingDefOf.Chemfuel || thingDef.defName == "VCHE_Deepchem")
                    //{
                    //    continue;
                    //}

                    if (!addedDefs.Contains(thingDef))
                    {
                        // Record that we're adding the new pipe network
                        addedDefs.Add(thingDef);

                        // Add the building dropdown def for pipes if not already present
                        //string designationDropDownGroupDefNamePrefix = $"{PipeStuff.ModIdentifier}_{ModSettings.GetAllowedThingCategories().Intersect(thingDef.thingCategories).First()}_group";

                        string designationDropDownGroupDefNamePrefix = $"{PipeStuff.ModIdentifier}_{thingCategoryDef}_group";

                        string designationDropDownGroupNamePipe = $"{designationDropDownGroupDefNamePrefix}_pipe";
                        DesignatorDropdownGroupDef designationDropDownGroupPipeDef = DefDatabase<DesignatorDropdownGroupDef>.GetNamed(designationDropDownGroupNamePipe, false);

                        if (designationDropDownGroupPipeDef == null)
                        {
                            designationDropDownGroupPipeDef = new DesignatorDropdownGroupDef()
                            {
                                defName = designationDropDownGroupNamePipe
                            };

                            DefDatabase<DesignatorDropdownGroupDef>.Add(designationDropDownGroupPipeDef);
                            PipeStuff.FinalizeDef(designationDropDownGroupPipeDef, typeof(DesignatorDropdownGroupDef));
                        }

                        // Add the building dropdown def for subterranean pipes if not already present
                        string designationDropDownGroupNameSubPipe = $"{designationDropDownGroupDefNamePrefix}_subpipe";
                        DesignatorDropdownGroupDef designationDropDownGroupSubPipeDef = DefDatabase<DesignatorDropdownGroupDef>.GetNamed(designationDropDownGroupNameSubPipe, false);

                        if (designationDropDownGroupSubPipeDef == null)
                        {
                            designationDropDownGroupSubPipeDef = new DesignatorDropdownGroupDef()
                            {
                                defName = designationDropDownGroupNameSubPipe
                            };

                            DefDatabase<DesignatorDropdownGroupDef>.Add(designationDropDownGroupSubPipeDef);
                            PipeStuff.FinalizeDef(designationDropDownGroupSubPipeDef, typeof(DesignatorDropdownGroupDef));
                        }

                        // Add the dropdown def for deconstructing pipes if not already present
                        string designationDropDownGroupNameDeconstruct = $"{designationDropDownGroupDefNamePrefix}_deconstruct";
                        DesignatorDropdownGroupDef designationDropDownGroupDeconstructDef = DefDatabase<DesignatorDropdownGroupDef>.GetNamed(designationDropDownGroupNameDeconstruct, false);

                        if (designationDropDownGroupDeconstructDef == null)
                        {
                            designationDropDownGroupDeconstructDef = new DesignatorDropdownGroupDef()
                            {
                                defName = designationDropDownGroupNameDeconstruct
                            };

                            DefDatabase<DesignatorDropdownGroupDef>.Add(designationDropDownGroupDeconstructDef);
                            PipeStuff.FinalizeDef(designationDropDownGroupDeconstructDef, typeof(DesignatorDropdownGroupDef));
                        }

                        // Add the building dropdown def for vats if not already present
                        string designationDropDownGroupNameVat = $"{designationDropDownGroupDefNamePrefix}_vat";
                        DesignatorDropdownGroupDef designationDropDownGroupVatDef = DefDatabase<DesignatorDropdownGroupDef>.GetNamed(designationDropDownGroupNameVat, false);

                        if (designationDropDownGroupVatDef == null)
                        {
                            designationDropDownGroupVatDef = new DesignatorDropdownGroupDef()
                            {
                                defName = designationDropDownGroupNameVat
                            };

                            DefDatabase<DesignatorDropdownGroupDef>.Add(designationDropDownGroupVatDef);
                            PipeStuff.FinalizeDef(designationDropDownGroupVatDef, typeof(DesignatorDropdownGroupDef));
                        }

                        // Add the building dropdown def for drains if not already present
                        string designationDropDownGroupNameDrain = $"{designationDropDownGroupDefNamePrefix}_drain";
                        DesignatorDropdownGroupDef designationDropDownGroupDrainDef = DefDatabase<DesignatorDropdownGroupDef>.GetNamed(designationDropDownGroupNameDrain, false);

                        if (designationDropDownGroupDrainDef == null)
                        {
                            designationDropDownGroupDrainDef = new DesignatorDropdownGroupDef()
                            {
                                defName = designationDropDownGroupNameDrain
                            };

                            DefDatabase<DesignatorDropdownGroupDef>.Add(designationDropDownGroupDrainDef);
                            PipeStuff.FinalizeDef(designationDropDownGroupDrainDef, typeof(DesignatorDropdownGroupDef));
                        }

                        // Add the building dropdown def for taps if not already present
                        string designationDropDownGroupNameTap = $"{designationDropDownGroupDefNamePrefix}_tap";
                        DesignatorDropdownGroupDef designationDropDownGroupTapDef = DefDatabase<DesignatorDropdownGroupDef>.GetNamed(designationDropDownGroupNameTap, false);

                        if (designationDropDownGroupTapDef == null)
                        {
                            designationDropDownGroupTapDef = new DesignatorDropdownGroupDef()
                            {
                                defName = designationDropDownGroupNameTap
                            };

                            DefDatabase<DesignatorDropdownGroupDef>.Add(designationDropDownGroupTapDef);
                            PipeStuff.FinalizeDef(designationDropDownGroupTapDef, typeof(DesignatorDropdownGroupDef));
                        }

                        ResearchProjectDef researchDef = PipeStuff.FindOrCreateResearchDef(thingCategoryDef);

                        // Create the pipe def
                        ThingDef pipeDef = PipeStuff.CreatePipeDef(PipeStuff.ModIdentifier, thingDef, designationDropDownGroupPipeDef, researchDef);
                        pipeDefsToAdd.Add(pipeDef);

                        // Create the pipe def
                        ThingDef subPipeDef = PipeStuff.CreateSubPipeDef(PipeStuff.ModIdentifier, thingDef, designationDropDownGroupSubPipeDef, researchDef);
                        pipeDefsToAdd.Add(subPipeDef);

                        // Create the pipe network def
                        PipeSystem.PipeNetDef pipeNetDef = PipeStuff.CreatePipeNetDef(PipeStuff.ModIdentifier, thingDef, pipeDef, subPipeDef);
                        pipeDef.comps = new List<CompProperties>()
                    {
                        new PipeSystem.CompProperties_Resource()
                        {
                            pipeNet = pipeNetDef
                        }
                    };
                        subPipeDef.comps = new List<CompProperties>()
                    {
                        new PipeSystem.CompProperties_Resource()
                        {
                            pipeNet = pipeNetDef
                        }
                    };
                        pipeNetDefsToAdd.Add(pipeNetDef);

                        // Create the vat defs
                        for (int i = 1; i <= 5; i++)
                        {
                            ThingDef vatDef = PipeStuff.CreateStorageVat(PipeStuff.ModIdentifier, thingDef, pipeNetDef, i, designationDropDownGroupVatDef, researchDef);
                            pipeDefsToAdd.Add(vatDef);
                        }

                        ThingDef drainDef = PipeStuff.CreateDrain(PipeStuff.ModIdentifier, thingDef, pipeNetDef, designationDropDownGroupDrainDef, researchDef);
                        pipeDefsToAdd.Add(drainDef);

                        ThingDef tapDef = PipeStuff.CreateTap(PipeStuff.ModIdentifier, thingDef, pipeNetDef, designationDropDownGroupTapDef, researchDef);
                        pipeDefsToAdd.Add(tapDef);

                        ThingDef coldTapDef = PipeStuff.CreateColdTap(PipeStuff.ModIdentifier, thingDef, pipeNetDef, designationDropDownGroupTapDef, researchDef);

                        if (coldTapDef != null)
                        {
                            pipeDefsToAdd.Add(coldTapDef);
                        }
                    }
                }

            }

            // Add the pipes, vats, taps, and drains (after finishing the thingdef traversal)
            foreach (ThingDef pipeDef in pipeDefsToAdd)
            {
                DefDatabase<ThingDef>.Add(pipeDef);
                PipeStuff.FinalizeDef(pipeDef, typeof(ThingDef));
            }

            LongEventHandler.ExecuteWhenFinished(delegate
            {
                // Access the pipesystem linked pipes private dictionaries, normally set in the static constructor on the main thread
                Dictionary<PipeSystem.PipeNetDef, PipeSystem.Graphic_LinkedOverlayPipe> overlayLinked = (Dictionary<PipeSystem.PipeNetDef, PipeSystem.Graphic_LinkedOverlayPipe>)typeof(PipeSystem.LinkedPipes)
                    .GetField("overlayLinked", BindingFlags.NonPublic | BindingFlags.Static)
                    .GetValue(null);

                Dictionary<ThingDef, PipeSystem.Graphic_LinkedPipe> pipesLinked = (Dictionary<ThingDef, PipeSystem.Graphic_LinkedPipe>)typeof(PipeSystem.LinkedPipes)
                    .GetField("pipesLinked", BindingFlags.NonPublic | BindingFlags.Static)
                    .GetValue(null);

                // Access the pipesystem CachedSignals private list
                List<string> signals = (List<string>)typeof(PipeSystem.CachedSignals)
                    .GetField("signals", BindingFlags.NonPublic | BindingFlags.Static)
                    .GetValue(null);

                // Add the pipenets (after finishing the thingdef traversal)
                foreach (PipeSystem.PipeNetDef pipeNetDef in pipeNetDefsToAdd)
                {
                    DefDatabase<PipeSystem.PipeNetDef>.Add(pipeNetDef);
                    PipeStuff.FinalizeDef(pipeNetDef, typeof(PipeSystem.PipeNetDef));

                    //////////
                    /// LinkedPipes code copy
                    //////////

                    // Set up the pipenet graphics
                    Graphic graphic = GraphicDatabase.Get<Graphic_Single>(pipeNetDef.overlayOptions.transmitterAtlas, ShaderDatabase.MetaOverlay);

                    if (pipeNetDef.overlayOptions.transmitterAtlas == "Special/PSTransmitterAtlas")
                    {
                        graphic = graphic.GetColoredVersion(graphic.Shader, pipeNetDef.overlayOptions.overlayColor, pipeNetDef.overlayOptions.overlayColor);
                    }

                    overlayLinked.Add(pipeNetDef, new PipeSystem.Graphic_LinkedOverlayPipe(graphic, pipeNetDef));

                    for (int j = 0; j < pipeNetDef.pipeDefs.Count; j++)
                    {
                        ThingDef thingDef = pipeNetDef.pipeDefs[j];
                        Graphic innerGraphic = GraphicDatabase.Get<Graphic_Single>(thingDef.graphic.data.texPath, ShaderDatabase.CutoutComplex);
                        pipesLinked.Add(thingDef, new PipeSystem.Graphic_LinkedPipe(innerGraphic, pipeNetDef));
                    }

                    //////////
                    /// End linkedPipes code copy
                    //////////

                    //////////
                    /// CachedSignals code copy
                    //////////
                    signals.Add("Resource" + pipeNetDef.resource.name + "TurnedOn");
                    signals.Add("Resource" + pipeNetDef.resource.name + "TurnedOff");
                    //////////
                    /// End CachedSignals code copy
                    //////////
                }

                //////////
                /// CachedResourceThings code copy
                //////////

                // Get the buildings from this mod and add them to the CachedResourceThings dictionary
                foreach (ThingDef thingDef in pipeDefsToAdd)
                {
                    List<CompProperties> comps = thingDef.comps;
                    for (int j = 0; j < comps.Count; j++)
                    {
                        CompProperties compProperties = comps[j];
                        if (compProperties is PipeSystem.CompProperties_Resource item)
                        {
                            if (!PipeSystem.CachedResourceThings.resourceCompsOf.ContainsKey(thingDef))
                            {
                                PipeSystem.CachedResourceThings.resourceCompsOf.Add(thingDef, new List<PipeSystem.CompProperties_Resource> { item });
                            }
                            else
                            {
                                PipeSystem.CachedResourceThings.resourceCompsOf[thingDef].Add(item);
                            }
                        }
                    }
                }

                //////////
                /// End CachedResourceThings code copy
                //////////
                
                //////////
                /// MaterialCreator code copy
                //////////
                foreach (PipeSystem.PipeNetDef pipeNetDef in pipeNetDefsToAdd)
                {
                    PipeSystem.MaterialCreator.materials.Add(pipeNetDef, SolidColorMaterials.SimpleSolidColorMaterial(pipeNetDef.resource.color));
                    if (pipeNetDef.resource.offTexPath != null)
                    {
                        pipeNetDef.GetType().GetField("offMat", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(pipeNetDef, MaterialPool.MatFrom(pipeNetDef.resource.offTexPath, ShaderDatabase.MetaOverlay));
                    }
                }

                foreach (ThingDef thingDef in pipeDefsToAdd)
                {
                    PipeSystem.CompProperties_ResourceStorage compProperties = thingDef.GetCompProperties<PipeSystem.CompProperties_ResourceStorage>();
                    if (compProperties != null && compProperties.extractOptions != null)
                    {
                        compProperties.extractOptions.GetType().GetField("tex", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(compProperties.extractOptions, ContentFinder<UnityEngine.Texture2D>.Get(compProperties.extractOptions.texPath));
                    }
                }
                
                //////////
                /// End MaterialCreator code copy
                //////////

                //////////
                /// ThingDefGenerator_Buildings.ImpliedBlueprintAndFrameDefs code copy
                //////////

                MethodInfo NewBlueprintDef_Thing = typeof(ThingDefGenerator_Buildings).GetMethod("NewBlueprintDef_Thing", BindingFlags.NonPublic | BindingFlags.Static);
                MethodInfo NewFrameDef_Thing = typeof(ThingDefGenerator_Buildings).GetMethod("NewFrameDef_Thing", BindingFlags.NonPublic | BindingFlags.Static);

                List<ThingDef> blueprintDefs = new List<ThingDef>();
                foreach (ThingDef def in pipeDefsToAdd)
                {
                    ThingDef blueprint = null;
                    if (def.BuildableByPlayer)
                    {
                        blueprint = (ThingDef)NewBlueprintDef_Thing.Invoke(null, new object[] { def, false, null });
                        blueprintDefs.Add(blueprint);

                        ThingDef frame = (ThingDef)NewFrameDef_Thing.Invoke(null, new object[] { def });
                        blueprintDefs.Add(frame);
                    }
                    if (def.Minifiable)
                    {
                        blueprintDefs.Add((ThingDef)NewBlueprintDef_Thing.Invoke(null, new object[] { def, true, blueprint }));
                    }
                }
                
                foreach (ThingDef blueprintDef in blueprintDefs)
                {
                    PipeStuff.FinalizeDef(blueprintDef, typeof(ThingDef));
                    DefGenerator.AddImpliedDef(blueprintDef);
                }

                //////////
                /// end ThingDefGenerator_Buildings.ImpliedBlueprintAndFrameDefs code copy
                //////////

                //////////
                /// PipeSystem.AutoLinkRefuelables code copy
                //////////
                List<ThingDef> list = new List<ThingDef>();
                foreach (ThingDef item in DefDatabase<ThingDef>.AllDefsListForReading)
                {
                    if (item.HasComp(typeof(CompRefuelable)))
                    {
                        list.Add(item);
                    }
                }

                foreach (PipeSystem.PipeNetDef pipeNetDef in pipeNetDefsToAdd)
                { 
                    if (pipeNetDef.linkToRefuelables == null)
                    {
                        continue;
                    }
                    for (int j = 0; j < pipeNetDef.linkToRefuelables.Count; j++)
                    {
                        PipeSystem.LinkOption linkOption = pipeNetDef.linkToRefuelables[j];
                        for (int k = 0; k < list.Count; k++)
                        {
                            PipeSystem.CompProperties_RefillWithPipes compProperties = list[k].GetCompProperties<PipeSystem.CompProperties_RefillWithPipes>();
                            if (compProperties == null || compProperties.thing != linkOption.thing)
                            {
                                CompProperties_Refuelable compProperties2 = list[k].GetCompProperties<CompProperties_Refuelable>();
                                if (compProperties2.fuelFilter.AllowedThingDefs.Contains(linkOption.thing))
                                {
                                    DefDatabase<ThingDef>.GetNamed(list[k].defName).comps.Add(new PipeSystem.CompProperties_RefillWithPipes
                                    {
                                        pipeNet = pipeNetDef,
                                        thing = linkOption.thing,
                                        ratio = linkOption.ratio
                                    });
                                }
                            }
                        }
                    }
                }

                //////////
                /// end PipeSystem.AutoLinkRefuelables code copy
                //////////

                DefDatabase<ThingDef>.ResolveAllReferences();
                DefDatabase<ResearchProjectDef>.ResolveAllReferences();
                ResearchProjectDef.GenerateNonOverlappingCoordinates();
                DefDatabase<PipeSystem.PipeNetDef>.ResolveAllReferences();
                DefDatabase<DesignationDef>.ResolveAllReferences();
                DefDatabase<DesignationCategoryDef>.ResolveAllReferences();
                DefDatabase<DesignatorDropdownGroupDef>.ResolveAllReferences();
            });
        }

        private static int ResearchProjectCounter = 0;

        private static ResearchProjectDef FindOrCreateResearchDef(ThingCategoryDef thingCategoryDef)
        {
            string researchDefName = $"{PipeStuff.ModIdentifier}_{thingCategoryDef}_research";

            ResearchProjectDef researchDef = DefDatabase<ResearchProjectDef>.GetNamed(researchDefName, false);

            if (researchDef == null)
            {
                ResearchProjectDef baseResearchDef = DefDatabase<ResearchProjectDef>.GetNamed("StandardizedExtrusions_PneumaticTubes");

                researchDef = new ResearchProjectDef()
                {
                    defName = researchDefName,
                    baseCost = baseResearchDef.baseCost,
                    label = "StandardizedExtrusions.PerCategoryResearchLabel".Translate(thingCategoryDef.label).CapitalizeFirst(),
                    description = "StandardizedExtrusions.PerCategoryResearchDesc".Translate(ModSettings.GetExpandedCategoryName(thingCategoryDef)).CapitalizeFirst(),
                    tab = baseResearchDef.tab,
                    techLevel = baseResearchDef.techLevel,
                    researchViewX = 1f,
                    researchViewY = PipeStuff.ResearchProjectCounter * 0.7f,
                    techprintCount = 0,
                    prerequisites = new List<ResearchProjectDef>()
                    {
                        baseResearchDef
                    }
                };

                DefDatabase<ResearchProjectDef>.Add(researchDef);
                PipeStuff.FinalizeDef(researchDef, typeof(ResearchProjectDef));
                PipeStuff.ResearchProjectCounter++;
            }

            return researchDef;
        }

        public static UnityEngine.Color DetermineColor(ThingDef thingDef)
        {
            if (thingDef.IsStuff)
            {
                return thingDef.stuffProps.color;
            }

            if (thingDef.thingCategories.Contains(ThingCategoryDefOf.MeatRaw))
            {
                return new UnityEngine.Color(128f/255, 0f, 0f);
            }

            if (thingDef.thingCategories.Contains(ThingCategoryDefOf.PlantFoodRaw))
            {
                return new UnityEngine.Color(4f / 255, 119f / 255, 59f / 255);
            }

            if (thingDef.thingCategories.Contains(ThingCategoryDefOf.PlantMatter))
            {
                return new UnityEngine.Color(11f / 255, 72f / 255, 43f / 255);
            }

            if (thingDef.thingCategories.Contains(ThingCategoryDefOf.Medicine))
            {
                return new UnityEngine.Color(204f/255, 204f / 255, 204f / 255);
            }

            if (thingDef.thingCategories.Contains(ThingCategoryDefOf.Drugs))
            {
                return new UnityEngine.Color(220f / 255, 188f / 255, 235f / 255);
            }

            return new UnityEngine.Color(165f / 255, 156f / 255, 148f / 255);
        }

        public static bool IsCandidateForPipeNetwork(ThingDef thingDef)
        {
            if (thingDef == null)
            {
                return false;
            }
            
            if (thingDef.category != ThingCategory.Item)
            {
                if (thingDef == ThingDefOf.WoodLog)
                {
                    Log.Warning($"PNEUMATIC TUBES: WoodLog is not ThingCategory.Item");
                }

                return false;
            }

            // Only things with comps; no special classes like gene packs, human ovum etc
            if (thingDef.thingClass == null || (thingDef.thingClass.Name != "Thing" && thingDef.thingClass.Name != "ThingWithComps" && thingDef.thingClass.Name != "Medicine"))
            {
                if (thingDef == ThingDefOf.WoodLog)
                {
                    if (thingDef.thingClass == null)
                    {
                        Log.Warning($"PNEUMATIC TUBES: WoodLog doesn't have a thingClass");
                    }
                    else
                    {
                        Log.Warning($"PNEUMATIC TUBES: WoodLog thingClass == {thingDef.thingClass.Name}");
                    }
                    
                }

                return false;
            }

            // No buildings, corpses, fertilized eggs, apparel, weapons, relics
            if (thingDef.thingCategories == null 
                || thingDef.thingCategories.Contains(ThingCategoryDefOf.Buildings)
                || thingDef.thingCategories.Contains(ThingCategoryDefOf.BuildingsArt)
                || thingDef.thingCategories.Contains(ThingCategoryDefOf.BuildingsSpecial)
                || thingDef.thingCategories.Contains(ThingCategoryDefOf.Corpses)
                || thingDef.thingCategories.Contains(ThingCategoryDefOf.CorpsesAnimal)
                || thingDef.thingCategories.Contains(ThingCategoryDefOf.CorpsesHumanlike)
                || thingDef.thingCategories.Contains(ThingCategoryDefOf.CorpsesInsect)
                || thingDef.thingCategories.Contains(ThingCategoryDefOf.CorpsesMechanoid)
                || thingDef.thingCategories.Contains(ThingCategoryDefOf.EggsFertilized) 
                || thingDef.thingCategories.Contains(ThingCategoryDefOf.Apparel) 
                || thingDef.thingCategories.Contains(ThingCategoryDefOf.ApparelArmor) 
                || thingDef.thingCategories.Contains(ThingCategoryDefOf.ApparelUtility)
                || thingDef.thingCategories.Contains(ThingCategoryDefOf.ArmorHeadgear)
                || thingDef.thingCategories.Contains(ThingCategoryDefOf.Weapons)
                || thingDef.thingCategories.Contains(ThingCategoryDefOf.Techprints)
                || thingDef.thingCategories.Any(c => c.defName == "InertRelics")
                || thingDef.thingCategories.Contains(ThingCategoryDefOf.Neurotrainers)
                || thingDef.thingCategories.Contains(ThingCategoryDefOf.NeurotrainersPsycast)
                || thingDef.thingCategories.Contains(ThingCategoryDefOf.NeurotrainersSkill)
                )
            {
                if (thingDef == ThingDefOf.WoodLog)
                {
                    if (thingDef.thingCategories == null)
                    {
                        Log.Warning($"PNEUMATIC TUBES: WoodLog doesn't have any thingCategories");
                    }
                    else
                    {
                        Log.Warning($"PNEUMATIC TUBES: WoodLog has categories {string.Join(",", thingDef.thingCategories)}");
                    }
                }

                return false;
            }

            // Nothing with quality
            if (thingDef.HasComp(typeof(CompQuality)))
            {
                if (thingDef == ThingDefOf.WoodLog)
                {
                    Log.Warning($"PNEUMATIC TUBES: WoodLog has CompQuality");
                }

                return false;
            }

            // Nothing unfinished
            if (thingDef.isUnfinishedThing)
            {
                if (thingDef == ThingDefOf.WoodLog)
                {
                    Log.Warning($"PNEUMATIC TUBES: WoodLog isUnfinishedThing");
                }

                return false;
            }

            // No thing with ingredients (meals, kibble)
            if (thingDef.comps.Any(c => c is CompProperties_Ingredients))
            {
                if (thingDef == ThingDefOf.WoodLog)
                {
                    Log.Warning($"PNEUMATIC TUBES: WoodLog has CompProperties_Ingredients");
                }

                return false;
            }

            // I said no fertilized eggs
            if (thingDef.comps.Any(c => c is CompProperties_Hatcher))
            {
                if (thingDef == ThingDefOf.WoodLog)
                {
                    Log.Warning($"PNEUMATIC TUBES: WoodLog has CompProperties_Hatcher");
                }

                return false;
            }

            // No dissolution things like toxic waste
            if (thingDef.comps.Any(c => c is CompProperties_Dissolution))
            {
                if (thingDef == ThingDefOf.WoodLog)
                {
                    Log.Warning($"PNEUMATIC TUBES: WoodLog has CompProperties_Dissolution");
                }
                return false;
            }

            // No art things
            if (thingDef.comps.Any(c => c is CompProperties_Art))
            {
                if (thingDef == ThingDefOf.WoodLog)
                {
                    Log.Warning($"PNEUMATIC TUBES: WoodLog has CompProperties_Art");
                }
                return false;
            }

            // No stylable things
            if (thingDef.comps.Any(c => c is CompProperties_Styleable))
            {
                if (thingDef == ThingDefOf.WoodLog)
                {
                    Log.Warning($"PNEUMATIC TUBES: WoodLog has CompProperties_Styleable");
                }
                return false;
            }

            // No neurotrainers or psytrainers
            if (thingDef.comps.Any(c => c is CompProperties_Neurotrainer))
            {
                if (thingDef == ThingDefOf.WoodLog)
                {
                    Log.Warning($"PNEUMATIC TUBES: WoodLog has CompProperties_Neurotrainer");
                }
                return false;
            }

            //// No weapons
            //if (thingDef.weaponClasses != null && thingDef.weaponClasses.Count() > 0)
            //{
            //    return false;
            //}




            // No thing with sources (like skulls that remember who they are from)
            if (thingDef.comps.Any(c => c is CompProperties_HasSources))
            {
                if (thingDef == ThingDefOf.WoodLog)
                {
                    Log.Warning($"PNEUMATIC TUBES: WoodLog has CompProperties_HasSources");
                }
                return false;
            }

            return true;
        }


        /// <summary>
        /// Create the pipe building based on the thingDef and assign it to a dropdown group
        /// </summary>
        /// <param name="defPrefix">The mod identifier to prepend onto def names</param>
        /// <param name="thingDef">The thing def the pipe will transport</param>
        /// <param name="designationDropDownGroupPipeDef">The dropdown group to add the pipe gizmo to</param>
        /// <returns>The pipe building thing def</returns>
        private static ThingDef CreatePipeDef(string defPrefix, ThingDef thingDef, DesignatorDropdownGroupDef designationDropDownGroupPipeDef, ResearchProjectDef researchDef)
        {
            ThingDef pipeDef = new ThingDef()
            {
                defName = $"{defPrefix}_{thingDef.defName}_pipe",
                label = "StandardizedExtrusions.PipeLabel".Translate(thingDef.label),
                description = "StandardizedExtrusions.PipeDescription".Translate(thingDef.label).CapitalizeFirst(),
                thingClass = typeof(PipeSystem.Building_Pipe),
                category = ThingCategory.Building,
                tickerType = TickerType.Rare,
                graphicData = new GraphicData()
                {
                    texPath = "Things/Building/Linked/StandardizedPipes_Atlas",
                    maskPath  = "Things/Building/Linked/StandardizedPipes_Atlas_Mask",
                    shaderType = ShaderTypeDefOf.CutoutComplex,
                    color = PipeStuff.DetermineColor(thingDef),
                    graphicClass = typeof(Graphic_Single),
                    linkType = LinkDrawerType.Basic,
                    linkFlags = LinkFlags.Custom7,
                    damageData = new DamageGraphicData()
                    {
                        rect = new UnityEngine.Rect(0f, 0.35f, 1f, 0.3f)
                    },
                },
                uiIconPath = "Things/Building/Linked/StandardizedPipes_MenuIcon",
                uiIconColor = PipeStuff.DetermineColor(thingDef),
                uiIconScale = 0.7f,
                building = new BuildingProperties()
                {
                    ai_chillDestination = false,
                    isInert = true,
                    isEdifice = false,
                    blueprintGraphicData = new GraphicData()
                    {
                        texPath = "Things/Building/Linked/StandardizedPipes_Blueprint_Atlas"
                    }
                },
                drawerType = DrawerType.MapMeshOnly,
                altitudeLayer = AltitudeLayer.Conduits,
                passability = Traversability.Standable,
                leaveResourcesWhenKilled = false,
                statBases = new List<StatModifier>()
                {
                    new StatModifier()
                    {
                        stat = StatDefOf.MaxHitPoints,
                        value = 40
                    },
                    new StatModifier()
                    {
                        stat = StatDefOf.WorkToBuild,
                        value = 35
                    },
                    new StatModifier()
                    {
                        stat = StatDefOf.Flammability,
                        value = 1
                    }
                },
                placingDraggableDimensions = 1,
                costList = new List<ThingDefCountClass>()
                {
                    new ThingDefCountClass(ThingDefOf.Steel, 2),
                    new ThingDefCountClass(ThingDefOf.ComponentIndustrial, 1)
                },
                placeWorkers = new List<Type>()
                {
                    typeof(PipeSystem.PlaceWorker_Pipe)
                },
                rotatable = false,
                selectable = true,
                neverMultiSelect = true,
                soundImpactDefault = DefDatabase<SoundDef>.GetNamed("BulletImpact_Metal"),
                terrainAffordanceNeeded = TerrainAffordanceDefOf.Light,
                repairEffect = DefDatabase<EffecterDef>.GetNamed("Repair"),
                designationCategory = DefDatabase<DesignationCategoryDef>.GetNamed("StandardizedExtrusions_PipeNetworks"),
                constructEffect = EffecterDefOf.ConstructMetal,
                clearBuildingArea = false,
                designatorDropdown = designationDropDownGroupPipeDef,
                researchPrerequisites = new List<ResearchProjectDef>()
                {
                    researchDef
                }
            };

            return pipeDef;
        }

        /// <summary>
        /// Create the subterranean pipe building based on the thingDef and assign it to a dropdown group
        /// </summary>
        /// <param name="defPrefix">The mod identifier to prepend onto def names</param>
        /// <param name="thingDef">The thing def the pipe will transport</param>
        /// <param name="designationDropDownGroupPipeDef">The dropdown group to add the subterranean pipe gizmo to</param>
        /// <returns>The subterranean pipe building thing def</returns>
        private static ThingDef CreateSubPipeDef(string defPrefix, ThingDef thingDef, DesignatorDropdownGroupDef designationDropDownGroupPipeDef, ResearchProjectDef researchDef)
        {
            ThingDef pipeDef = new ThingDef()
            {
                defName = $"{defPrefix}_{thingDef.defName}_subpipe",
                label = "StandardizedExtrusions.SubPipeLabel".Translate(thingDef.label),
                description = "StandardizedExtrusions.SubPipeDescription".Translate(thingDef.label).CapitalizeFirst(),
                thingClass = typeof(PipeSystem.Building_Pipe),
                category = ThingCategory.Building,
                tickerType = TickerType.Rare,
                graphicData = new GraphicData()
                {
                    texPath = "UI/CSG/IConduit",
                    graphicClass = typeof(Graphic_Single),
                    linkType = LinkDrawerType.Basic,
                    linkFlags = LinkFlags.Custom7,
                    damageData = new DamageGraphicData()
                    {
                        rect = new UnityEngine.Rect(0f, 0.35f, 1f, 0.3f)
                    },
                },
                uiIconPath = "Things/Building/Linked/StandardizedPipes_MenuIcon",
                uiIconColor = PipeStuff.DetermineColor(thingDef),
                uiIconScale = 0.7f,
                building = new BuildingProperties()
                {
                    ai_chillDestination = false,
                    isInert = true,
                    isEdifice = false,
                    blueprintGraphicData = new GraphicData()
                    {
                        texPath = "Things/Building/Linked/StandardizedPipes_Blueprint_Atlas"
                    }
                },
                drawerType = DrawerType.MapMeshOnly,
                altitudeLayer = AltitudeLayer.Conduits,
                passability = Traversability.Standable,
                leaveResourcesWhenKilled = false,
                statBases = new List<StatModifier>()
                {
                    new StatModifier()
                    {
                        stat = StatDefOf.MaxHitPoints,
                        value = 400
                    },
                    new StatModifier()
                    {
                        stat = StatDefOf.WorkToBuild,
                        value = 120
                    },
                    new StatModifier()
                    {
                        stat = StatDefOf.Flammability,
                        value = 0
                    }
                },
                placingDraggableDimensions = 1,
                costList = new List<ThingDefCountClass>()
                {
                    new ThingDefCountClass(ThingDefOf.Steel, 4),
                    new ThingDefCountClass(ThingDefOf.ComponentIndustrial, 1)
                },
                placeWorkers = new List<Type>()
                {
                    typeof(PipeSystem.PlaceWorker_Pipe)
                },
                rotatable = false,
                selectable = true,
                neverMultiSelect = true,
                soundImpactDefault = DefDatabase<SoundDef>.GetNamed("BulletImpact_Metal"),
                terrainAffordanceNeeded = TerrainAffordanceDefOf.Light,
                repairEffect = DefDatabase<EffecterDef>.GetNamed("Repair"),
                designationCategory = DefDatabase<DesignationCategoryDef>.GetNamed("StandardizedExtrusions_PipeNetworks"),
                constructEffect = EffecterDefOf.ConstructMetal,
                clearBuildingArea = false,
                designatorDropdown = designationDropDownGroupPipeDef,
                researchPrerequisites = new List<ResearchProjectDef>()
                {
                    researchDef
                }
            };

            return pipeDef;
        }

        /// <summary>
        /// Create the pipe network based on the thingDef and the pipe building
        /// </summary>
        /// <param name="defPrefix">The mod identifier to prepend onto def names</param>
        /// <param name="thingDef">The thing def the pipe network will transport</param>
        /// <param name="pipeDef">The pipe building doing the transporting</param>
        /// <param name="subPipeDef">The subterranean pipe building doing the transporting</param>
        /// <returns>The pipenet def</returns>
        private static PipeSystem.PipeNetDef CreatePipeNetDef(string defPrefix, ThingDef thingDef, ThingDef pipeDef, ThingDef subPipeDef)
        {
            PipeSystem.PipeNetDef pipenetDef = new PipeSystem.PipeNetDef()
            {
                defName = $"{defPrefix}_{thingDef.defName}_pipenet",
                transferAmount = 100,
                convertAmount = 100,
                noStorageAlert = false,
                resource = new PipeSystem.Resource()
                {
                    name = thingDef.LabelCap,
                    unit = "units",
                    color = PipeStuff.DetermineColor(thingDef),
                    onlyShowStored = true,
                },
                pipeDefs = new List<ThingDef>()
                {
                    pipeDef,
                    subPipeDef
                },
                designator = new PipeSystem.DeconstructOption()
                {
                    deconstructIconPath = "UI/Gizmos/StandardizedPipes_Deconstruct",
                    designationCategoryDef = DefDatabase<DesignationCategoryDef>.GetNamed("StandardizedExtrusions_PipeNetworks")
                },
                overlayOptions = new PipeSystem.OverlayOption()
                {
                    overlayColor = PipeStuff.DetermineColor(thingDef)
                },

                linkToRefuelables = new List<PipeSystem.LinkOption>()
                {
                    new PipeSystem.LinkOption()
                    {
                        thing = thingDef,
                        ratio = 1
                    }
                }
            };

            return pipenetDef;
        }

        private static Dictionary<int, string> VatSize = new Dictionary<int, string>()
        {
            {1, "small"},
            {2, "medium"},
            {3, "large" },
            {4, "huge"},
            {5, "gigantic"},
        };

        public static void AdjustVatCapacity()
        {
            for (int i = 1; i <= 5; i++)
            {
                var vatList = DefDatabase<ThingDef>.AllDefs.Where(t => t.defName.StartsWith(PipeStuff.ModIdentifier) && t.defName.EndsWith($"_vat_{VatSize[i]}"));

                foreach (var vat in vatList)
                {
                    PipeSystem.CompProperties_ResourceStorage vatComp = (PipeSystem.CompProperties_ResourceStorage)vat.comps.Where(c => c is PipeSystem.CompProperties_ResourceStorage).First();

                    var thingDef = vatComp.pipeNet.linkToRefuelables[0].thing;
                    int capacity = i * i * i * 3 * thingDef.stackLimit;

                    vatComp.storageCapacity = capacity;
                    vat.description = "StandardizedExtrusions.VatDescription".Translate(VatSize[i], capacity, thingDef.label).CapitalizeFirst();
                }
            }
        }

        /// <summary>
        /// Create a storage vat building of size (sizeKey, sizeKey) based on the thingDef and assign it to a dropdown group
        /// </summary>
        /// <param name="defPrefix">The mod identifier to prepend onto def names</param>
        /// <param name="thingDef">The thing def the building will store</param>
        /// <param name="pipeNetDef">The pipe network the building will attach to</param>
        /// <param name="sizeKey">The width and height of the building</param>
        /// <param name="designationDropDownGroupPipeDef">The dropdown group to add the building gizmo to</param>
        /// <returns>The storage vat building</returns>
        private static ThingDef CreateStorageVat(string defPrefix, ThingDef thingDef, PipeSystem.PipeNetDef pipeNetDef, int sizeKey, DesignatorDropdownGroupDef designationDropDownGroupPipeDef, ResearchProjectDef researchDef)
        {
            int capacity = sizeKey * sizeKey * sizeKey * 3 * thingDef.stackLimit;

            var powerComp = new CompProperties_Power()
            {
                compClass = typeof(CompPowerTrader)
            };

            powerComp.GetType().GetField("basePowerConsumption", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(powerComp, 75 * sizeKey * (sizeKey <= 1 ? 1 : sizeKey - 1));

            ThingDef vatDef = new ThingDef()
            {
                defName = $"{defPrefix}_{thingDef.defName}_vat_{VatSize[sizeKey]}",
                label = "StandardizedExtrusions.VatLabel".Translate(VatSize[sizeKey], thingDef.label),
                description = "StandardizedExtrusions.VatDescription".Translate(VatSize[sizeKey], capacity, thingDef.label).CapitalizeFirst(),
                category = ThingCategory.Building,
                thingClass = typeof(Building),
                drawerType = DrawerType.MapMeshAndRealTime,
                graphicData = new GraphicData()
                {
                    texPath = "Things/Building/Production/StandardizedVat",
                    maskPath = "Things/Building/Production/StandardizedVat_Mask",
                    shaderType = ShaderTypeDefOf.CutoutComplex,
                    color = PipeStuff.DetermineColor(thingDef),
                    graphicClass = typeof(Graphic_Single),
                    drawSize = new UnityEngine.Vector2(sizeKey * 1.5f, sizeKey * 1.5f),
                    damageData = new DamageGraphicData()
                    {
                        cornerTL = "Damage/Corner",
                        cornerTR = "Damage/Corner",
                        cornerBL = "Damage/Corner",
                        cornerBR = "Damage/Corner",
                    },
                },
                altitudeLayer = AltitudeLayer.Building,
                passability = Traversability.Impassable,
                blockWind = true,
                castEdgeShadows = true,
                fillPercent = 1,
                statBases = new List<StatModifier>()
                {
                    new StatModifier()
                    {
                        stat = StatDefOf.MaxHitPoints,
                        value = 75 * (1 << (sizeKey - 1))
                    },
                    new StatModifier()
                    {
                        stat = StatDefOf.WorkToBuild,
                        value = 400 * (1 << (sizeKey - 1))
                    },
                    new StatModifier()
                    {
                        stat = StatDefOf.Flammability,
                        value = 1
                    }
                },
                size = new IntVec2(sizeKey, sizeKey),
                costList = new List<ThingDefCountClass>()
                {
                    new ThingDefCountClass(ThingDefOf.Steel, sizeKey * sizeKey * 40),
                    new ThingDefCountClass(ThingDefOf.ComponentIndustrial, sizeKey * sizeKey)
                },
                comps = new List<CompProperties>()
                {
                    new PipeSystem.CompProperties_ResourceStorage()
                    {
                        pipeNet = pipeNetDef,
                        storageCapacity = capacity,
                        drawStorageBar = true,
                        addStorageInfo = true,
                        addTransferGizmo = true,
                        barSize = new UnityEngine.Vector2(0.375f * sizeKey, 0.2f * sizeKey),
                        margin =  (1 << (sizeKey - 1)) * 0.0175f,
                        centerOffset = new UnityEngine.Vector3(0, 0, -0.175f * sizeKey),
                        extractOptions = new PipeSystem.ExtractOptions()
                        {
                            texPath = "UI/Gizmos/ExtractResource",
                            labelKey = "StandardizedExtrusions.ExtractResourceLabel",
                            descKey = "StandardizedExtrusions.ExtractResourceDesc",
                            extractAmount = 75,
                            extractTime = 150,
                            thing = thingDef,
                            ratio = 1
                        },
                        destroyOption = new PipeSystem.DestroyOption()
                        {
                            filth = ThingDefOf.Filth_RubbleBuilding,
                            ratio = 1,
                            maxRadius = sizeKey * sizeKey,
                        },
                        contentRequirePower = thingDef.HasComp(typeof(CompProperties_Rottable)) && thingDef.GetCompProperties<CompProperties_Rottable>().rotDestroys,
                    },
                    powerComp
                },
                tickerType = TickerType.Rare,
                rotatable = false,
                selectable = true,
                soundImpactDefault = DefDatabase<SoundDef>.GetNamed("BulletImpact_Metal"),
                terrainAffordanceNeeded = TerrainAffordanceDefOf.Medium,
                repairEffect = DefDatabase<EffecterDef>.GetNamed("Repair"),
                leaveResourcesWhenKilled = false,
                filthLeaving = ThingDefOf.Filth_RubbleBuilding,
                designationCategory = DefDatabase<DesignationCategoryDef>.GetNamed("StandardizedExtrusions_PipeNetworks"),
                designatorDropdown = designationDropDownGroupPipeDef,
                constructEffect = EffecterDefOf.ConstructMetal,
                staticSunShadowHeight = 0.2f,
                clearBuildingArea = true,
                researchPrerequisites = new List<ResearchProjectDef>()
                {
                    researchDef
                },
            };

            vatDef.GetType().GetField("canOverlapZones", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(vatDef, false);

            if (sizeKey >= 4)
            {
                vatDef.researchPrerequisites.Add(DefDatabase<ResearchProjectDef>.GetNamed("StandardizedExtrusions_ImposingContainers"));
            }

            return vatDef;
        }

        /// <summary>
        /// Create a drain building based on the thingDef and assign it to a dropdown group
        /// </summary>
        /// <param name="defPrefix">The mod identifier to prepend onto def names</param>
        /// <param name="thingDef">The thing def the building will store</param>
        /// <param name="pipeNetDef">The pipe network the building will attach to</param>
        /// <param name="designationDropDownGroupPipeDef">The dropdown group to add the building gizmo to</param>
        /// <returns>The drain building</returns>
        private static ThingDef CreateDrain(string defPrefix, ThingDef thingDef, PipeSystem.PipeNetDef pipeNetDef, DesignatorDropdownGroupDef designationDropDownGroupPipeDef, ResearchProjectDef researchDef)
        {
            ThingDef drainDef = new ThingDef()
            {
                defName = $"{defPrefix}_{thingDef.defName}_drain",
                label = "StandardizedExtrusions.DrainLabel".Translate(thingDef.label),
                description = "StandardizedExtrusions.DrainDescription".Translate(thingDef.label).CapitalizeFirst(),
                category = ThingCategory.Building,
                minifiedDef = ThingDefOf.MinifiedThing,
                thingClass = typeof(Building_Storage),
                thingCategories = new List<ThingCategoryDef>()
                {
                    DefDatabase<ThingCategoryDef>.GetNamed("BuildingsFurniture")
                },
                drawerType = DrawerType.MapMeshOnly,
                graphicData = new GraphicData()
                {
                    texPath = "Things/Building/Production/StandardizedDrain",
                    maskPath = "Things/Building/Production/StandardizedDrain_Mask",
                    shaderType = ShaderTypeDefOf.CutoutComplex,
                    color = PipeStuff.DetermineColor(thingDef),
                    graphicClass = typeof(Graphic_Single),
                    damageData = new DamageGraphicData()
                    {
                        cornerTL = "Damage/Corner",
                        cornerTR = "Damage/Corner",
                        cornerBL = "Damage/Corner",
                        cornerBR = "Damage/Corner",
                    },
                    drawRotated = false,
                    allowFlip = false,
                },
                altitudeLayer = AltitudeLayer.BuildingOnTop,
                passability = Traversability.Standable,
                blockWind = false,
                castEdgeShadows = true,
                fillPercent = 0.5f,
                pathCost = 50,
                statBases = new List<StatModifier>()
                {
                    new StatModifier()
                    {
                        stat = StatDefOf.MaxHitPoints,
                        value = 100
                    },
                    new StatModifier()
                    {
                        stat = StatDefOf.Mass,
                        value = 8
                    },
                    new StatModifier()
                    {
                        stat = StatDefOf.WorkToBuild,
                        value = 600
                    },
                    new StatModifier()
                    {
                        stat = StatDefOf.Flammability,
                        value = 1
                    },
                    new StatModifier()
                    {
                        stat = StatDefOf.Beauty,
                        value = 0.5f
                    }
                },
                size = new IntVec2(1, 1),
                costList = new List<ThingDefCountClass>()
                {
                    new ThingDefCountClass(ThingDefOf.Steel, 60),
                    new ThingDefCountClass(ThingDefOf.ComponentIndustrial, 1)
                },
                comps = new List<CompProperties>()
                {
                    new PipeSystem.CompProperties_ConvertThingToResource()
                    {
                        pipeNet = pipeNetDef,
                        thing = thingDef,
                    },
                },
                rotatable = false,
                building = new BuildingProperties()
                {
                    preventDeteriorationOnTop = true,
                    ignoreStoredThingsBeauty = true,
                    fixedStorageSettings = new StorageSettings()
                    {
                        filter = new ThingFilter(),
                    },
                    defaultStorageSettings = new StorageSettings(),
                },
                inspectorTabs = new List<Type>()
                {
                    typeof(ITab_Storage)
                },
                selectable = true,
                soundImpactDefault = DefDatabase<SoundDef>.GetNamed("BulletImpact_Metal"),
                terrainAffordanceNeeded = TerrainAffordanceDefOf.Medium,
                repairEffect = DefDatabase<EffecterDef>.GetNamed("Repair"),
                leaveResourcesWhenKilled = false,
                filthLeaving = ThingDefOf.Filth_RubbleBuilding,
                designationCategory = DefDatabase<DesignationCategoryDef>.GetNamed("StandardizedExtrusions_PipeNetworks"),
                designatorDropdown = designationDropDownGroupPipeDef,
                constructEffect = EffecterDefOf.ConstructMetal,
                staticSunShadowHeight = 0.5f,
                surfaceType = SurfaceType.Item,
                clearBuildingArea = true,
                researchPrerequisites = new List<ResearchProjectDef>()
                {
                    researchDef
                },
            };

            drainDef.GetType().GetField("canOverlapZones", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(drainDef, false);

            drainDef.building.fixedStorageSettings.filter.GetType().GetField("disallowNotEverStorable", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(drainDef.building.fixedStorageSettings.filter, true);

            List<ThingDef> thingDefs = new List<ThingDef>()
            {
                thingDef
            };

            drainDef.building.fixedStorageSettings.filter.GetType().GetField("thingDefs", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(drainDef.building.fixedStorageSettings.filter, thingDefs);

            drainDef.building.defaultStorageSettings.GetType().GetField("priorityInt", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(drainDef.building.defaultStorageSettings, StoragePriority.Important);
            drainDef.building.defaultStorageSettings.filter.GetType().GetField("disallowNotEverStorable", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(drainDef.building.defaultStorageSettings.filter, true);

            List<ThingDef> defaultThingDefs = new List<ThingDef>()
            {
                thingDef
            };

            drainDef.building.defaultStorageSettings.filter.GetType().GetField("thingDefs", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(drainDef.building.defaultStorageSettings.filter, defaultThingDefs);

            return drainDef;
        }

        /// <summary>
        /// Create a tap building based on the thingDef and assign it to a dropdown group
        /// </summary>
        /// <param name="defPrefix">The mod identifier to prepend onto def names</param>
        /// <param name="thingDef">The thing def the building will store</param>
        /// <param name="pipeNetDef">The pipe network the building will attach to</param>
        /// <param name="designationDropDownGroupPipeDef">The dropdown group to add the building gizmo to</param>
        /// <returns>The tap building</returns>
        private static ThingDef CreateTap(string defPrefix, ThingDef thingDef, PipeSystem.PipeNetDef pipeNetDef, DesignatorDropdownGroupDef designationDropDownGroupPipeDef, ResearchProjectDef researchDef)
    {
            ThingDef tapDef = new ThingDef()
            {
                defName = $"{defPrefix}_{thingDef.defName}_tap",
                label = "StandardizedExtrusions.TapLabel".Translate(thingDef.label),
                description = "StandardizedExtrusions.TapDescription".Translate(thingDef.label).CapitalizeFirst(),
                category = ThingCategory.Building,
                minifiedDef = ThingDefOf.MinifiedThing,
                thingClass = typeof(Building_Storage),
                thingCategories = new List<ThingCategoryDef>()
                {
                    DefDatabase<ThingCategoryDef>.GetNamed("BuildingsFurniture")
                },
                drawerType = DrawerType.MapMeshOnly,
                graphicData = new GraphicData()
                {
                    texPath = "Things/Building/Production/StandardizedTap",
                    maskPath = "Things/Building/Production/StandardizedTap_Mask",
                    shaderType = ShaderTypeDefOf.CutoutComplex,
                    color = PipeStuff.DetermineColor(thingDef),
                    graphicClass = typeof(Graphic_Single),
                    damageData = new DamageGraphicData()
                    {
                        cornerTL = "Damage/Corner",
                        cornerTR = "Damage/Corner",
                        cornerBL = "Damage/Corner",
                        cornerBR = "Damage/Corner",
                    },
                    drawRotated = false,
                    allowFlip = false,
                },
                altitudeLayer = AltitudeLayer.Building,
                passability = Traversability.PassThroughOnly,
                blockWind = false,
                castEdgeShadows = true,
                fillPercent = 0.5f,
                pathCost = 50,
                statBases = new List<StatModifier>()
                {
                    new StatModifier()
                    {
                        stat = StatDefOf.MaxHitPoints,
                        value = 100
                    },
                    new StatModifier()
                    {
                        stat = StatDefOf.Mass,
                        value = 8
                    },
                    new StatModifier()
                    {
                        stat = StatDefOf.WorkToBuild,
                        value = 600
                    },
                    new StatModifier()
                    {
                        stat = StatDefOf.Flammability,
                        value = 1
                    },
                    new StatModifier()
                    {
                        stat = StatDefOf.Beauty,
                        value = 0.5f
                    }
                },
                size = new IntVec2(1, 1),
                costList = new List<ThingDefCountClass>()
                {
                    new ThingDefCountClass(ThingDefOf.Steel, 60),
                    new ThingDefCountClass(ThingDefOf.ComponentIndustrial, 1)
                },
                comps = new List<CompProperties>()
                {
                    new PipeSystem.CompProperties_ConvertResourceToThing()
                    {
                        pipeNet = pipeNetDef,
                        thing = thingDef,
                        compClass = typeof(CompConvertToThingExtra)
                    },
                },
                rotatable = false,
                building = new BuildingProperties()
                {
                    preventDeteriorationOnTop = true,
                    ignoreStoredThingsBeauty = true,
                    fixedStorageSettings = new StorageSettings()
                    {
                        filter = new ThingFilter(),
                    },
                    defaultStorageSettings = new StorageSettings(),
                },
                inspectorTabs = new List<Type>()
                {
                    typeof(ITab_Storage)
                },
                selectable = true,
                soundImpactDefault = DefDatabase<SoundDef>.GetNamed("BulletImpact_Metal"),
                terrainAffordanceNeeded = TerrainAffordanceDefOf.Medium,
                repairEffect = DefDatabase<EffecterDef>.GetNamed("Repair"),
                leaveResourcesWhenKilled = false,
                filthLeaving = ThingDefOf.Filth_RubbleBuilding,
                designationCategory = DefDatabase<DesignationCategoryDef>.GetNamed("StandardizedExtrusions_PipeNetworks"),
                designatorDropdown = designationDropDownGroupPipeDef,
                constructEffect = EffecterDefOf.ConstructMetal,
                staticSunShadowHeight = 0.5f,
                surfaceType = SurfaceType.Item,
                clearBuildingArea = true,
                researchPrerequisites = new List<ResearchProjectDef>()
                {
                    researchDef
                },
            };

            tapDef.GetType().GetField("canOverlapZones", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(tapDef, false);

            tapDef.building.fixedStorageSettings.filter.GetType().GetField("disallowNotEverStorable", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(tapDef.building.fixedStorageSettings.filter, true);

            List<ThingDef> thingDefs = new List<ThingDef>()
            {
                thingDef
            };

            tapDef.building.fixedStorageSettings.filter.GetType().GetField("thingDefs", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(tapDef.building.fixedStorageSettings.filter, thingDefs);

            tapDef.building.defaultStorageSettings.GetType().GetField("priorityInt", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(tapDef.building.defaultStorageSettings, StoragePriority.Important);
            tapDef.building.defaultStorageSettings.filter.GetType().GetField("disallowNotEverStorable", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(tapDef.building.defaultStorageSettings.filter, true);

            List<ThingDef> defaultThingDefs = new List<ThingDef>()
            {
                thingDef
            };

            tapDef.building.defaultStorageSettings.filter.GetType().GetField("thingDefs", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(tapDef.building.defaultStorageSettings.filter, defaultThingDefs);

            return tapDef;
        }

        private static ThingDef CreateColdTap(string defPrefix, ThingDef thingDef, PipeSystem.PipeNetDef pipeNetDef, DesignatorDropdownGroupDef designationDropDownGroupPipeDef, ResearchProjectDef researchDef)
        {
            return null;
        }

        /// <summary>
        /// Finalize the def: set hashes, resolve refs
        /// </summary>
        /// <param name="def">The def to finalize</param>
        /// <param name="defType">The type of the def</param>
        public static void FinalizeDef(Def def, Type defType)
        {
            if (def.shortHash == 0)
            {
                FieldInfo takenHashesPerDeftypeField = typeof(ShortHashGiver).GetField("takenHashesPerDeftype", BindingFlags.NonPublic | BindingFlags.Static);

                Dictionary<Type, HashSet<ushort>> takenHashesPerDeftype = (Dictionary<Type, HashSet<ushort>>)(takenHashesPerDeftypeField.GetValue(null));

                if (!takenHashesPerDeftype.ContainsKey(defType))
                {
                    takenHashesPerDeftype.Add(defType, new HashSet<ushort>());
                }

                var foo = typeof(ShortHashGiver).GetMethod("GiveShortHash", BindingFlags.NonPublic | BindingFlags.Static);

                foo.Invoke(null, new object[] { def, defType, takenHashesPerDeftype[defType] });
            }

            def.ResolveReferences();

            if (defType == typeof(RecipeDef) && ((RecipeDef)def).recipeUsers != null)
            {
                foreach (ThingDef building in (def as RecipeDef).recipeUsers)
                {
                    var foo = typeof(ThingDef).GetField("allRecipesCached", BindingFlags.NonPublic | BindingFlags.Instance);
                    foo.SetValue(building, null);
                }
            }

            def.PostLoad();
        }
    }
}
