using HarmonyLib;
using RimWorld;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RedAlert
{
    [StaticConstructorOnStartup]
    public static class Core
    {
        private static ColorInt threatColorRaider = new ColorInt(Color.red);
        private static ColorInt threatColorAnimal = new ColorInt(Color.yellow);
        public static List<ThingDef> allLamps = new List<ThingDef>();
        public static MethodInfo UpdateLitWallLights => updateLitWallLights ??= AccessTools.Method("MURWallLight.CompOffsetGlow:UpdateLit");
        public static MethodInfo GetGlowColorWallLights => getGlowColorWallLights ??= AccessTools.Method("MURWallLight.CompOffsetGlow:get_GlowColor");
        public static MethodInfo SetGlowColorWallLights => setGlowColorWallLights ??= AccessTools.Method("MURWallLight.CompOffsetGlow:set_GlowColor");
        public static FieldInfo CompGlowerWallLights => compGlowerWallLights ??= AccessTools.Field("MURWallLight.CompOffsetGlow:compGlower");
                
        public static MethodInfo GetGlowColorGlowerExtended => getGlowColorGlowerExtended ??= AccessTools.Method("VanillaFurnitureExpanded.CompGlowerExtended:get_GlowColor");
        public static MethodInfo SetGlowColorGlowerExtended => setGlowColorGlowerExtended ??= AccessTools.Method("VanillaFurnitureExpanded.CompGlowerExtended:set_GlowColor");
        public static MethodInfo UpdateLitGlowerExtended => updateLitGlowerExtended ??= AccessTools.Method("VanillaFurnitureExpanded.CompGlowerExtended:UpdateLit");
        public static FieldInfo CompGlowerGlowerExtended => compGlowerGlowerExtended ??= AccessTools.Field("VanillaFurnitureExpanded.CompGlowerExtended:compGlower");

        private static MethodInfo updateLitWallLights;
        private static MethodInfo getGlowColorWallLights;
        private static MethodInfo setGlowColorWallLights;
        private static FieldInfo compGlowerWallLights;

        private static MethodInfo updateLitGlowerExtended;
        private static MethodInfo getGlowColorGlowerExtended;
        private static MethodInfo setGlowColorGlowerExtended;
        private static FieldInfo compGlowerGlowerExtended;

        static Core()
        {
            new Harmony("RedAlert").PatchAll();
            foreach (var def in DefDatabase<ThingDef>.AllDefs)
            {
                var props = def.GetCompProperties<CompProperties_Glower>();
                if (props != null && props.colorPickerEnabled)
                {
                    allLamps.Add(def);
                }
                var otherProps = def.comps?.FirstOrDefault(x => x.GetType().Name == "CompProperties_GlowOffset");
                if (otherProps != null)
                {
                    allLamps.Add(def);
                }
                var otherProps2 = def.comps?.FirstOrDefault(x => x.GetType().Name == "CompProperties_GlowerExtended");
                if (otherProps2 != null)
                {
                    var colorOptions = Traverse.Create(otherProps2).Field("colorOptions").GetValue() as IEnumerable;
                    foreach (var c in colorOptions)
                    {
                        var colorPickerEnabled = (bool)Traverse.Create(c).Field("colorPickerEnabled").GetValue();
                        if (colorPickerEnabled)
                        {
                            allLamps.Add(def);
                        }
                    }
                }
            }

            foreach (var lamp in allLamps)
            {
                lamp.comps ??= new List<CompProperties>();
                lamp.comps.Add(new CompProperties
                {
                    compClass = typeof(CompRedAlertToggleable)
                });
            }
        }

        public static ColorInt ThreatColorFor(IEnumerable<IAttackTarget> threats)
        {
            if (threats.Count() == 1)
            {
                var threat = threats.First();
                if (threat is Pawn pawn && pawn.RaceProps.Animal)
                {
                    return threatColorAnimal;
                }
            }
            return threatColorRaider;
        }

        public static IEnumerable<IAttackTarget> AllActiveThreats(Map map)
        {
            foreach (IAttackTarget item in map.attackTargetsCache.TargetsHostileToFaction(Faction.OfPlayer))
            {
                if (GenHostility.IsActiveThreatTo(item, Faction.OfPlayer))
                {
                    yield return item;
                }
            }
        }
    }
}
