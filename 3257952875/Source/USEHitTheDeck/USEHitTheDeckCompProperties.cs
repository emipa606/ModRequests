using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using USEHitTheDeck;
using Verse;
using Verse.AI;

namespace USEUtilityButton
{
    public class USEHitTheDeckCompProperties : CompProperties
    {
        public USEHitTheDeckCompProperties()
        {
            this.compClass = typeof(USEHitTheDeckComp);
        }

        public USEHitTheDeckCompProperties(Type compClass) : base(compClass)
        {
            this.compClass = compClass;
        }
    }

    public class USEHitTheDeckComp : ThingComp
    {
        public USEHitTheDeckCompProperties Props => (USEHitTheDeckCompProperties)this.Props;
        public bool isCrawling = false;
        public PawnPosture posture = PawnPosture.Standing;

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (this.parent.Faction != Faction.OfPlayer && USEHitTheDeckConfigModSetting.onlyAppliedPlayerFactionPawn)
            {
                yield break;
            }
            if (this.parent is Pawn p && (p.RaceProps.Animal || p.RaceProps.IsMechanoid) && USEHitTheDeckConfigModSetting.excludeAnimalAndMech)
            {
                yield break;
            }

            Command_Toggle crawlingToggle = new Command_Toggle();
            crawlingToggle.defaultDesc = "Toggle Crawling State";
            crawlingToggle.defaultLabel = "Crawling";
            crawlingToggle.isActive = () => this.isCrawling;
            crawlingToggle.toggleAction = new Action(delegate ()
            {
                isCrawling = !isCrawling;

                if (isCrawling)
                {
                    posture = PawnPosture.LayingOnGroundNormal;
                }
                else
                {
                    posture = PawnPosture.Standing;
                }
                //foreach (Pawn pawn in Find.Selector.SelectedPawns)
                //{
                //    USEFocusedJobComp comp = pawn.TryGetComp<USEFocusedJobComp>();

                //    if (comp != null)
                //    {
                //        comp.;
                //    }
                //}
            });
            crawlingToggle.hotKey = USEKeyBindingHitTheDeckDefOf.USEHitTheDeckKeyBinding;

            crawlingToggle.icon = TexButton.ReorderDown;

            yield return crawlingToggle;
            //yield return posture;

        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref isCrawling, "USEIsCrawling");
            Scribe_Values.Look(ref posture, "USEPosture");
        }
    }


    [HarmonyPatch(typeof(Pawn), nameof(Pawn.Crawling), MethodType.Getter)]
    public static class CrawlingPatch
    {
        [HarmonyPostfix]
        public static void Patch_0(Pawn __instance, ref bool __result)
        {
            if (__instance.GetComp<USEHitTheDeckComp>().isCrawling)
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(Pawn), nameof(Pawn.TicksPerMove))]
    public static class CrawlingToggleMoveSpeedPatch
    {
        public static bool CheckCrawlingToggleOn(Pawn pawn)
        {
            if (pawn != null && pawn.TryGetComp<USEHitTheDeckComp>() != null)
            {
                return pawn.TryGetComp<USEHitTheDeckComp>().isCrawling;
            }
            else
            {
                return false;
            }
        }

        public static MethodInfo checkCrawlingToggleOn_MethodInfo = typeof(CrawlingToggleMoveSpeedPatch).GetMethod("CheckCrawlingToggleOn");

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Patch_0(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var codes = instructions.ToList();

            Label label = il.DefineLabel();
            for (int i = 0; i < codes.Count; ++i)
            {
                if (i + 1 < codes.Count &&
                    codes[i + 1].opcode == OpCodes.Ldsfld && (FieldInfo)codes[i + 1].operand == typeof(StatDefOf).GetField(nameof(StatDefOf.CrawlSpeed), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance) &&
                    codes[i].opcode == OpCodes.Ldarg_0)
                {
                    codes[i].labels.Add(label);
                    break;
                }
            }

            for (int i = 0; i < codes.Count; ++i)
            {
                if (i + 2 < codes.Count &&
                    codes[i + 1].opcode == OpCodes.Call && (MethodInfo)codes[i + 1].operand == typeof(Pawn).GetProperty(nameof(Pawn.Downed), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetMethod &&
                    codes[i + 2].opcode == OpCodes.Brfalse_S)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, checkCrawlingToggleOn_MethodInfo);
                    yield return new CodeInstruction(OpCodes.Brtrue, label);
                    yield return codes[i];
                }
                else
                {
                    yield return codes[i];
                }
            }
        }
    }

    [HarmonyPatch(typeof(RimWorld.PawnUtility), nameof(RimWorld.PawnUtility.GetPosture))]
    public static class PawnPosturePatch
    {
        [HarmonyPostfix]
        public static void Patch(Pawn p, ref PawnPosture __result)
        {
            USEHitTheDeckComp hitTheDeckComp = p.TryGetComp<USEHitTheDeckComp>();

            if (hitTheDeckComp != null && hitTheDeckComp.posture != PawnPosture.Standing)
            {
                __result = hitTheDeckComp.posture;
            }
        }
    }

    //[HarmonyPatch(typeof(Verb), nameof(Verb.ValidateTarget))]
    //public static class CanNotShootWhileThereIsObstacleWhenCrawlling
    //{
    //    [HarmonyPostfix]
    //    public static void Patch(Verb __instance, ref bool __result, LocalTargetInfo target, bool showMessages)
    //    {
    //        if (USEHitTheDeckConfigModSetting.canNotShootWhileThereIsObstacle)
    //        {
    //            USEHitTheDeckComp hitTheDeckComp = __instance.CasterPawn.TryGetComp<USEHitTheDeckComp>();

    //            if (hitTheDeckComp != null)
    //            {
    //                if (hitTheDeckComp.isCrawling && __instance.CasterPawn.Position.GetCover(__instance.CasterPawn.Map) != null)
    //                {
    //                    __result = false;
    //                }
    //            }
    //        }
    //    }
    //}

    [DefOf]
    public static class USEKeyBindingHitTheDeckDefOf
    {
        public static KeyBindingDef USEHitTheDeckKeyBinding;
    }
}
