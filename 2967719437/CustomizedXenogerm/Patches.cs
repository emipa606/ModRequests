using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace CustomizedXenogerm
{
    [StaticConstructorOnStartup]
    class Main
    {
        static Main()
        {
            var harmony = new Harmony("com.github.harmony.rimworld.maarx.customizedxenogerm");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [HarmonyPatch(typeof(Building_GeneAssembler))]
    [HarmonyPatch("GetGizmos")]
    class Patch_Building_GeneAssembler_GetGizmos
    {
        static void Postfix(Building_GeneAssembler __instance, ref IEnumerable<Gizmo> __result)
        {
            List<Gizmo> __newResult = new List<Gizmo>();
            string targetLabel = "Recombine".Translate() + "...";
            foreach (Gizmo g in __result)
            {
                if (g is Command_Action)
                {
                    Command_Action ca = g as Command_Action;
                    if (ca.defaultLabel == targetLabel)
                    {
                        Command_Action command_Action = new Command_Action();
                        command_Action.defaultLabel = "Recombine".Translate() + "...";
                        command_Action.defaultDesc = "RecombineDesc".Translate();
                        command_Action.icon = new CachedTexture("UI/Gizmos/RecombineGenes").Texture;
                        command_Action.action = delegate
                        {
                            Find.WindowStack.Add(new Dialog_CreateCustomizedXenogerm(__instance));
                        };
                        __newResult.Add(command_Action);
                    }
                    else
                    {
                        __newResult.Add(g);
                    }
                }
                else
                {
                    __newResult.Add(g);
                }
            }
            __result = __newResult;
        }
    }

    [HarmonyPatch(typeof(Dialog_CreateXenogerm))]
    [HarmonyPatch("StartAssembly")]
    class Patch_Dialog_CreateXenogerm_StartAssembly
    {
        static void Postfix(Dialog_CreateXenogerm __instance)
        {
            ((Dialog_CreateCustomizedXenogerm)__instance).PropagateCustomPawn();
        }
    }

    [HarmonyPatch(typeof(Building_GeneAssembler))]
    [HarmonyPatch("Reset")]
    class Patch_Building_GeneAssembler_Reset
    {
        static void Postfix(Building_GeneAssembler __instance)
        {
            __instance.TryGetComp<CustomizedXenogermComp>().customPawn = null;
        }
    }

    [HarmonyPatch(typeof(Building_GeneAssembler))]
    [HarmonyPatch("Finish")]
    class Patch_Building_GeneAssembler_Finish
    {
        static bool Prefix(Building_GeneAssembler __instance)
        {
            List<Genepack> genepacksToRecombine = (List<Genepack>)__instance.GetType().GetField("genepacksToRecombine", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            if (!genepacksToRecombine.NullOrEmpty())
            {
                SoundDefOf.GeneAssembler_Complete.PlayOneShot(SoundInfo.InMap(__instance));
                Xenogerm xenogerm = (Xenogerm)ThingMaker.MakeThing(ThingDefOf.Xenogerm);
                xenogerm.Initialize(genepacksToRecombine, __instance.xenotypeName, __instance.iconDef);
                // INTENDED CHANGE: Propagate the customPawn from the GeneAssembler to the Xenogerm
                xenogerm.TryGetComp<CustomizedXenogermComp>().customPawn = __instance.TryGetComp<CustomizedXenogermComp>().customPawn;
                if (GenPlace.TryPlaceThing(xenogerm, __instance.InteractionCell, __instance.Map, ThingPlaceMode.Near))
                {
                    Messages.Message("MessageXenogermCompleted".Translate(), xenogerm, MessageTypeDefOf.PositiveEvent);
                }
            }
            // NEW LOGIC: Make "genepacksToRecombine.NullOrEmpty()" truthy so we can pass it back to original for the remainder without re-executing the above.
            __instance.GetType().GetField("genepacksToRecombine", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, null);
            return true;
        }
    }
}
