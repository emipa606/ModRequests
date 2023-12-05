using System;
using RimWorld;
using Verse;
using HarmonyLib;

namespace RimcuisineDrugsStandalone
{
    [StaticConstructorOnStartup]
    public class HarmonyPatches
    {
        public HarmonyPatches()
        {
            var harmony = new Harmony("RimcuisineDrugsStandalone");
            harmony.PatchAll();
            Log.Message("[RimcuisineDrugsStandalone] patched harmony assembly");

            //bruh moment: doesn't this part like, get called after def loading? The original mod could have probably gone without this patch but whatevzs
            /*foreach (DrugPolicy drugPolicy in Current.Game.drugPolicyDatabase.AllPolicies)
            {
                if (drugPolicy.label == Translator.Translate("SocialDrugs"))
                {
                }
            }*/
        }
    }
}

//the harmony code from the original mod
/*
static HarmonyPatches()
    {
        //IL_0006: Unknown result type (might be due to invalid IL or missing references)
        //IL_000c: Expected O, but got Unknown
        //IL_0034: Unknown result type (might be due to invalid IL or missing references)
        //IL_0040: Expected O, but got Unknown
        Harmony harmonyInstance = new Harmony("mehni.rimworld.rimCuisine.main");
        harmonyInstance.Patch((MethodBase)AccessTools.Method(typeof(Scenario), "PostGameStart", (Type[])null, (Type[])null), (HarmonyMethod)null, new HarmonyMethod(typeof(HarmonyPatches), "GenerateStartingDrugPolicies_PostFix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null);
    }

    private static void GenerateStartingDrugPolicies_PostFix()
    {
        //IL_002d: Unknown result type (might be due to invalid IL or missing references)
        foreach (DrugPolicy drugPolicy in Current.get_Game().drugPolicyDatabase.get_AllPolicies())
        {
            if (drugPolicy.label == TaggedString.op_Implicit(Translator.Translate("SocialDrugs")))
            {
                drugPolicy.get_Item(RCSSDefOf.RC2_Zope).allowedForJoy = true;
                drugPolicy.get_Item(RCSSDefOf.RC2_Cigar).allowedForJoy = true;
                drugPolicy.get_Item(RCSSDefOf.RC2_Cigarette).allowedForJoy = true;
            }
        }
    }

*/