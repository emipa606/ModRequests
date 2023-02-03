using System;
using System.Collections.Generic;
using HarmonyLib;
using HugsLib;
using HugsLib.Settings;
using RimWorld;
using Verse;

namespace CensoredNudity
{
    public class CensoredNudity : ModBase
    {
        public static CensoredNudity Instance { get; private set; }

        public CensoredNudity()
        {
            Instance = this;
        }

        public static SettingHandle<CensorMode> censorMode { get; private set; }
        public static SettingHandle<CensorBasis> basisMaleHair { get; private set; }
        public static SettingHandle<CensorBasis> basisMaleFace { get; private set; }
        public static SettingHandle<CensorBasis> basisMaleChest { get; private set; }
        public static SettingHandle<CensorBasis> basisMaleGroin { get; private set; }
        public static SettingHandle<CensorBasis> basisFemaleHair { get; private set; }
        public static SettingHandle<CensorBasis> basisFemaleFace { get; private set; }
        public static SettingHandle<CensorBasis> basisFemaleChest { get; private set; }
        public static SettingHandle<CensorBasis> basisFemaleGroin { get; private set; }

        public override void DefsLoaded()
        {
            censorMode = Settings.GetHandle(
                "censorMode",
                "CensoredNudity_censorMode_title".Translate(),
                "CensoredNudity_censorMode_desc".Translate(),
                CensorMode.Mosaic,
                null,
                "CensoredNudity_CensorMode_"
            );

            bool validateIdeologySetting(string value)
            {
                return ModLister.IdeologyInstalled || AccessTools.Field(typeof(CensorBasis), value)?.IsDefined(typeof(MayRequireIdeologyAttribute), false) == false;
            }

            basisMaleHair = Settings.GetHandle(
                "basisMaleHair",
                "CensoredNudity_basisMaleHair_title".Translate(),
                "CensoredNudity_basisMaleHair_desc".Translate(),
                ModLister.IdeologyInstalled ? CensorBasis.PawnIdeoligion : CensorBasis.Never,
                validateIdeologySetting,
                "CensoredNudity_CensorBasis_"
            );
            basisMaleFace = Settings.GetHandle(
                "basisMaleFace",
                "CensoredNudity_basisMaleFace_title".Translate(),
                "CensoredNudity_basisMaleFace_desc".Translate(),
                ModLister.IdeologyInstalled ? CensorBasis.PawnIdeoligion : CensorBasis.Never,
                validateIdeologySetting,
                "CensoredNudity_CensorBasis_"
            );
            basisMaleChest = Settings.GetHandle(
                "basisMaleChest",
                "CensoredNudity_basisMaleChest_title".Translate(),
                "CensoredNudity_basisMaleChest_desc".Translate(),
                ModLister.IdeologyInstalled ? CensorBasis.PawnIdeoligion : CensorBasis.Never,
                validateIdeologySetting,
                "CensoredNudity_CensorBasis_"
            );
            basisMaleGroin = Settings.GetHandle(
                "basisMaleGroin",
                "CensoredNudity_basisMaleGroin_title".Translate(),
                "CensoredNudity_basisMaleGroin_desc".Translate(),
                ModLister.IdeologyInstalled ? CensorBasis.PawnIdeoligion : CensorBasis.Uncovered,
                validateIdeologySetting,
                "CensoredNudity_CensorBasis_"
            );

            basisFemaleHair = Settings.GetHandle(
                "basisFemaleHair",
                "CensoredNudity_basisFemaleHair_title".Translate(),
                "CensoredNudity_basisFemaleHair_desc".Translate(),
                ModLister.IdeologyInstalled ? CensorBasis.PawnIdeoligion : CensorBasis.Never,
                validateIdeologySetting,
                "CensoredNudity_CensorBasis_"
            );
            basisFemaleFace = Settings.GetHandle(
                "basisFemaleFace",
                "CensoredNudity_basisFemaleFace_title".Translate(),
                "CensoredNudity_basisFemaleFace_desc".Translate(),
                ModLister.IdeologyInstalled ? CensorBasis.PawnIdeoligion : CensorBasis.Never,
                validateIdeologySetting,
                "CensoredNudity_CensorBasis_"
            );
            basisFemaleChest = Settings.GetHandle(
                "basisFemaleChest",
                "CensoredNudity_basisFemaleChest_title".Translate(),
                "CensoredNudity_basisFemaleChest_desc".Translate(),
                ModLister.IdeologyInstalled ? CensorBasis.PawnIdeoligion : CensorBasis.Uncovered,
                validateIdeologySetting,
                "CensoredNudity_CensorBasis_"
            );
            basisFemaleGroin = Settings.GetHandle(
                "basisFemaleGroin",
                "CensoredNudity_basisFemaleGroin_title".Translate(),
                "CensoredNudity_basisFemaleGroin_desc".Translate(),
                ModLister.IdeologyInstalled ? CensorBasis.PawnIdeoligion : CensorBasis.Uncovered,
                validateIdeologySetting,
                "CensoredNudity_CensorBasis_"
            );
        }

