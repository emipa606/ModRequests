using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace UniqueItems
{
    [StaticConstructorOnStartup]
    internal static class Core
    {
        public static Harmony harmony;
        static Core()
        {
            harmony = new Harmony("Morphium93.UniqueItems");
            harmony.PatchAll();
            ApplySettings(false);
        }
        public static void ApplySettings(bool afterSettings)
        {
            if (UniqueItemsSettings.filter is null)
            {
                UniqueItemsSettings.filter = new ThingFilter();
            }

            if (UniqueItemsSettings.allowedThings is null)
            {
                UniqueItemsSettings.allowedThings = new HashSet<string>();
            }

            if (!afterSettings)
            {
                foreach (var allowedDefName in UniqueItemsSettings.allowedThings)
                {
                    var thingDef = DefDatabase<ThingDef>.GetNamedSilentFail(allowedDefName);
                    //Log.Message("Allowed: " + allowedDefName + " - " + thingDef);
                    if (thingDef != null)
                    {
                        UniqueItemsSettings.filter.SetAllow(thingDef, true);
                    }
                }
            }

            foreach (var thingDef in UniqueItemsSettings.filter.AllowedThingDefs)
            {
                if (thingDef != null)
                {
                    if (thingDef.comps is null)
                    {
                        thingDef.comps = new List<CompProperties>();
                    }

                    if (thingDef.GetCompProperties<CompProperties_UniqueItem>() is null)
                    {
                        //Log.Message("Adding comp to " + thingDef);
                        thingDef.comps.Add(new CompProperties_UniqueItem
                        {
                            maxCount = 1
                        });
                    }
                }
            }

            foreach (var thingDef in DefDatabase<ThingDef>.AllDefs)
            {
                if (!UniqueItemsSettings.filter.AllowedThingDefs.Contains(thingDef))
                {
                    var props = thingDef.GetCompProperties<CompProperties_UniqueItem>();
                    if (props != null)
                    {
                        //Log.Message("Removing comp from " + thingDef);
                        thingDef.comps.Remove(props);
                    }

                    if (UniqueItemsSettings.allowedThings.Remove(thingDef.defName))
                    {
                        //Log.Message("Removing from allowedThings: " + thingDef.defName);
                    }

                    if (UniqueItemsSettings.uniqueThingsMaxCount.Remove(thingDef.defName))
                    {
                        //Log.Message("Removing from uniqueThingsMaxCount: " + thingDef.defName);
                    }
                }

                var compProps = thingDef.GetCompProperties<CompProperties_UniqueItem>();
                if (compProps != null)
                {
                    if (!UniqueItemsSettings.uniqueThingsMaxCount.ContainsKey(thingDef.defName))
                    {
                        //Log.Message("Adding to uniqueThingsMaxCount: " + thingDef.defName);
                        UniqueItemsSettings.uniqueThingsMaxCount[thingDef.defName] = compProps.maxCount;
                    }

                    if (!UniqueItemsSettings.allowedThings.Contains(thingDef.defName))
                    {
                        //Log.Message("Adding to allowedThings: " + thingDef.defName);
                        UniqueItemsSettings.allowedThings.Add(thingDef.defName);
                    }
                }
            }

            foreach (var thingDefName in UniqueItemsSettings.uniqueThingsMaxCount.Keys.ToList())
            {
                var thingDef = DefDatabase<ThingDef>.GetNamedSilentFail(thingDefName);
                if (thingDef != null)
                {
                    var compProps = thingDef.GetCompProperties<CompProperties_UniqueItem>();
                    if (compProps != null && UniqueItemsSettings.uniqueThingsMaxCount[thingDefName] != compProps.maxCount)
                    {
                        compProps.maxCount = UniqueItemsSettings.uniqueThingsMaxCount[thingDefName];
                    }
                }
                else
                {
                    UniqueItemsSettings.uniqueThingsMaxCount.Remove(thingDefName);
                }
            }
        }

        public static void RemoveUniqueItemsFrom(Pawn pawn)
        {
            var apparels = pawn.apparel?.WornApparel;
            if (apparels != null)
            {
                for (int num = apparels.Count - 1; num >= 0; num--)
                {
                    var apparel = apparels[num];
                    var comp = apparel.TryGetComp<CompUniqueItem>();
                    if (comp != null)
                    {
                        pawn.apparel.Remove(apparel);
                        apparel.Destroy();
                    }
                }
            }
            var equipments = pawn.equipment?.AllEquipmentListForReading;
            if (equipments != null)
            {
                for (int num = equipments.Count - 1; num >= 0; num--)
                {
                    var equipment = equipments[num];
                    var comp = equipment.TryGetComp<CompUniqueItem>();
                    if (comp != null)
                    {
                        pawn.equipment.Remove(equipment);
                        equipment.Destroy();
                    }
                }
            }
            var inventory = pawn.inventory;
            if (inventory != null)
            {
                for (int num = inventory.innerContainer.Count - 1; num >= 0; num--)
                {
                    var thing = inventory.innerContainer[num];
                    var comp = thing.TryGetComp<CompUniqueItem>();
                    if (comp != null)
                    {
                        pawn.inventory.innerContainer.Remove(thing);
                        thing.Destroy();
                    }
                }
            }
    
        }
    }

    [HarmonyPatch(typeof(KidnappedPawnsTracker), "Kidnap")]
    public class Kidnap_Patch
    {
        private static void Prefix(Pawn pawn, Pawn kidnapper)
        {
            Core.RemoveUniqueItemsFrom(pawn);
        }
    }
    
    [HarmonyPatch(typeof(Pawn), "PreTraded")]
    public class PreTraded_Patch
    {
        private static void Prefix(Pawn __instance, TradeAction action, Pawn playerNegotiator, ITrader trader)
        {
            if (action == TradeAction.PlayerSells)
            {
                Core.RemoveUniqueItemsFrom(__instance);
            }
        }
    }
    
    [HarmonyPatch(typeof(PawnBanishUtility), "Banish")]
    public class PawnBanishUtility_Patch
    {
        private static void Prefix(Pawn pawn, int tile = -1)
        {
            Core.RemoveUniqueItemsFrom(pawn);
        }
    }
    
    [HarmonyPatch(typeof(ThingWithComps), "SpawnSetup")]
    public class SpawnSetup_Patch
    {
        private static bool Prefix(ThingWithComps __instance)
        {
            var comp = __instance.TryGetComp<CompUniqueItem>();
            if (comp != null)
            {
                if (comp.MaxUniqueCountExceeded(__instance))
                {
                    CompUniqueItem.instances[__instance.def].Remove(__instance);
                    return false;
                }
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(ThingOwner<Thing>), "TryAdd", new Type[]
    {
        typeof(Thing),
        typeof(bool)
    })]
    internal static class ThingOwnerThing_TryAdd_Patch
    {
        private static bool Prefix(ThingOwner<Thing> __instance, bool __result, Thing item)
        {
            var comp = item.TryGetComp<CompUniqueItem>();
            if (comp != null)
            {
                if (comp.MaxUniqueCountExceeded(item))
                {
                    return false;
                }
                else
                {
                    comp.AddInstance();
                }
            }
            return true;
        }
    }
    
    [HarmonyPatch(typeof(Bill_Production), "ShouldDoNow")]
    public class ShouldDoNow_Patch
    {
        private static void Postfix(ref bool __result, Bill_Production __instance)
        {
            var compProps = __instance.recipe?.ProducedThingDef?.GetCompProperties<CompProperties_UniqueItem>();
            if (compProps != null)
            {
                var prop = compProps.compClass.GetProperty("Instances");
                var value = prop.GetValue(null) as Dictionary<ThingDef, HashSet<Thing>>;
                if (value != null && value.TryGetValue(__instance.recipe.ProducedThingDef, out var instances) && instances.Count >= compProps.maxCount)
                {
                    __result = false;
                }
            }
        }
    }
    
    [HarmonyPatch(typeof(StockGeneratorUtility), "TryMakeForStockSingle")]
    public class TryMakeForStockSingle_Patch
    {
        private static bool Prefix(ThingDef thingDef, int stackCount)
        {
            var compProps = thingDef.GetCompProperties<CompProperties_UniqueItem>();
            if (compProps != null)
            {
                var prop = compProps.compClass.GetProperty("Instances");
                var value = prop.GetValue(null) as Dictionary<ThingDef, HashSet<Thing>>;
                if (value != null && value.TryGetValue(thingDef, out var instances) && instances.Count >= compProps.maxCount)
                {
                    return false;
                }
            }
            return true;
        }
    }
    
    [StaticConstructorOnStartup]
    public static class GroupArrivalPatch
    {
        static GroupArrivalPatch()
        {
            var postfix = typeof(GroupArrivalPatch).GetMethod("GroupArrivalChecker");
            var baseType = typeof(PawnsArrivalModeWorker);
            var types = baseType.AllSubclassesNonAbstract();
            foreach (Type cur in types)
            {
                var method = cur.GetMethod("Arrive");
                try
                {
                    Core.harmony.Patch(method, null, new HarmonyMethod(postfix));
                }
                catch (Exception ex)
                {
                    Log.Error("Error patching " + cur + " - " + method);
                }
            }
        }
    
        public static void GroupArrivalChecker(List<Pawn> pawns, IncidentParms parms)
        {
            if (pawns != null && parms.target is Map map && parms.faction != null)
            {
                bool foundUnique = false;
                string rareOrUnique = "";
                Thing lastItem = null;
                string itemList = "";
                HashSet<Pawn> wearers = new HashSet<Pawn>();
                foreach (var pawn in pawns)
                {
                    if (pawn.Faction != Faction.OfPlayer)
                    {
                        var gears = new List<Thing>();
                        foreach (var gear in pawn.equipment.AllEquipmentListForReading)
                        {
                            gears.Add(gear);
                        }
                        if (pawn.apparel?.WornApparel != null)
                        {
                            foreach (var gear in pawn.apparel.WornApparel)
                            {
                                gears.Add(gear);
                            }
                        }

                        foreach (var gear in gears)
                        {
                            var comp = gear.TryGetComp<CompUniqueItem>();
                            if (comp != null)
                            {
                                rareOrUnique = gear.def.IsRare() ? "DM.Rare".Translate() : "DM.Unique".Translate();
                                foundUnique = true;
                                lastItem = gear;
                                itemList += pawn.LabelCap + " - " + gear.def.GetRareOrUniqueLabel() + "\n";
                                wearers.Add(pawn);
                            }
                        }
                    }
                }
                if (foundUnique)
                {
                    if (parms.faction.HostileTo(Faction.OfPlayer))
                    {
                        Find.LetterStack.ReceiveLetter("DM.NewUniqueItemAppeared".Translate(rareOrUnique).CapitalizeFirst(), "DM.NewUniqueItemAppearedEnemiesDesc"
                            .Translate(parms.faction.def.pawnsPlural, lastItem.def.GetRareOrUniqueLabel(), itemList, parms.faction.Named("FACTION")).ToString().TrimEndNewlines(), 
                            LetterDefOf.NeutralEvent, wearers.ToList());
                    }
                    else
                    {
                        Find.LetterStack.ReceiveLetter("DM.NewUniqueItemAppeared".Translate(rareOrUnique).CapitalizeFirst(), "DM.NewUniqueItemAppearedFriendliesDesc"
                        .Translate(parms.faction.def.pawnsPlural, lastItem.def.GetRareOrUniqueLabel(), itemList, parms.faction.Named("FACTION")).ToString().TrimEndNewlines(),
                        LetterDefOf.NeutralEvent, wearers.ToList());
                    }
                }
            }
        }
    }

    [StaticConstructorOnStartup]
    public static class GroupInhabitantPatch
    {
        static GroupInhabitantPatch()
        {
            var postfix = typeof(GroupInhabitantPatch).GetMethod("GroupHabitantChecker");
            var baseType = typeof(PawnGroupKindWorker);
            var types = baseType.AllSubclassesNonAbstract();
            foreach (Type cur in types)
            {
                var method = AccessTools.Method(cur, "GeneratePawns", new Type[]{ typeof(PawnGroupMakerParms), typeof(PawnGroupMaker), typeof(List<Pawn>), typeof(bool)});
                try
                {
                    Core.harmony.Patch(method, null, new HarmonyMethod(postfix));
                }
                catch (Exception ex)
                {
                    Log.Error("Error patching " + cur + " - " + method + " - " + ex);
                }
            }
        }

        public static void GroupHabitantChecker(PawnGroupMakerParms parms, PawnGroupMaker groupMaker, List<Pawn> outPawns, bool errorOnZeroResults = true)
        {
            if (parms != null && outPawns != null && parms.faction != null && parms.inhabitants)
            {
                bool foundUnique = false;
                string rareOrUnique = "";
                Thing lastItem = null;
                string itemList = "";
                HashSet<Pawn> wearers = new HashSet<Pawn>();
                foreach (var pawn in outPawns)
                {
                    if (pawn.Faction != Faction.OfPlayer)
                    {
                        var gears = new List<Thing>();
                        foreach (var gear in pawn.equipment.AllEquipmentListForReading)
                        {
                            gears.Add(gear);
                        }
                        if (pawn.apparel?.WornApparel != null)
                        {
                            foreach (var gear in pawn.apparel.WornApparel)
                            {
                                gears.Add(gear);
                            }
                        }

                        foreach (var gear in gears)
                        {
                            var comp = gear.TryGetComp<CompUniqueItem>();
                            if (comp != null)
                            {
                                rareOrUnique = gear.def.IsRare() ? "DM.Rare".Translate() : "DM.Unique".Translate();
                                foundUnique = true;
                                lastItem = gear;
                                itemList += pawn.LabelCap + " - " + gear.def.GetRareOrUniqueLabel() + "\n";
                                wearers.Add(pawn);
                            }
                        }
                    }
                }
                if (foundUnique)
                {
                    Find.LetterStack.ReceiveLetter("DM.NewUniqueItemAppeared".Translate(rareOrUnique).CapitalizeFirst(), "DM.NewUniqueItemAppearedInhabitantsDesc"
                    .Translate(parms.faction.def.pawnsPlural, lastItem.def.GetRareOrUniqueLabel(), itemList, parms.faction.Named("FACTION")).ToString().TrimEndNewlines(),
                    LetterDefOf.NeutralEvent, wearers.ToList());
                }
            }
        }
    }
    //[HarmonyPatch(typeof(Map), "FinalizeLoading")]
    //public static class FinalizeLoadingPatch
    //{
    //    public static bool isLoading;
    //    public static void Prefix(Map __instance)
    //    {
    //        isLoading = true;
    //    }
    //    public static void Postfix(Map __instance)
    //    {
    //        isLoading = false;
    //    }
    //}
    //
    //[HarmonyPatch(typeof(Map), "FinalizeInit")]
    //public static class MapGeneratedPatch
    //{
    //    public static void Postfix(Map __instance)
    //    {
    //        string itemList = "";
    //        string rareOrUnique = "";
    //
    //        foreach (var thing in __instance.listerThings.AllThings)
    //        {
    //            var comp = thing.TryGetComp<CompUniqueItem>();
    //            if (comp != null)
    //            {
    //                if (!comp.isNoticedByPlayer)
    //                {
    //                    itemList += thing.def.GetRareOrUniqueLabel() + "\n";
    //                    rareOrUnique = thing.def.IsRare() ? "DM.Rare".Translate() : "DM.Unique".Translate();
    //                }
    //            }
    //        }
    //
    //        if (itemList.Length > 0 && rareOrUnique.Length > 0)
    //        {
    //            Find.LetterStack.ReceiveLetter("DM.NewUniqueItemHidden".Translate(rareOrUnique), "DM.NewUniqueItemHiddenDesc".Translate(rareOrUnique, itemList), LetterDefOf.NeutralEvent);
    //        }
    //    }
    //}


    [HarmonyPatch(typeof(ThingStuffPair), "Commonality", MethodType.Getter)]
    public class Commonality_Patch
    {
        private static bool Prefix(ThingStuffPair __instance, ref float __result)
        {
            var compProps = __instance.thing.GetCompProperties<CompProperties_UniqueItem>();
            if (compProps != null)
            {
                var prop = compProps.compClass.GetProperty("Instances");
                var value = prop.GetValue(null) as Dictionary<ThingDef, HashSet<Thing>>;
                if (value != null && value.TryGetValue(__instance.thing, out var instances) && instances.Count >= compProps.maxCount)
                {
                    __result = 0f;
                    return false;
                }
            }
            return true;
        }
    }
    
    [HarmonyPatch(typeof(GenRecipe), "PostProcessProduct")]
    public static class PostProcessProduct_Patch
    {
        private static void Postfix(ref Thing __result, Thing product, RecipeDef recipeDef, Pawn worker)
        {
            if (__result != null && worker != null)
            {
                var comp = __result.TryGetComp<CompUniqueItem>();
                if (comp != null)
                {
                    comp.crafter = worker;
                    var thought = (Thought_UniqueItem)ThoughtMaker.MakeThought(UniqueItemsDefOf.DM_CraftedUniqueItem);
                    thought.uniqueItemDef = __result.def;
                    worker.needs?.mood?.thoughts?.memories?.TryGainMemory(thought);
                }
            }
        }
    }
    
    [HarmonyPatch(typeof(CompArt), "GenerateImageDescription")]
    public static class GenerateImageDescription_Patch
    {
        private static void Postfix(ref CompArt __instance, ref TaggedString __result)
        {
            var comp = __instance.parent.TryGetComp<CompUniqueItem>();
            if (comp != null)
            {
                __result += comp.Props.artDescriptionPostfix.Translate(__instance.AuthorName);
            }
        }
    }
}
