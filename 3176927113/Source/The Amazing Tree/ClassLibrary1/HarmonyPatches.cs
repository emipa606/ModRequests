using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using The_Amazing_Tree;
using Verse;
using static UnityEngine.Random;

namespace The_Amazing_Tree_Harmony_Patches
{
    //Add Thing
    [HarmonyPatch(typeof(ListerThings), nameof(ListerThings.Add))]
    public static class AddCheck
    {
        public static void Postfix(object[] __args)
        {
           Thing t = __args[0] as Thing;    
            if (t == null)
            {
                return;
            }
            var map = t.Map;
            if (map == null)
            {
                return;
            }
            CompPsycastMedium comp = t.TryGetComp<CompPsycastMedium>();
            if(comp == null)
            {
                return;
            }
            var mapComp = map.GetComponent<MapComponent_PsyMediumManager>();
            if (mapComp == null)
            {
                return;
            }

            if (!mapComp.PsyMediumComps.Contains(comp))
            {
                mapComp.AddThing(t);
            }
        }
    }


    //Rimworld
    [HarmonyPatch(typeof(RimWorld.Verb_CastAbility), nameof(Verb_CastAbility.CanHitTarget))]
    public static class CanHitCheck1
    {
        public static void Postfix(ref bool __result, Verb_CastAbility __instance, object[] __args)
        {
            if (__result == true)
            {
                return;
            }
            LocalTargetInfo loc1 = (LocalTargetInfo)__args[0];
            var ability = __instance.ability;
            if (!ability.def.IsPsycast)
            {
                return;
            }
            Map map = ability.pawn.Map;
            var treeGlComp = map.GetComponent<MapComponent_PsyMediumManager>();
            if (treeGlComp == null)
            {
                return;
            }
            foreach (var item in treeGlComp.PsyMediumComps)
            {
                IntVec3 loc2 = item.parent.Position;
                var dir = loc1.Cell - loc2;
                int dist = dir.LengthHorizontalSquared;
                if (dist <= item.Sight * item.Sight && AmazingTreeUtility.CanUseMedium(item, __instance.CasterPawn))
                {
                    __result = true;
                    return;
                }
            }
        }
    }

    [HarmonyPatch(typeof(RimWorld.Verb_CastAbility), nameof(Verb_CastAbility.DrawHighlight))]
    public static class HighlightCheck
    {
        public static void Postfix(Verb_CastAbility __instance)
        {

            var ability = __instance.ability;
            if (!ability.def.IsPsycast)
            {
                return;
            }
            if (__instance.verbProps.range <= 0f)
            {
                return;
            }
            Map map = ability.pawn.Map;
            var treeGlComp = map.GetComponent<MapComponent_PsyMediumManager>();
            if (treeGlComp == null)
            {
                return;
            }
            foreach (var item in treeGlComp.PsyMediumComps)
            {
                if (AmazingTreeUtility.CanUseMedium(item, __instance.CasterPawn))
                    GenDraw.DrawRadiusRing(item.parent.Position, item.Sight);
            }
        }
    }

    [HarmonyPatch(typeof(RimWorld.Verb_CastAbility), nameof(Verb_CastAbility.TryStartCastOn))]
    public static class CanHitCheck2
    {
        public static void Postfix(ref bool __result, Verb_CastAbility __instance, object[] __args)
        {
            if (__result == true)
            {
                return;
            }
            LocalTargetInfo loc1 = (LocalTargetInfo)__args[0];
            LocalTargetInfo caster= (LocalTargetInfo)__args[1];
            var ability = __instance.ability;
            if (!ability.def.IsPsycast)
            {
                return;
            }
            Map map = ability.pawn.Map;
            var treeGlComp = map.GetComponent<MapComponent_PsyMediumManager>();
            if (treeGlComp == null)
            {
                return;
            }
            foreach (var item in treeGlComp.PsyMediumComps)
            {
                IntVec3 loc2 = item.parent.Position;
                var dir = loc1.Cell - loc2;
                int dist = dir.LengthHorizontalSquared;
                if (dist <= item.Sight * item.Sight && AmazingTreeUtility.CanUseMedium(item, __instance.CasterPawn))
                {
                    ability.Activate(loc1, loc2);
                    __result = true;
                    return;
                }
            }
        }


    }

    //VFECore
    [HarmonyPatch(typeof(VFECore.Abilities.Ability), nameof(VFECore.Abilities.Ability.DrawHighlight))]
    public static class HighlightCheck2
    {

        public static void Postfix(VFECore.Abilities.Ability __instance)
        {
            Map map = __instance.pawn.Map;
            var treeGlComp = map.GetComponent<MapComponent_PsyMediumManager>();
            if (treeGlComp == null)
            {
                return;
            }
            foreach (var item in treeGlComp.PsyMediumComps)
            {
                if (AmazingTreeUtility.CanUseMedium(item, __instance.pawn))
                    GenDraw.DrawRadiusRing(item.parent.Position, item.Sight);
            }
        }
    }

    [HarmonyPatch(typeof(VFECore.Abilities.Ability), nameof(VFECore.Abilities.Ability.CanHitTarget),
        new Type[] { typeof(LocalTargetInfo) })]
    public static class CanHitCheck4
    {
        public static void Postfix(ref bool __result, VFECore.Abilities.Ability __instance, object[] __args)
        {
            if (__result == true)
            {
                return;
            }
            LocalTargetInfo loc1 = (LocalTargetInfo)__args[0];
            Map map = __instance.pawn.Map;
            var treeGlComp = map.GetComponent<MapComponent_PsyMediumManager>();
            if (treeGlComp == null)
            {
                return;
            }
            foreach (var item in treeGlComp.PsyMediumComps)
            {
                IntVec3 loc2 = item.parent.Position;
                var dir = loc1.Cell - loc2;
                int dist = dir.LengthHorizontalSquared;
                if (dist <= item.Sight * item.Sight && AmazingTreeUtility.CanUseMedium(item, __instance.CasterPawn))
                {
                    __result = true;
                    return;
                }
            }
        }


    }


    //Meditaion Patch
    [HarmonyPatch(typeof(Pawn_PsychicEntropyTracker),nameof(Pawn_PsychicEntropyTracker.Notify_Meditated))]
    public static class MedicationPatch
    {
        public static void Postfix(Pawn_PsychicEntropyTracker __instance)
        {
            int num = GenRadial.NumCellsInRadius(MeditationUtility.FocusObjectSearchRadius);
            for (int i = 0; i < num; i++)
            {
                IntVec3 c = __instance.Pawn.Position + GenRadial.RadialPattern[i];
                Map map = __instance.Pawn.Map;
                if(map == null)
                {
                    return;
                }
                var globalComp = map.GetComponent<MapComponent_PsyMediumManager>();
                if (c.InBounds(__instance.Pawn.Map))
                {
                    var comps= globalComp.PsyMediumComps;
                    foreach( var comp in comps ) 
                    {
                        if (comp.parent.Position != c)
                        {
                            continue;
                        }
                        comp.AddPower();
                    }
                   
                }
            }
        }
    }
}