        public static void ConvertIdeoBasis(Pawn pawn, ref CensorBasis basisHair, ref CensorBasis basisFace, ref CensorBasis basisChest, ref CensorBasis basisGroin)
        {
            part(ref basisHair, GetBasisHair);
            part(ref basisFace, GetBasisFace);
            part(ref basisChest, GetBasisChest);
            part(ref basisGroin, GetBasisGroin);

            void part(ref CensorBasis basis, Func<Ideo, Gender, CensorBasis> get)
            {
                if (basis == CensorBasis.PawnIdeoligion)
                {
                    basis = get(pawn.Ideo, pawn.gender);
                }
                else if (basis == CensorBasis.PlayerIdeoligion)
                {
                    basis = get(Faction.OfPlayer.ideos?.PrimaryIdeo, pawn.gender);
                }
            }
        }

        public static PreceptDef GetNudityPrecept(Ideo ideo, Gender gender)
        {
            if (ideo == null)
            {
                return null;
            }

            if (gender != Gender.Male && gender != Gender.Female)
            {
                return null;
            }

            var issue = DefDatabase<IssueDef>.GetNamed("Nudity_" + gender);

            foreach (var precept in ideo.PreceptsListForReading)
            {
                if (precept.def.issue == issue)
                {
                    return precept.def;
                }
            }

            return null;
        }

        private static readonly HashSet<string> unknownPrecepts = new HashSet<string>();

        private static void LogUnknownPrecept(string defName)
        {
            if (unknownPrecepts.Add(defName))
            {
                Log.Error($"[CensoredNudity] Unhandled nudity precept {defName} (the meanings of these are hard-coded; report this bug on the Steam Workshop page for Censored Nudity)");
            }
        }

        public static CensorBasis GetBasisHair(Ideo ideo, Gender gender)
        {
            var precept = GetNudityPrecept(ideo, gender);

            if (precept != null)
            {
                switch (precept.defName)
                {
                    case "Nudity_Male_Mandatory":
                    case "Nudity_Female_Mandatory":
                    case "Nudity_Male_CoveringAnythingButGroinDisapproved":
                    case "Nudity_Female_CoveringAnythingButGroinDisapproved":
                        return CensorBasis.Covered;
                    case "Nudity_Male_NoRules":
                    case "Nudity_Female_NoRules":
                    case "Nudity_Male_UncoveredGroinDisapproved":
                    case "Nudity_Female_UncoveredGroinDisapproved":
                    case "Nudity_Male_UncoveredGroinOrChestDisapproved":
                    case "Nudity_Female_UncoveredGroinOrChestDisapproved":
                        return CensorBasis.Never;
                    case "Nudity_Male_UncoveredGroinChestOrHairDisapproved":
                    case "Nudity_Female_UncoveredGroinChestOrHairDisapproved":
                    case "Nudity_Male_UncoveredGroinChestHairOrFaceDisapproved":
                    case "Nudity_Female_UncoveredGroinChestHairOrFaceDisapproved":
                        return CensorBasis.Uncovered;
                    default:
                        LogUnknownPrecept(precept.defName);
                        break;
                }
            }

            return CensorBasis.Never;
        }

