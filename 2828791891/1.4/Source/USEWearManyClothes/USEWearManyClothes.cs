using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Verse;
using RimWorld;
using Verse.AI;
using HarmonyLib;
using UnityEngine;

namespace USEWearManyClothes
{
    [StaticConstructorOnStartup]
    static class USEWearManyClothes
    {
        static USEWearManyClothes()
        {
            var harmony = new Harmony("use.wearmanyclothes");

            harmony.PatchAll(Assembly.GetExecutingAssembly());

            DefsLoaded();
        }
        public static void DefsLoaded()
        {
            IEnumerable<ThingDef> things = (
                    from def in DefDatabase<ThingDef>.AllDefs
                    where def.race != null && def.race.intelligence == Intelligence.Humanlike
                    select def
                );

            foreach (ThingDef thingDef in things)
            {
                thingDef.comps.Add(new USEWearManyCompProperties());
            }
        }
    }

    public class USEWearManyCompProperties : CompProperties
    {
        public USEWearManyCompProperties()
        {
            this.compClass = typeof(USEWearManyComp);
        }

        public USEWearManyCompProperties(Type compClass) : base(compClass)
        {
            this.compClass = compClass;
        }
    }

    public class USEWearManyComp : ThingComp
    {
        public USEWearManyCompProperties Props => (USEWearManyCompProperties)this.Props;

        public bool USEwearAsManyEnabled = false;

        public USEWearManyComp()
        {
        }

        // comp 단위 
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            Command_Toggle wearAsManyAsButton = new Command_Toggle();

            wearAsManyAsButton.defaultLabel = "Wear Many";
            wearAsManyAsButton.defaultDesc = "Wear many as we want.";
            wearAsManyAsButton.icon = TexCommand.DesirePower;
            //wearAsManyAsButton.shrinkable = true;
            //stopButton.hotKey = USEStopButtonKeyBindingDefOf.USEStopButtonKeyBinding;
            wearAsManyAsButton.isActive = delegate
            {
                return USEwearAsManyEnabled;
            };
            wearAsManyAsButton.toggleAction = delegate
            {
                USEwearAsManyEnabled = !USEwearAsManyEnabled;
                Pawn pawn = this.parent as Pawn;
            };

            yield return wearAsManyAsButton;

            foreach (Gizmo g in base.CompGetWornGizmosExtra())
            {
                yield return g;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<bool>(ref USEwearAsManyEnabled, "USEwearAsManyEnabled");
        }
    }



    [HarmonyPatch(typeof(JobDriver_Wear), "TryUnequipSomething")]
    public static class USEWearManyClothesUnequipPatch
    {
        public class PassingData
        {
            public List<Apparel> apparelList;
        }

        [HarmonyPrefix]
        public static bool PrefixPatch(JobDriver_Wear __instance, out PassingData __state)
        {
            __state = new PassingData();
            __state.apparelList = __instance.pawn.apparel.WornApparel.ListFullCopy();

            __instance.pawn.apparel.WornApparel.Clear();

            return true;
        }

        [HarmonyPostfix]
        public static void PostfixPatch(JobDriver_Wear __instance, PassingData __state)
        {
            foreach (var a in __state.apparelList.ListFullCopy())
            {
                __instance.pawn.apparel.WornApparel.Add(a);
            }
        }
    }

    [HarmonyPatch(typeof(RimWorld.Pawn_ApparelTracker), nameof(RimWorld.Pawn_ApparelTracker.Wear))]
    public static class USEWearManyClothesPatch
    {
        public class PassingData
        {
            public List<Apparel> apparelList = new List<Apparel>();
            public List<Apparel> forcedApparelList = new List<Apparel>();
            public bool forbidden = false;
        }

        public static FieldInfo wornApparelField = AccessTools.Field(typeof(RimWorld.Pawn_ApparelTracker), "wornApparel");

        [HarmonyPrefix]
        public static bool WearPrefixPatch1(RimWorld.Pawn_ApparelTracker __instance, out PassingData __state, Apparel newApparel, bool dropReplacedApparel = true, bool locked = false)
        {
            __state = new PassingData();

            if (__instance.pawn.GetComp<USEWearManyComp>() != null && __instance.pawn.GetComp<USEWearManyComp>().USEwearAsManyEnabled == true)
            {
                ThingOwner<Apparel> wornApparelInstance = wornApparelField.GetValue(__instance) as ThingOwner<Apparel>;

                __state.apparelList = __instance.WornApparel.ListFullCopy();
                if (__instance.pawn.outfits != null && __instance.pawn.outfits.forcedHandler != null)
                {
                    __state.forcedApparelList = __instance.pawn.outfits.forcedHandler.ForcedApparel.ListFullCopy();
                }

                if (wornApparelInstance != null)
                {
                    wornApparelInstance.Clear();
                }
            }

            return true;
        }

