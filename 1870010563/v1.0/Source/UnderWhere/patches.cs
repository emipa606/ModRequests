using System.Reflection;
using System.Collections.Generic;
using Verse;
using RimWorld;
using Harmony;
using Verse.AI;

namespace UnderWhere
{
    [StaticConstructorOnStartup]
    static class HarmonyPatching
    {
        static HarmonyPatching()
        {
            var harmony = HarmonyInstance.Create("{rimworld.malistaticy.underwhere}");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    //[HarmonyPatch(typeof(ApparelUtility), "HasPartsToWear")]
    //public class HasPartsToWear_PrePatch_Gender
    //{
    //    [HarmonyPrefix]
    //    public static bool PreFix(ref bool __result, Pawn p, ThingDef apparel)
    //    {
    //        string genderlabel = p.GetGenderLabel();
    //        if (genderlabel != null)
    //        {
    //            List<string> list = apparel?.apparel?.tags;
    //            if ((list != null) && (list.Count > 0))
    //            {
    //                foreach (string tag in list)
    //                {
    //                    if (tag.StartsWith("OnlyGender_"))
    //                    {
    //                        if (tag != ("OnlyGender_" + genderlabel))
    //                        {
    //                            __result = false;
    //                            return false;
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //        return true;
    //    }
    //}

    [HarmonyPatch(typeof(PawnApparelGenerator), "CanUsePair")]
    public class CanUsePair_PostPatch_Gender
    {
        [HarmonyPostfix]
        public static void PostFix(ref bool __result, ThingStuffPair pair, Pawn pawn, float moneyLeft, bool allowHeadgear, int fixedSeed)
        {
            if (__result == true)
            {
                string genderlabel = pawn.GetGenderLabel();
                if (genderlabel != null)
                {
                    if (pair.thing?.apparel?.tags != null)
                    {
                        List<string> list = pair.thing.apparel.tags;
                        if ((list != null) && (list.Count > 0))
                        {
                            foreach (string tag in list)
                            {
                                if (tag.StartsWith("OnlyGender_"))
                                {
                                    if (tag != ("OnlyGender_" + genderlabel))
                                    {
                                        __result = false;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    [HarmonyPatch(typeof(Pawn_ApparelTracker), "HasBasicApparel")]
    public class HasBasicApparel_PostPatch_Waist
    {
        [HarmonyPostfix]
        public static void PostFix(out bool hasPants, ThingOwner<Apparel> ___wornApparel)
        {

            hasPants = false;
                for (int i = 0; i < ___wornApparel.Count; i++)
                {
                    Apparel apparel = ___wornApparel[i];
                    for (int j = 0; j < apparel.def.apparel.bodyPartGroups.Count; j++)
                    {
                        if (apparel.def.apparel.bodyPartGroups[j] == UWBodyPartGroupDefOf.Waist || apparel.def.apparel.bodyPartGroups[j] == BodyPartGroupDefOf.Legs)
                        {
                            hasPants = true;
                        }
                    }
                }
        }
    }
    [HarmonyPatch(typeof(JobGiver_PrisonerGetDressed), "TryGiveJob")]
    public class PrisonerGetDressed_PostPatch_Waist
    {
        static Apparel FindGarmentCoveringPart(Pawn pawn, BodyPartGroupDef bodyPartGroupDef)
        {
            Room room = pawn.GetRoom(RegionType.Set_Passable);
            if (room.isPrisonCell)
            {
                foreach (IntVec3 c in room.Cells)
                {
                    List<Thing> thingList = c.GetThingList(pawn.Map);
                    for (int i = 0; i < thingList.Count; i++)
                    {
                        Apparel apparel = thingList[i] as Apparel;
                        if (apparel != null && apparel.def.apparel.bodyPartGroups.Contains(bodyPartGroupDef) && pawn.CanReserve(apparel, 1, -1, null, false) && !apparel.IsBurning() && ApparelUtility.HasPartsToWear(pawn, apparel.def))
                        {
                            return apparel;
                        }
                    }
                }
            }
            return null;
        }
        [HarmonyPostfix]
        public static void PostFix(JobGiver_PrisonerGetDressed __instance, ref Job __result, Pawn pawn)
        {
            if (!pawn.apparel.BodyPartGroupIsCovered(UWBodyPartGroupDefOf.Waist))
            {
                Apparel apparel3 = FindGarmentCoveringPart(pawn, UWBodyPartGroupDefOf.Waist);
                if (apparel3 != null)
                {
                    __result = new Job(JobDefOf.Wear, apparel3)
                    {
                        ignoreForbidden = true
                    };
                }
            }
        }
    }
}