        public static CensorBasis GetBasisFace(Ideo ideo, Gender gender)
        {
            var precept = GetNudityPrecept(ideo, gender);

            if (precept != null)
            {
                switch (precept.defName)
                {
                    case "Nudity_Male_Mandatory":
                    case "Nudity_Female_Mandatory":
                    case "Nudity_Male_CoveringAnythingButGroinDisapproved":
                    case "Nudity_Female_CoveringAnythingButGroinDisapproved":
                        return CensorBasis.Covered;
                    case "Nudity_Male_NoRules":
                    case "Nudity_Female_NoRules":
                    case "Nudity_Male_UncoveredGroinDisapproved":
                    case "Nudity_Female_UncoveredGroinDisapproved":
                    case "Nudity_Male_UncoveredGroinOrChestDisapproved":
                    case "Nudity_Female_UncoveredGroinOrChestDisapproved":
                    case "Nudity_Male_UncoveredGroinChestOrHairDisapproved":
                    case "Nudity_Female_UncoveredGroinChestOrHairDisapproved":
                        return CensorBasis.Never;
                    case "Nudity_Male_UncoveredGroinChestHairOrFaceDisapproved":
                    case "Nudity_Female_UncoveredGroinChestHairOrFaceDisapproved":
                        return CensorBasis.Uncovered;
                    default:
                        LogUnknownPrecept(precept.defName);
                        break;
                }
            }

            return CensorBasis.Never;
        }

        public static CensorBasis GetBasisChest(Ideo ideo, Gender gender)
        {
            var precept = GetNudityPrecept(ideo, gender);

            if (precept != null)
            {
                switch (precept.defName)
                {
                    case "Nudity_Male_Mandatory":
                    case "Nudity_Female_Mandatory":
                    case "Nudity_Male_CoveringAnythingButGroinDisapproved":
                    case "Nudity_Female_CoveringAnythingButGroinDisapproved":
                        return CensorBasis.Covered;
                    case "Nudity_Male_NoRules":
                    case "Nudity_Female_NoRules":
                    case "Nudity_Male_UncoveredGroinDisapproved":
                    case "Nudity_Female_UncoveredGroinDisapproved":
                        return CensorBasis.Never;
                    case "Nudity_Male_UncoveredGroinOrChestDisapproved":
                    case "Nudity_Female_UncoveredGroinOrChestDisapproved":
                    case "Nudity_Male_UncoveredGroinChestOrHairDisapproved":
                    case "Nudity_Female_UncoveredGroinChestOrHairDisapproved":
                    case "Nudity_Male_UncoveredGroinChestHairOrFaceDisapproved":
                    case "Nudity_Female_UncoveredGroinChestHairOrFaceDisapproved":
                        return CensorBasis.Uncovered;
                    default:
                        LogUnknownPrecept(precept.defName);
                        break;
                }
            }
            else if (ModLister.IdeologyInstalled)
            {
                return CensorBasis.Never;
            }

            // This says a lot about our society.
            return gender == Gender.Female ? CensorBasis.Uncovered : CensorBasis.Never;
        }

        public static CensorBasis GetBasisGroin(Ideo ideo, Gender gender)
        {
            var precept = GetNudityPrecept(ideo, gender);

            if (precept != null)
            {
                switch (precept.defName)
                {
                    case "Nudity_Male_Mandatory":
                    case "Nudity_Female_Mandatory":
                        return CensorBasis.Covered;
                    case "Nudity_Male_CoveringAnythingButGroinDisapproved":
                    case "Nudity_Female_CoveringAnythingButGroinDisapproved":
                        return CensorBasis.Uncovered;
                    case "Nudity_Male_NoRules":
                    case "Nudity_Female_NoRules":
                        return CensorBasis.Never;
                    case "Nudity_Male_UncoveredGroinDisapproved":
                    case "Nudity_Female_UncoveredGroinDisapproved":
                    case "Nudity_Male_UncoveredGroinOrChestDisapproved":
                    case "Nudity_Female_UncoveredGroinOrChestDisapproved":
                    case "Nudity_Male_UncoveredGroinChestOrHairDisapproved":
                    case "Nudity_Female_UncoveredGroinChestOrHairDisapproved":
                    case "Nudity_Male_UncoveredGroinChestHairOrFaceDisapproved":
                    case "Nudity_Female_UncoveredGroinChestHairOrFaceDisapproved":
                        return CensorBasis.Uncovered;
                    default:
                        LogUnknownPrecept(precept.defName);
                        break;
                }
            }
            else if (ModLister.IdeologyInstalled)
            {
                return CensorBasis.Never;
            }

            return CensorBasis.Uncovered;
        }