        [HarmonyPostfix]
        public static void WearPostfixPatch1(RimWorld.Pawn_ApparelTracker __instance, PassingData __state, Apparel newApparel, bool dropReplacedApparel = true, bool locked = false)
        {
            if (__instance.pawn.GetComp<USEWearManyComp>() != null && __instance.pawn.GetComp<USEWearManyComp>().USEwearAsManyEnabled == true)
            {
                ThingOwner<Apparel> wornApparelInstance = wornApparelField.GetValue(__instance) as ThingOwner<Apparel>;

                //wornApparelInstance.Clear();

                foreach (Apparel apparel in __state.apparelList)
                {
                    wornApparelInstance.TryAddOrTransfer(apparel.SplitOff(apparel.stackCount), false);
                }

                if (__state.forcedApparelList != null)
                {
                    foreach (Apparel apparel in __state.forcedApparelList)
                    {
                        __instance.pawn.outfits.forcedHandler.SetForced(apparel, true);
                    }
                }

                //wornApparelInstance.TryAddOrTransfer(newApparel.SplitOff(newApparel.stackCount), false);
            }
        }
    }

    //[HarmonyPatch(typeof(ApparelUtility))]
    //[HarmonyPatch(nameof(ApparelUtility.HasPartsToWear))]
    //static class Patch_HasPartsToWear
    //{
    //    static bool Prefix(ref bool __result, Pawn p, ThingDef apparel)
    //    {
    //        //if (!HarmonyStart.Instance.ProsthesesFixEnabled)
    //        //{
    //        //    return true;
    //        //}

    //        __result = HasPartsToWear(p, apparel);
    //        return false;
    //    }

    //    public static bool HasPartsToWear(Pawn p, ThingDef apparel)
    //    {
    //        List<Hediff> hediffs = p.health.hediffSet.hediffs;
    //        bool flag = false;
    //        for (int index = 0; index < hediffs.Count; ++index)
    //        {
    //            if (hediffs[index] is Hediff_MissingPart)
    //            {
    //                flag = true;
    //                break;
    //            }
    //        }
    //        if (!flag)
    //            return true;

    //        IEnumerable<BodyPartRecord> notMissingParts = GetNotMissingParts(p.health.hediffSet);
    //        List<BodyPartGroupDef> groups = apparel.apparel.bodyPartGroups;
    //        for (int i = 0; i < groups.Count; ++i)
    //        {
    //            if (notMissingParts.Any(x => x.IsInGroup(groups[i])))
    //                return true;
    //        }
    //        return false;
    //    }

    //    public static IEnumerable<BodyPartRecord> GetNotMissingParts(HediffSet hediffSet)
    //    {
    //        List<BodyPartRecord> allPartsList = hediffSet.pawn.def.race.body.AllParts;
    //        for (int i = 0; i < allPartsList.Count; i++)
    //        {
    //            BodyPartRecord part = allPartsList[i];
    //            if (!PartIsMissing(hediffSet, part))
    //            {
    //                yield return part;
    //            }
    //        }
    //        yield break;
    //    }

    //    public static bool PartIsMissing(HediffSet hediffSet, BodyPartRecord part)
    //    {
    //        foreach (var h in hediffSet.hediffs)
    //        {
    //            if (h.Part == part &&
    //                h is Hediff_MissingPart &&
    //                !hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(part))
    //            {
    //                return true;
    //            }
    //        }
    //        return false;
    //    }
    //}

    //[HarmonyPatch(typeof(ApparelUtility))]
    //[HarmonyPatch(nameof(ApparelUtility.CanWearTogether))]
    //static class Patch_CanWearTogether
    //{
    //    static bool Prefix(ref bool __result, ThingDef A, ThingDef B, BodyDef body)
    //    {
    //        __result = CanWearTogether(A, B, body);
    //        return false;
    //    }

    //    public static bool CanWearTogether(ThingDef A, ThingDef B, BodyDef body)
    //    {
    //        bool flag = false;
    //        for (int index1 = 0; index1 < A.apparel.layers.Count; ++index1)
    //        {
    //            for (int index2 = 0; index2 < B.apparel.layers.Count; ++index2)
    //            {
    //                if (A.apparel.layers[index1] == B.apparel.layers[index2])
    //                    flag = true;
    //                if (flag)
    //                    break;
    //            }
    //            if (flag)
    //                break;
    //        }
    //        if (!flag)
    //            return true;

    //        //foreach (var a in A.apparel.bodyPartGroups)
    //        //{
    //        //    foreach (var b in B.apparel.bodyPartGroups)
    //        //    {
    //        //        foreach (var all in body.AllParts)
    //        //        {
    //        //            if (all.groups.Contains(a) && all.groups.Contains(b))
    //        //                return false;
    //        //        }
    //        //    }
    //        //}

    //        return true;
    //    }
    //}
}