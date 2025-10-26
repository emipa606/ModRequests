using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;
using System.Reflection;

namespace Clutter_StructureWall
{
    static public class StructureDefGenerator
    {
       static  public void WallDefs()
       {

            if (C_StructureWallModUI.ModdedWallsActive)
            {
                //---------------------------------->> Silver Wall <<------------------------------------------    

                ThingDef SilverWallzie = new ThingDef()
                {
                    //Wall Custom 
                    defName = "ClutterSilverWall",
                    label = "Panel Reinforced Wall",
                    description = "Heavily reinforced wall with some shiny stuff on it to please the eyes",
                    thingClass = typeof(Building),
                    tickerType = TickerType.Never,

                    //Wall Static
                    size = new IntVec2(1, 1),
                    passability = Traversability.Impassable,
                    category = ThingCategory.Building,
                    selectable = true,
                    designationCategory = ClutterStructureDefOf.Structure,
                    useHitPoints = true,
                    altitudeLayer = AltitudeLayer.Building,
                    leaveResourcesWhenKilled = true,
                    blockWind = true,
                    rotatable = false,
                    costStuffCount = 10,
                    castEdgeShadows = true,
                    fillPercent = 1,
                    coversFloor = true,
                    placingDraggableDimensions = 1,
                    neverMultiSelect = true,
                    terrainAffordanceNeeded = TerrainAffordanceDefOf.Heavy,
                    holdsRoof = true,
                    staticSunShadowHeight = 1f,
                    blockLight = true,
                    repairEffect = EffecterDefOf.ConstructMetal,
                    filthLeaving = ThingDefOf.Filth_RubbleBuilding,
                    researchPrerequisites = new List<ResearchProjectDef>
                    {
                        ResearchProjectDef.Named("WallReinforcment"),
                    },
                    stuffCategories = new List<StuffCategoryDef>
                    {
                        StuffCategoryDefOf.Metallic,
                        StuffCategoryDefOf.Stony,
                        StuffCategoryDefOf.Woody

                    },
                    costList = new List<ThingDefCountClass>
                    {
                        new ThingDefCountClass(ThingDefOf.Steel, 4)

                    },

                    uiIconPath = "Clutter/Structure/SilverWallIco",

                    drawerType = DrawerType.MapMeshAndRealTime,
                    graphicData = new GraphicData()
                    {
                        texPath = "Clutter/Structure/SilverWall",
                        graphicClass = typeof(Graphic_Single),
                        shaderType = ShaderTypeDefOf.CutoutComplex,
                        linkType = LinkDrawerType.CornerFiller,
                        linkFlags = LinkFlags.Rock | LinkFlags.Wall,
                        damageData = new DamageGraphicData()
                        {
                            cornerTL = "Damage/Corner",
                            cornerTR = "Damage/Corner",
                            cornerBL = "Damage/Corner",
                            cornerBR = "Damage/Corner",
                            edgeBot = "Damage/Edge",
                            edgeLeft = "Damage/Edge",
                            edgeRight = "Damage/Edge",
                            edgeTop = "Damage/Edge",

                        }

                    },
                    statBases = new List<StatModifier>()
                        {
                            new StatModifier()
                            {
                                stat = StatDefOf.MaxHitPoints,
                                value = 620
                            },
                            new StatModifier()
                            {
                                stat = StatDefOf.WorkToBuild,
                                value = 1550
                            },
                            new StatModifier()
                            {
                                stat = StatDefOf.Flammability,
                                value = 0
                            },
                            new StatModifier()
                            {
                                stat = StatDefOf.Beauty,
                                value = 1
                            }
                        },
                    //costList = new List<ThingCountClass>()
                    //{
                    //    new ThingCountClass(ThingDefOf.ChunkSlagSteel, 2)
                    //},
                    building = new BuildingProperties()
                    {
                        isInert = true,
                        paintable = true,
                        isPlaceOverableWall = true,
                        supportsWallAttachments = true
                    }

                };

                StuffGeneratrs(SilverWallzie);
                 ThingDef UtilityWallzie = new Clutter_Structure.ClutterStructureThingDefs()
                {
                    //Wall Custom 
                    defName = "ClutterUtilityWall",
                    label = "Utility Wall",
                    description = "Upgraded version of basic walls, with the ability to easily attach things, like centuries old dust!",
                    thingClass = typeof(Clutter_Structure.Wall_Multi),
                    tickerType = TickerType.Never,

                    //Wall Static
                    size = new IntVec2(1, 1),
                    passability = Traversability.Impassable,
                    category = ThingCategory.Building,
                    selectable = true,
                    designationCategory = ClutterStructureDefOf.Structure,
                    useHitPoints = true,
                    altitudeLayer = AltitudeLayer.Building,
                    leaveResourcesWhenKilled = true,
                    blockWind = true,
                    rotatable = false,
                    costStuffCount = 10,
                    castEdgeShadows = true,
                    fillPercent = 1,
                    coversFloor = true,
                    placingDraggableDimensions = 1,
                    neverMultiSelect = true,
                    terrainAffordanceNeeded = TerrainAffordanceDefOf.Heavy,
                    holdsRoof = true,
                    staticSunShadowHeight = 1f,
                    blockLight = true,
                    repairEffect = EffecterDefOf.ConstructMetal,
                    filthLeaving = ThingDefOf.Filth_RubbleBuilding,
                    researchPrerequisites = new List<ResearchProjectDef>
                    {
                        ResearchProjectDef.Named("UtilityStructures"),
                    },
                    stuffCategories = new List<StuffCategoryDef>
                    {
                        StuffCategoryDefOf.Metallic,
                    },
                    costList = new List<ThingDefCountClass>
                    {
                      new ThingDefCountClass(ThingDefOf.WoodLog, 6)
                       
                    },

                    uiIconPath = "Clutter/Structure/UWallIco",

                    drawerType = DrawerType.MapMeshAndRealTime,
                    graphicData = new GraphicData()
                    {
                        texPath = "Clutter/Structure/UWall",
                        graphicClass = typeof(Graphic_Single),
                        shaderType = ShaderTypeDefOf.CutoutComplex,
                        linkType = LinkDrawerType.CornerFiller,
                        linkFlags = LinkFlags.Rock | LinkFlags.Wall,
                        damageData = new DamageGraphicData()
                        {
                            cornerTL = "Damage/Corner",
                            cornerTR = "Damage/Corner",
                            cornerBL = "Damage/Corner",
                            cornerBR = "Damage/Corner",
                            edgeBot = "Damage/Edge",
                            edgeLeft = "Damage/Edge",
                            edgeRight = "Damage/Edge",
                            edgeTop = "Damage/Edge",

                        }

                    },
                    statBases = new List<StatModifier>()
                        {
                            new StatModifier()
                            {
                                stat = StatDefOf.MaxHitPoints,
                                value = 250
                            },
                            new StatModifier()
                            {
                                stat = StatDefOf.WorkToBuild,
                                value = 1000
                            },
                            new StatModifier()
                            {
                                stat = StatDefOf.Flammability,
                                value = 0.5f
                            },
                            new StatModifier()
                            {
                                stat = StatDefOf.Beauty,
                                value = 1
                            }
                        },
                    //costList = new List<ThingCountClass>()
                    //{
                    //    new ThingCountClass(ThingDefOf.ChunkSlagSteel, 2)
                    //},
                    building = new BuildingProperties()
                    {
                        isInert = true,
                        paintable = true,
                        isPlaceOverableWall = true,
                        supportsWallAttachments = true
                    },
                    SecondaryMaterialPath ="Clutter/Structure/UWallA",
                    ThirdMaterialPath = "Clutter/Structure/UWallB"
                };

                StuffGeneratrs(UtilityWallzie);
             
              }
        }   