        public static void ShouldCensor(Pawn pawn, out bool censorHair, out bool censorFace, out bool censorChest, out bool censorGroin, bool forceNoHeadClothes = false, bool forceNoBodyClothes = false)
        {
            if (!pawn.RaceProps.Humanlike)
            {
                censorHair = false;
                censorFace = false;
                censorChest = false;
                censorGroin = false;

                return;
            }

            CensorBasis basisHair = CensorBasis.Never, basisFace = CensorBasis.Never, basisChest = CensorBasis.Never, basisGroin = CensorBasis.Never;

            switch (pawn.gender)
            {
                case Gender.Male:
                    basisHair = basisMaleHair;
                    basisFace = basisMaleFace;
                    basisChest = basisMaleChest;
                    basisGroin = basisMaleGroin;
                    break;
                case Gender.Female:
                    basisHair = basisFemaleHair;
                    basisFace = basisFemaleFace;
                    basisChest = basisFemaleChest;
                    basisGroin = basisFemaleGroin;
                    break;
            }

            ConvertIdeoBasis(pawn, ref basisHair, ref basisFace, ref basisChest, ref basisGroin);

            bool coveredHair = false, coveredFace = false, coveredChest = false, coveredGroin = false;

            if (pawn.apparel?.WornApparel != null)
            {
                foreach (var apparel in pawn.apparel.WornApparel)
                {
                    if (!apparel.def.apparel.countsAsClothingForNudity)
                    {
                        continue;
                    }

                    var kindRequires = pawn.kindDef.apparelRequired != null && pawn.kindDef.apparelRequired.Contains(apparel.def);

                    foreach (var part in apparel.def.apparel.bodyPartGroups)
                    {
                        if (part == BodyPartGroupDefOf.FullHead)
                        {
                            if (!kindRequires || basisHair != CensorBasis.Covered)
                                coveredHair = true;
                            if (!kindRequires || basisFace != CensorBasis.Covered)
                                coveredFace = true;
                        }
                        else if (part == BodyPartGroupDefOf.UpperHead)
                        {
                            if (!kindRequires || basisHair != CensorBasis.Covered)
                                coveredHair = true;
                        }
                        else if (part == BodyPartGroupDefOf.Eyes)
                        {
                            if (!kindRequires || basisFace != CensorBasis.Covered)
                                coveredFace = true;
                        }
                        else if (part == BodyPartGroupDefOf.Torso)
                        {
                            if (!kindRequires || basisChest != CensorBasis.Covered)
                                coveredChest = true;
                        }
                        else if (part == BodyPartGroupDefOf.Legs)
                        {
                            if (!kindRequires || basisGroin != CensorBasis.Covered)
                                coveredGroin = true;
                        }
                    }
                }
            }

            if (forceNoHeadClothes)
            {
                coveredHair = false;
                coveredFace = false;
            }

            if (forceNoBodyClothes)
            {
                coveredChest = false;
                coveredGroin = false;
            }

            censorHair = ShouldCensor(basisHair, coveredHair);
            censorFace = ShouldCensor(basisFace, coveredFace);
            censorChest = ShouldCensor(basisChest, coveredChest);
            censorGroin = ShouldCensor(basisGroin, coveredGroin);
        }

        public static bool ShouldCensor(CensorBasis basis, bool covered)
        {
            switch (basis)
            {
                case CensorBasis.Never:
                    return false;
                case CensorBasis.Always:
                    return true;
                case CensorBasis.Covered:
                    return covered;
                case CensorBasis.Uncovered:
                    return !covered;
                default:
                    return false;
            }
        }
    }
}
