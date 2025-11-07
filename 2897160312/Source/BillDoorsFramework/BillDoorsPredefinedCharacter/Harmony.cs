
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BillDoorsPredefinedCharacter
{
    [UsedImplicitly]
    [StaticConstructorOnStartup]
    public class PatchMain
    {
        private static Harmony harmonyInstance;

        internal static Harmony HarmonyInstance
        {
            get
            {
                if (harmonyInstance == null)
                {
                    harmonyInstance = new Harmony("BD_PredefChar");
                }
                return harmonyInstance;
            }
        }

        static PatchMain()
        {
            HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(PawnGenerator), "AdjustXenotypeForFactionlessPawn")]
    static class AdjustXenotypeForFactionlessPawn_PostFix
    {
        [HarmonyPostfix]
        static void PostFix(ref PawnGenerationRequest request)
        {
            if (request.ForcedCustomXenotype != null)
            {
                foreach (var gene in request.ForcedCustomXenotype.genes)
                {
                    if (gene.HasModExtension<ModExtension_PreventCustomXenotypeNaturalGen>())
                    {
                        request.ForcedCustomXenotype = null;
                        return;
                    }
                }
            }
        }
    }

    public class ModExtension_PreventCustomXenotypeNaturalGen : DefModExtension
    {

    }

    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(PawnRenderTree), "SetupDynamicNodes")]
    static class SetupDynamicNodes_Postfix
    {
        static MethodInfo AddChild => typeof(PawnRenderTree).GetMethod("AddChild", BindingFlags.Instance | BindingFlags.NonPublic);

        [HarmonyPrefix]
        static void Postfix(PawnRenderTree __instance)
        {
            try
            {
                if (BDPDC_Mod.PawnPDCPair?.NullOrEmpty() ?? true) return;
                if (BDPDC_Mod.PawnPDCPair.TryGetValue(__instance.pawn, out var def))
                {
                    foreach (PawnRenderNodeProperties renderNodeProperty in def.renderNodeProperties)
                    {
                        PawnRenderNode pawnRenderNode = (PawnRenderNode)Activator.CreateInstance(renderNodeProperty.nodeClass, __instance.pawn, renderNodeProperty, __instance);
                        AddChild.Invoke(__instance, new object[] { pawnRenderNode, null });
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"{ex.Message}:{ex.StackTrace}");
            }
        }
    }

    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(WorldPawnGC), "GetCriticalPawnReason")]
    static class WorldPawnGC_GetCriticalPawnReason_Postfix
    {
        [HarmonyPostfix]
        static void Postfix(Pawn pawn, ref string __result)
        {
            if (__result == null) return;
            try
            {
                if (BDPDC_Mod.PawnPDCPair?.NullOrEmpty() ?? true) return;
                if (BDPDC_Mod.PawnPDCPair.TryGetValue(pawn, out var def))
                {
                    if (def.preventDiscard) __result = "BDPDC_Prevented";
                }
            }
            catch (Exception ex)
            {
                Log.Error($"{ex.Message}:{ex.StackTrace}");
            }
        }
    }

    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(AnomalyUtility), "TryDuplicatePawn_NewTemp")]
    static class TryDuplicatePawn_NewTemp_Prefix
    {
        static PredefinedCharacterParmDef defCache;

        [HarmonyPrefix]
        static bool Prefix(Pawn originalPawn, out bool __result)
        {
            defCache = null;
            __result = false;
            try
            {
                if (BDPDC_Mod.PawnPDCPair?.NullOrEmpty() ?? true) return true;
                if (BDPDC_Mod.PawnPDCPair.TryGetValue(originalPawn, out var def))
                {
                    defCache = def;
                    if (!def.allowDuplicate) return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"{ex.Message}:{ex.StackTrace}");
            }
            return true;
        }

        [HarmonyPostfix]
        static void Postfix(Pawn duplicatePawn)
        {
            if (duplicatePawn == null) return;
            if (defCache == null) return;
            try
            {
                BDPDC_Mod.PawnPDCPair.Add(duplicatePawn, defCache);
            }
            catch (Exception ex)
            {
                Log.Error($"{ex.Message}:{ex.StackTrace}");
            }
        }
    }
}
