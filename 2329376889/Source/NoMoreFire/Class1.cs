using HarmonyLib;
using UnityEngine;
using Verse;

namespace NoMoreFire
{
    public class KdSettings : ModSettings
    {
        public bool disableFoodPoisoning;
        public bool disableFire;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref disableFoodPoisoning, "kd8lvt_NMF_DisableFoodPosioning", false);
            Scribe_Values.Look(ref disableFire, "kd8lvt_NMF_DisableFire", true);
            base.ExposeData();
        }
    }

    [HarmonyPatch(typeof(RimWorld.Fire))]
    [HarmonyPatch(nameof(RimWorld.Fire.Tick))]
    public class Fire_Tick_Patch
    {
        static bool Prefix(ref RimWorld.Fire __instance)
        {
            if (LoadedModManager.GetMod<NoMoreFire>().GetSettings<KdSettings>().disableFire)
            {
                __instance.Destroy();
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Verse.Pawn))]
    [HarmonyPatch(nameof(Verse.Pawn.Tick))]
    public class Food_Poison_Patch
    {
        static bool Prefix(ref Verse.Pawn __instance)
        {
            if (LoadedModManager.GetMod<NoMoreFire>().GetSettings<KdSettings>().disableFoodPoisoning)
            {
                    if (__instance.health.hediffSet.HasHediff(DefDatabase<HediffDef>.GetNamed("FoodPoisoning")))
                    {
                        HediffDef poisoningDef = DefDatabase<HediffDef>.GetNamed("FoodPoisoning");
                        __instance.health.RemoveHediff(__instance.health.hediffSet.GetFirstHediffOfDef(poisoningDef));
                    }
            }
            return true;
        }
    }
    [StaticConstructorOnStartup]
    public static class NoMoreFirePatcher
    {
        static NoMoreFirePatcher()
        {
            Harmony harmony = new Harmony("kd8lvt_nomorefire");
            harmony.PatchAll();
        }
    }

    public class NoMoreFire : Mod
    {
        KdSettings settings;

        public NoMoreFire(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<KdSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("Disable Fire", ref settings.disableFire, "When checked, Fire will be disabled.");
            listingStandard.CheckboxLabeled("Disable Food Poisoning", ref settings.disableFoodPoisoning, "When checked, Food-poisoning will be disabled.");
            listingStandard.End();

            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory() => "No More Fire";
    }

}