       public static void StuffGeneratrs(ThingDef Wallzie)
       {
           if (Wallzie != null)
           {
               if (Wallzie.blueprintDef == null)
               {
                   Wallzie.blueprintDef = (ThingDef)typeof(ThingDefGenerator_Buildings).GetMethod("NewBlueprintDef_Thing", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { Wallzie, false, null, false });
                   Wallzie.blueprintDef.ResolveReferences();
                   Wallzie.blueprintDef.PostLoad();
               }

               if (Wallzie.frameDef == null)
               {

                   Wallzie.frameDef = (ThingDef)typeof(ThingDefGenerator_Buildings).GetMethod("NewFrameDef_Thing", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { Wallzie, false });
                   Wallzie.frameDef.ResolveReferences();
                   Wallzie.frameDef.PostLoad();
                   Wallzie.ResolveReferences();
                   Wallzie.PostLoad();
               }

               // Short hash generator dont delete this dumbass, might come in handy

               //if (SilverWallzie.shortHash == 0)
               //{
               //    MethodInfo shortHashGiver = typeof(ShortHashGiver).GetMethod("GiveShortHash", BindingFlags.NonPublic | BindingFlags.Static);
               //    Type t = typeof(ThingDef);
               //    shortHashGiver.Invoke(null, new object[] { SilverWallzie, t });
               //    shortHashGiver.Invoke(null, new object[] { SilverWallzie.blueprintDef, t });
               //    shortHashGiver.Invoke(null, new object[] { SilverWallzie.frameDef, t });
               //}
               DefDatabase<ThingDef>.Add(Wallzie);

               DefDatabase<ThingDef>.Add(Wallzie.blueprintDef);
               DefDatabase<ThingDef>.Add(Wallzie.frameDef);
               Wallzie.designationCategory.ResolveReferences();
               Wallzie.designationCategory.PostLoad();
           }
       }

    }
}
