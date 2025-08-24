using HarmonyLib;
using RimWorld;
using StuffableCore.SCComps;
using StuffableCore.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace StuffableCore.SCPatches
{

    internal static class RecipeWorker_Harmony_Patch
    {
        

        public static void ApplyOnPawnPostfix(RecipeWorker __instance, Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            if (__instance.recipe?.addsHediff == null) return;
            var hediff = pawn.health.hediffSet.hediffs.LastOrDefault(x => x is HediffWithComps && x.def == __instance.recipe.addsHediff); // get the hediff we just added
            if (hediff == null) return;
            var stuffableComp = hediff.TryGetComp<HediffCompStuffable>();
            if (stuffableComp == null) return;
            ThingDef stuff = ThingDefOf.Steel;
            ThingDef thingDef = hediff.def.spawnThingOnRemoved;
            if (thingDef == null) return;
            if (SCMod.settings.ImplantProstheticSettings.enabled)
            {
                var list = SCMod.settings.ImplantProstheticSettings.GetEnabledIngredientsForEnabledCategories();
                if (!list.NullOrEmpty())
                    stuff = list.RandomElement();
            }
            if (SCMod.settings.EditorSettings.ThingDefSettingsCache.TryGetValue(thingDef.defName, out var scs) && scs != null && scs.enabled)
            {
                var list = scs.GetEnabledIngredientsForEnabledCategories();
                if (!list.NullOrEmpty())
                    stuff = list.RandomElement();
            }
            stuffableComp.stuff = stuff;
        }
    }
}
