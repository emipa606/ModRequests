using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;
using static System.Net.Mime.MediaTypeNames;

namespace DoctorStupid.PrettySkin
{
    [StaticConstructorOnStartup]
    public static class PrettySkin
    {
        static PrettySkin()
        {
            //append new descriptions
            if (ModsConfig.BiotechActive)
            {
                GeneDef d1 = DefDatabase<GeneDef>.GetNamed("Furskin", true);
                if (d1.label == "furskin")
                {
                    d1.description = d1.description + " Their fur is also able to be detached from their lifeless bodies and made into warm winter clothing.";
                }
                GeneDef d2 = DefDatabase<GeneDef>.GetNamed("FireResistant", true);
                if (d2.label == "fire resistant")
                {
                    d2.description = d2.description + " Carriers may also have their skin be used in the production of high-quality heat-resistant apparel.";
                }
            }
            //harmony patch
            var harmony = new Harmony("DoctorStupid.PrettySkin");
            harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch(nameof(Pawn.ButcherProducts))]
    static class Pawn_ButcherProducts
    {
        static IEnumerable<Thing> Postfix(IEnumerable<Thing> values, Thing __instance)
        {
            foreach (Thing thing in values)
            {
                if (__instance.def == ThingDefOf.Human)
                {
                    Pawn human = __instance as Pawn;
                    if (thing.def == DefDatabase<ThingDef>.GetNamed("Leather_Human"))
                    {
                        if (ModsConfig.BiotechActive)
                        {
                            if (human.genes != null)
                            {
                                bool hasFur = human.genes.HasGene(DefDatabase<GeneDef>.GetNamed("Furskin"));
                                bool hasHeat = human.genes.HasGene(DefDatabase<GeneDef>.GetNamed("FireResistant"));
                                if (hasFur)
                                {
                                    Thing newthing = ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("DoctorStupid_Leather_Human_Fur"), null);
                                    newthing.stackCount = thing.stackCount;
                                    thing.Destroy();
                                    newthing.SetColor(human.story.SkinColor);
                                    yield return newthing;
                                }
                                else if (hasHeat)
                                {
                                    Thing newthing = ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("DoctorStupid_Leather_Human_Heat"), null);
                                    newthing.stackCount = thing.stackCount;
                                    thing.Destroy();
                                    newthing.SetColor(human.story.SkinColor);
                                    yield return newthing;
                                }
                                else
                                {
                                    thing.SetColor(human.story.SkinColor);
                                    yield return thing;
                                }
                            }
                            else
                            {
                                thing.SetColor(human.story.SkinColor);
                                yield return thing;
                            }
                        }
                        else
                        {
                            thing.SetColor(human.story.SkinColor);
                            yield return thing;
                        }
                    }
                    else
                    {
                        yield return thing;
                    }
                }
                else
                {
                    yield return thing;
                }
            }
        }
    }

    [HarmonyPatch(typeof(ThoughtWorker_HumanLeatherApparel))]
    [HarmonyPatch(nameof(ThoughtWorker_HumanLeatherApparel.CurrentThoughtState))]
    static class ThoughtWorker_HumanLeatherApparel_CurrentThoughtState
    {
        static void Postfix(Pawn p, ref ThoughtState __result)
        {
            if (ModsConfig.BiotechActive)
            {
                int num2 = __result.StageIndex;
                num2++;
                if (num2 < 0)
                {
                    num2 = 0;
                }
                string text2 = __result.Reason;
                ThingDef fur = DefDatabase<ThingDef>.GetNamed("DoctorStupid_Leather_Human_Fur");
                ThingDef heat = DefDatabase<ThingDef>.GetNamed("DoctorStupid_Leather_Human_Heat");
                List<Apparel> wornApparel = p.apparel.WornApparel;
                for (int i = 0; i < wornApparel.Count; i++)
                {
                    if (wornApparel[i].Stuff == fur || wornApparel[i].Stuff == heat)
                    {
                        if (text2 == null)
                        {
                            text2 = wornApparel[i].def.label;
                        }
                        num2++;
                    }
                }
                if (num2 == 0)
                {
                    __result = ThoughtState.Inactive;
                }
                if (num2 >= 5)
                {
                    __result = ThoughtState.ActiveAtStage(4, text2);
                }
                __result = ThoughtState.ActiveAtStage(num2 - 1, text2);
            }
        }
    }
}