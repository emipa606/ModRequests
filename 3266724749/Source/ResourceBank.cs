using HarmonyLib;
using RimWorld;
using Verse;
using Settings = Tacticowl.ModSettings_Tacticowl;

namespace Tacticowl
{
    [StaticConstructorOnStartup]
    public static class ResourceBank
    {
        public static JobDef CastAbilityVerb;
        public static JobDef CastAbilitySelf;
        
        static ResourceBank()
        {
            CastAbilityVerb = DefDatabase<JobDef>.GetNamed("CastAbilityVerb", false);
            CastAbilitySelf = DefDatabase<JobDef>.GetNamed("CastAbilitySelf", false);
            if (CastAbilitySelf != null || CastAbilityVerb != null) Settings.usingJecsTools = true;


            if (AccessTools.TypeByName("Ammunition.Components.KitComponent") != null && AccessTools.TypeByName("Ammunition.Logic.AmmoLogic") != null && AccessTools.Method(AccessTools.TypeByName("Ammunition.Logic.AmmoLogic"), "AmmoCheck") != null) Settings.usingLTSAmmo = true;
        }

        [DefOf]
        public static class JobDefOf
        {
            public static JobDef DW_EquipOffHand;
        }
        [DefOf]
        public static class ConceptDefOf
        {
            public static ConceptDef DW_Penalties;
            public static ConceptDef DW_Settings;
        }
    }
}
