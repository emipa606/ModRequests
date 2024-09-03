using Dark.Cloning;
using HarmonyLib;
using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace BiotechCloningHARPatch
{
    public class BiotechCloningHARPatchMod : Mod
    {
        public BiotechCloningHARPatchMod(ModContentPack pack) : base(pack)
        {
            new Harmony("BiotechCloningHARPatchMod").PatchAll();
        }
    }

    [HarmonyPatch(typeof(Patch_PregnancyUtility_ApplyBirthOutcome), "GeneratePawn")]
    public static class GeneratePawn_Patch
    {
        public static ThingDef originalRace;
        public static PawnKindDef changedKindDef;
        public static void Postfix(PawnGenerationRequest __0)
        {
            if (Patch_PregnancyUtility_ApplyBirthOutcome.staticCloneData != null)
            {
                changedKindDef = __0.KindDef;
                originalRace = changedKindDef.race;
                changedKindDef.race = Patch_PregnancyUtility_ApplyBirthOutcome.staticCloneData.donorPawn.def;
            }
        }
    }

    [HarmonyPatch(typeof(Patch_PregnancyUtility_ApplyBirthOutcome), "Postfix")]
    public static class Postfix_Patch
    {
        public static void Postfix(ref Thing __0)
        {
            if (GeneratePawn_Patch.originalRace != null && GeneratePawn_Patch.changedKindDef != null)
            {
                GeneratePawn_Patch.changedKindDef.race = GeneratePawn_Patch.originalRace;
            }
        }
    }

    [HarmonyPatch(typeof(CloneData), "ApplyAppearance")]
    public static class CloneData_ApplyAppearance_Patch
    {
        public static void Postfix(Pawn pawn, CloneData __instance)
        {
            var dest = pawn;
            var source = __instance.donorPawn;
            if (source != null) 
            {
                CopyBodyAddons(source, dest);
                SetSkinColorFirst(dest, GetSkinColorFirst(source));
                SetSkinColorSecond(dest, GetSkinColorSecond(source));
                SetHairColorFirst(dest, GetHairColorFirst(source));
                SetHairColorSecond(dest, GetHairColorSecond(source));
            }
        }

        public static Color GetSkinColorFirst(Pawn pawn)
        {
            AlienRace.AlienPartGenerator.AlienComp alienComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(pawn);
            return alienComp != null ? alienComp.GetChannel("skin").first : Color.white;
        }

        public static Color GetSkinColorSecond(Pawn pawn)
        {
            AlienRace.AlienPartGenerator.AlienComp alienComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(pawn);
            return alienComp != null ? alienComp.GetChannel("skin").second : Color.white;
        }
        public static void SetSkinColorFirst(Pawn pawn, Color color)
        {
            AlienRace.AlienPartGenerator.AlienComp alienComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(pawn);
            if (alienComp != null)
            {
                alienComp.OverwriteColorChannel("skin", color, null);
            }
        }
        public static void SetSkinColorSecond(Pawn pawn, Color color)
        {
            AlienRace.AlienPartGenerator.AlienComp alienComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(pawn);
            if (alienComp != null)
            {
                alienComp.OverwriteColorChannel("skin", null, color);
            }
        }


        public static Color GetHairColorFirst(Pawn pawn)
        {
            AlienRace.AlienPartGenerator.AlienComp alienComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(pawn);
            return alienComp != null ? alienComp.GetChannel("hair").first : Color.white;
        }

        public static Color GetHairColorSecond(Pawn pawn)
        {
            AlienRace.AlienPartGenerator.AlienComp alienComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(pawn);
            return alienComp != null ? alienComp.GetChannel("hair").second : Color.white;
        }
        public static void SetHairColorFirst(Pawn pawn, Color color)
        {
            AlienRace.AlienPartGenerator.AlienComp alienComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(pawn);
            if (alienComp != null)
            {
                alienComp.OverwriteColorChannel("hair", color, null);
                pawn.story.HairColor = color;
            }
        }
        public static void SetHairColorSecond(Pawn pawn, Color color)
        {
            AlienRace.AlienPartGenerator.AlienComp alienComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(pawn);
            if (alienComp != null)
            {
                alienComp.OverwriteColorChannel("hair", null, color);
            }
        }

        public static void CopyBodyAddons(Pawn source, Pawn to)
        {
            AlienRace.AlienPartGenerator.AlienComp sourceComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(source);
            if (sourceComp != null && sourceComp.addonGraphics != null && sourceComp.addonVariants != null)
            {
                AlienRace.AlienPartGenerator.AlienComp toComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(to);
                if (toComp != null)
                {
                    toComp.addonGraphics = sourceComp.addonGraphics.ListFullCopy();
                    toComp.addonVariants = sourceComp.addonVariants.ListFullCopy();
                }
            }
        }
    }
}
